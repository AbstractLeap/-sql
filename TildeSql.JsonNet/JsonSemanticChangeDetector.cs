namespace TildeSql.JsonNet {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.Json.Nodes;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    using TildeSql.Serialization;

    using JsonProperty = Newtonsoft.Json.Serialization.JsonProperty;
    using JsonSerializer = Newtonsoft.Json.JsonSerializer;

    /// <summary>
    ///     Object-graph-based semantic JSON change detector
    ///     Semantics:
    ///     - Declared nullability preserved 
    ///     - Strict semantic typing 
    ///     - Default collapsing: default(T) => null for non-nullable numerics/bools; T? never collapsed.
    ///     - Uses custom converters to check equality
    ///     - Missing JSON property ≡ null (non-nullables may default-collapse to null).
    ///     - Unknown JSON properties => change.
    ///     - Parses incoming JSON with DateParseHandling.None to avoid coercing date-like strings.
    /// </summary>
    public sealed class JsonSemanticChangeDetector : IChangeDetector {
        private readonly JsonSerializerSettings settings;

        public JsonSemanticChangeDetector(JsonSerializerSettings settings) {
            this.settings = settings ?? new JsonSerializerSettings();
        }

        public bool HasChanged(string json, object obj) {
            var serializer = JsonSerializer.Create(settings);
            var jsonToken = ParseJsonWithoutDateParsing(json);

            // Compare JSON token vs actual object graph
            return !SemanticEquals(jsonToken, dotnetValue: obj, declaredClrType: obj.GetType(), serializer: serializer, parentProperty: null);
        }

        // =====================================================================
        // JSON PARSING WITHOUT DATE COERCION
        // =====================================================================

        private static JToken ParseJsonWithoutDateParsing(string json) {
            using var sr = new StringReader(json);
            using var reader = new JsonTextReader(sr) { DateParseHandling = DateParseHandling.None };
            return JToken.ReadFrom(reader);
        }

        // =====================================================================
        // CORE RECURSIVE EQUALITY (Option B)
        // =====================================================================

        private static bool SemanticEquals(JToken json, object? dotnetValue, Type declaredClrType, JsonSerializer serializer, JsonProperty? parentProperty) {
            // Preserve declared nullability (N1)
            Type effectiveType = GetEffectiveRuntimeType(dotnetValue, declaredClrType);

            JsonContract runtimeContract = serializer.ContractResolver.ResolveContract(effectiveType);

            // Cache nullability/core type once per node
            var clr = runtimeContract.UnderlyingType;
            var underlying = Nullable.GetUnderlyingType(clr);
            bool isNullable = underlying != null;
            Type coreType = underlying ?? clr;

            // Normalize both sides (default collapsing with declared nullability)
            json        = NormalizeJsonDefault(json, isNullable, coreType);
            dotnetValue = NormalizeDotnetDefault(dotnetValue, isNullable, coreType);

            // Null-like equivalence
            if (IsNullLike(json) && dotnetValue == null)
                return true;

            // set a dotnet prop to null
            if (dotnetValue == null && !IsNullLike(json))
                return false;

            // Converter-aware short-circuit: skip property-level converters; allow type/serializer-level
            var convInfo = FindApplicableConverter(parentProperty, runtimeContract, coreType, serializer);
            if (convInfo is not null)
                return CompareViaConverter(json, dotnetValue, isNullable, coreType, serializer, convInfo);

            // Primitive
            if (runtimeContract is JsonPrimitiveContract prim)
                return ComparePrimitiveFast(json, dotnetValue, prim, serializer, isNullable, coreType);

            // Object
            if (runtimeContract is JsonObjectContract objC)
                return CompareObjectFast(json, dotnetValue!, objC, serializer);

            // Array
            if (runtimeContract is JsonArrayContract arrC)
                return CompareArrayFast(json, dotnetValue!, arrC, serializer);

            // Fallback (rare): let serializer produce expected token once
            return SerializerAwareDeepEquals.DeepEqualsNormalized(json, dotnetValue, serializer);
        }

        // =====================================================================
        // CONVERTER DETECTION 
        // =====================================================================

        private sealed record ConverterInfo(JsonConverter Converter, JsonProperty? Property);

        private static ConverterInfo? FindApplicableConverter(JsonProperty? parentProperty, JsonContract runtimeContract, Type coreType, JsonSerializer serializer) {
            // 1) Property-level converter has highest precedence
            if (parentProperty?.Converter != null)
                return new ConverterInfo(parentProperty.Converter, parentProperty);

            // 2) Type-level (contract) converter
            if (runtimeContract.Converter != null)
                return new ConverterInfo(runtimeContract.Converter, null);

            // 3) Serializer-level converters
            foreach (var conv in serializer.Converters)
                if (conv.CanConvert(coreType))
                    return new ConverterInfo(conv, null);

            return null;
        }

        private static bool CompareViaConverter(JToken json, object? dotnetValue, bool isNullable, Type coreType, JsonSerializer serializer, ConverterInfo convInfo) {
            if (dotnetValue == null)
                return IsNullLike(json); // already normalized earlier

            // Render the .NET value exactly as this converter would write it.
            var expected = WriteValueWithConverter(convInfo.Converter, dotnetValue, serializer);

            // Normalize
            expected = NormalizeJsonDefault(expected, isNullable, coreType);
            json     = NormalizeJsonDefault(json, isNullable, coreType);

            if (IsNullLike(expected) && IsNullLike(json))
                return true;

            if (IsNumeric(expected) && IsNumeric(json))
                return NumericEquals((JValue)expected, (JValue)json);

            // Check structural equality
            // with converters we lose the ability to do default value checking because we can't hook in to the resolution of contracts
            // so we just check for structural or string equality
            return JToken.DeepEquals(json, expected) || JsonSolidusEscapeIgnoringStringComparator.StringEquals(json.ToString(Formatting.None), expected.ToString(Formatting.None));
        }

        private static JToken WriteValueWithConverter(JsonConverter converter, object value, JsonSerializer serializer) {
            // Ask the converter to write the VALUE into a token writer.
            // NOTE: We do NOT wrap with an object/property name. Property converters write just the value.
            var jw = new JTokenWriter();
            converter.WriteJson(jw, value, serializer);

            // If a converter wrote nothing (rare), treat as null; otherwise return the root token it produced.
            return jw.Token ?? JValue.CreateNull();
        }

        // =====================================================================
        // PRIMITIVE COMPARISON (fast path + fallback)
        // =====================================================================

        private static bool ComparePrimitiveFast(JToken json, object? dotnetValue, JsonPrimitiveContract contract, JsonSerializer serializer, bool isNullable, Type coreType) {
            if (dotnetValue == null)
                return IsNullLike(json);

            // bool
            if (dotnetValue is bool b) {
                if (json.Type == JTokenType.Boolean)
                    return (bool)((JValue)json).Value! == b;
                return false;
            }

            // Guid (string or typed Guid token)
            if (dotnetValue is Guid g) {
                if (json.Type == JTokenType.Guid)
                    return (Guid)((JValue)json).Value! == g;

                if (json.Type == JTokenType.String)
                    return Guid.TryParse((string)json, out var gj) && gj == g;

                return false;
            }

            // DateTime (DT-2)
            if (dotnetValue is DateTime dt) {
                if (json.Type == JTokenType.Date) {
                    var jv = (JValue)json;
                    if (jv.Value is DateTime dj) return dj == dt;
                    if (jv.Value is DateTimeOffset) return false; // strict DT-2
                    return false;
                }

                if (json.Type == JTokenType.String) {
                    if (!DateTime.TryParse((string)json, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dj)) return false;
                    return dj == dt;
                }

                return false;
            }

            // DateTimeOffset (DT-2 exact)
            if (dotnetValue is DateTimeOffset dto) {
                if (json.Type == JTokenType.Date) {
                    var jv = (JValue)json;
                    if (jv.Value is DateTimeOffset djo) {
#if NET6_0_OR_GREATER
                        return dto.EqualsExact(djo);
#else
                    return dto.Ticks == djo.Ticks && dto.Offset == djo.Offset;
#endif
                    }

                    if (jv.Value is DateTime) return false; // strict DT-2
                    return false;
                }

                if (json.Type == JTokenType.String) {
                    if (!DateTimeOffset.TryParse((string)json, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var djoj)) return false;
#if NET6_0_OR_GREATER
                    return dto.EqualsExact(djoj);
#else
                return dto.Ticks == djoj.Ticks && dto.Offset == djoj.Offset;
#endif
                }

                return false;
            }

            // TimeSpan (typed or string)
            if (dotnetValue is TimeSpan ts) {
                if (json.Type == JTokenType.TimeSpan)
                    return (TimeSpan)((JValue)json).Value! == ts;

                if (json.Type == JTokenType.String)
                    return TimeSpan.TryParse((string)json, out var tjs) && tjs == ts;

                return false;
            }

            // Uri (typed or string)
            if (dotnetValue is Uri uri) {
                if (json.Type == JTokenType.Uri) {
                    var u = (Uri)((JValue)json).Value!;
                    return UriCompare(uri, u);
                }

                if (json.Type == JTokenType.String) {
                    if (!Uri.TryCreate((string)json, UriKind.RelativeOrAbsolute, out var u))
                        return false;
                    return UriCompare(uri, u);
                }

                return false;
            }

            // byte[] (B1 strict)
            if (dotnetValue is byte[] bytes) {
                if (json.Type != JTokenType.String) return false;
                try {
                    var decoded = Convert.FromBase64String((string)json);
#if NET6_0_OR_GREATER
                    return decoded.AsSpan().SequenceEqual(bytes);
#else
                if (decoded.Length != bytes.Length) return false;
                for (int i = 0; i < decoded.Length; i++)
                    if (decoded[i] != bytes[i]) return false;
                return true;
#endif
                }
                catch {
                    return false;
                }
            }

            // Numeric CLR family (no string parsing)
            if (IsNumericClr(coreType)) {
                if (json.Type == JTokenType.Integer) {
                    var j64 = Convert.ToInt64(((JValue)json).Value!, CultureInfo.InvariantCulture);
                    var v64 = Convert.ToInt64(dotnetValue, CultureInfo.InvariantCulture);
                    return j64 == v64;
                }

                if (json.Type == JTokenType.Float) {
                    try {
                        var jd = Convert.ToDecimal(((JValue)json).Value!, CultureInfo.InvariantCulture);
                        var vd = Convert.ToDecimal(dotnetValue, CultureInfo.InvariantCulture);
                        return jd == vd;
                    }
                    catch {
                        return false;
                    }
                }

                // If JSON is numeric string, we keep strict typing (no liberal parse here).
            }

            // Fallback: let serializer determine shape once
            var expected = JToken.FromObject(dotnetValue, serializer);
            if (IsNumeric(expected) && IsNumeric(json))
                return NumericEquals((JValue)expected, (JValue)json);

            return SerializerAwareDeepEquals.DeepEqualsNormalized(json, dotnetValue, serializer);
        }

        private static bool UriCompare(Uri a, Uri b) {
            return Uri.Compare(a, b, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.Ordinal) == 0;
        }

        private static bool IsNumericClr(Type t) {
            t = Nullable.GetUnderlyingType(t) ?? t;
            return t == typeof(byte)
                   || t == typeof(sbyte)
                   || t == typeof(short)
                   || t == typeof(ushort)
                   || t == typeof(int)
                   || t == typeof(uint)
                   || t == typeof(long)
                   || t == typeof(ulong)
                   || t == typeof(float)
                   || t == typeof(double)
                   || t == typeof(decimal);
        }

        // =====================================================================
        // OBJECT COMPARISON (fast)
        // =====================================================================

        private static bool CompareObjectFast(JToken json, object dotnetObj, JsonObjectContract contract, JsonSerializer serializer) {
            if (json is not JObject jsonObj)
                return false;

            // Contracted properties
            foreach (var prop in contract.Properties) {
                if (prop.Ignored || !prop.Readable) continue;

                string name = prop.PropertyName;

                bool hasJson = jsonObj.TryGetValue(name, out var jv);
                object? dv = prop.ValueProvider.GetValue(dotnetObj);

                if (hasJson) {
                    if (!SemanticEquals(jv!, dv, prop.PropertyType, serializer, prop))
                        return false;
                }
                else {
                    // Missing JSON => compare null vs dotnet value
                    if (!SemanticEquals(JValue.CreateNull(), dv, prop.PropertyType, serializer, prop))
                        return false;
                }
            }

            // Unknown JSON properties => change
            foreach (var jp in jsonObj.Properties()) {
                if (jp.Name == "$type" || jp.Name == "$ref") continue; // we can safely ignore metadata
                var jsonProp = contract.Properties.GetProperty(jp.Name, StringComparison.Ordinal);
                if (jsonProp == null || jsonProp.Ignored || !jsonProp.Readable)
                    return false;
            }

            return true;
        }

        // =====================================================================
        // ARRAY COMPARISON (single-pass, no temp list)
        // =====================================================================

        private static bool CompareArrayFast(JToken json, object dotnetObj, JsonArrayContract contract, JsonSerializer serializer) {
            if (json is not JArray jsonArr)
                return false;

            // Cheap count when available
            if (dotnetObj is ICollection coll) {
                if (jsonArr.Count != coll.Count) return false;
            }
            else if (dotnetObj is IReadOnlyCollection<object?> ro) {
                if (jsonArr.Count != ro.Count) return false;
            }
            else {
                int count = 0;
                foreach (var _ in (IEnumerable)dotnetObj) count++;
                if (jsonArr.Count != count) return false;
            }

            var it = ((IEnumerable)dotnetObj).GetEnumerator();
            for (int i = 0; i < jsonArr.Count; i++) {
                if (!it.MoveNext()) return false;
                var dotItem = it.Current;
                var declaredItem = contract.CollectionItemType ?? typeof(object);

                if (!SemanticEquals(jsonArr[i], dotItem, declaredItem, serializer, parentProperty: null))
                    return false;
            }

            return true;
        }

        // =====================================================================
        // DEFAULT NORMALIZATION (declared-type aware; fast)
        // =====================================================================

        private static JToken NormalizeJsonDefault(JToken t, bool isNullable, Type coreType) {
            // Early-out for non-values
            if (t.Type is not (JTokenType.Integer or JTokenType.Float or JTokenType.Boolean or JTokenType.String or JTokenType.Null or JTokenType.Undefined))
                return t;

            if (t is not JValue v)
                return t;

            var val = v.Value;
            if (val == null)
                return JValue.CreateNull();

            if (isNullable)
                return t;

            // collapse enums
            if (coreType.IsEnum) {
                // If JSON gave a number, collapse 0 → null
                if (t.Type == JTokenType.Integer) {
                    var num = Convert.ToInt64(v.Value!, CultureInfo.InvariantCulture);
                    if (num == 0) return JValue.CreateNull();
                    return t;
                }

                // If JSON gave a string, collapse when it resolves to underlying 0
                if (t.Type == JTokenType.String) {
                    var s = (string)v.Value!;
                    // Try by name
                    if (Enum.TryParse(coreType, s, ignoreCase: false, out var byName)) {
                        if (Convert.ToInt64(byName, CultureInfo.InvariantCulture) == 0)
                            return JValue.CreateNull();
                        return t;
                    }

                    // Try numeric-in-string
                    if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numStr) && numStr == 0)
                        return JValue.CreateNull();

                    return t;
                }

                // Any other token type for enums: don’t collapse
                return t;
            }
            // ---------------------------------------------------------------

            // Collapse only non-nullable numerics/bools
            return val switch {
                int i when i == 0 => JValue.CreateNull(),
                long l when l == 0L => JValue.CreateNull(),
                double d when d == 0d => JValue.CreateNull(),
                float f when f == 0f => JValue.CreateNull(),
                decimal m when m == 0m => JValue.CreateNull(),
                bool b when !b => JValue.CreateNull(),
                _ => t
            };
        }

        private static object? NormalizeDotnetDefault(object? value, bool isNullable, Type coreType) {
            if (value == null)
                return null;

            if (isNullable)
                return value;

            // collapse enums
            if (coreType.IsEnum) {
                var num = Convert.ToInt64(value, CultureInfo.InvariantCulture);
                if (num == 0) return null;
                return value;
            }

            // Collapse non-nullable numerics/bools
            if (coreType == typeof(int) && (int)value == 0) return null;
            if (coreType == typeof(long) && (long)value == 0L) return null;
            if (coreType == typeof(float) && (float)value == 0f) return null;
            if (coreType == typeof(double) && (double)value == 0d) return null;
            if (coreType == typeof(decimal) && (decimal)value == 0m) return null;
            if (coreType == typeof(bool) && (bool)value == false) return null;

            // Do NOT collapse empty string
            return value;
        }

        // =====================================================================
        // NUMERIC SEMANTICS (no strings)
        // =====================================================================

        private static bool NumericEquals(JValue a, JValue b) {
            if (a.Type == JTokenType.Integer && b.Type == JTokenType.Integer) {
                long ai = Convert.ToInt64(a.Value!, CultureInfo.InvariantCulture);
                long bi = Convert.ToInt64(b.Value!, CultureInfo.InvariantCulture);
                return ai == bi;
            }

            try {
                var da = Convert.ToDecimal(a.Value!, CultureInfo.InvariantCulture);
                var db = Convert.ToDecimal(b.Value!, CultureInfo.InvariantCulture);
                return da == db;
            }
            catch { return false; }
        }

        private static bool IsNumeric(JToken t)
            => t.Type == JTokenType.Integer || t.Type == JTokenType.Float;

        private static bool IsNullLike(JToken t)
            => t.Type == JTokenType.Null || t.Type == JTokenType.Undefined;

        // =====================================================================
        // RUNTIME TYPE (N1)
        // =====================================================================

        private static Type GetEffectiveRuntimeType(object? dotnetValue, Type declared) {
            var underlying = Nullable.GetUnderlyingType(declared);
            if (underlying == null)
                return dotnetValue?.GetType() ?? declared;

            // Declared is Nullable<T>
            if (dotnetValue == null)
                return declared;

            // Value present but preserve nullability wrapper
            return declared;
        }
    }

    internal static class SerializerAwareDeepEquals {
        /// <summary>
        ///     Fast-path + serializer-aware comparison.
        ///     1) If tokens are already equal, return true.
        ///     2) If neither contains non-JSON primitives, return false (strict diff).
        ///     3) Otherwise, normalize only the side(s) that need it and compare.
        /// </summary>
        public static bool DeepEqualsNormalized(JToken json, object dotnetValue, JsonSerializer serializer) {
            var expectedJToken = JToken.FromObject(dotnetValue!, serializer);

            // Fast path: already equal
            if (JToken.DeepEquals(json, expectedJToken))
                return true;

            var jsonHasNonJsonPrimitives = ContainsNonJsonPrimitives(json);
            var dotnetContainsNonJsonPrimitives = ContainsNonJsonPrimitives(expectedJToken);

            // Fast path: both are purely native JSON → already not equal
            if (!jsonHasNonJsonPrimitives && !dotnetContainsNonJsonPrimitives)
                return false;

            // serialize out to text, then back and compare JTokens
            var standardizedDotnet = SerializeAsLiteralToken(dotnetValue, serializer);
            return JToken.DeepEquals(json, standardizedDotnet);
        }

        /// <summary>
        ///     Serialize a value using the provided Newtonsoft.Json serializer
        ///     into JSON TEXT, then re-materialize a JToken with literal semantics:
        ///     - If the value is a string JSON literal --> return JValue(string).
        ///     - Otherwise --> parse via System.Text.Json and convert to JToken (no magic).
        ///     This ensures Guid/Date/Uri/TimeSpan/etc. become whatever TEXT your serializer would output,
        ///     and the resulting JToken reflects that literal JSON representation.
        /// </summary>
        private static JToken SerializeAsLiteralToken(object? dotnetValue, JsonSerializer serializer) {
            // 1) Serialize to TEXT to capture exact JSON output of your serializer (converters & settings included).
            string jsonText;
            var sb = new StringBuilder(128);
            using (var sw = new StringWriter(sb))
            using (var jw = new JsonTextWriter(sw)) {
                // Optional: you can propagate formatting/indent chars if needed:
                jw.Formatting = serializer.Formatting;
                serializer.Serialize(jw, dotnetValue);
                jw.Flush();
                jsonText = sw.ToString();
            }

            // 2) If it's a JSON string literal, avoid re-parsing—return a JValue(string) directly.
            // JSON strings always start with a double-quote (after whitespace).
            int i = 0;
            while (i < jsonText.Length && char.IsWhiteSpace(jsonText[i])) i++;
            if (i < jsonText.Length && jsonText[i] == '"') {
                // Parse the JSON string literal to a .NET string with System.Text.Json (reliable unescape).
                var strNode = JsonNode.Parse(jsonText);
                var asString = strNode!.GetValue<string>();
                return JValue.CreateString(asString);
            }

            return JToken.Parse(jsonText);
        }

        /// <summary>
        ///     Returns true if any subtree contains a non-native JSON JValue:
        ///     anything other than String, Integer, Float, Boolean, Null, Undefined.
        /// </summary>
        public static bool ContainsNonJsonPrimitives(JToken token) {
            var stack = new Stack<JToken>();
            stack.Push(token);

            while (stack.Count > 0) {
                var t = stack.Pop();

                if (t is JValue v) {
                    if (v.Type is not (JTokenType.String or JTokenType.Integer or JTokenType.Float or JTokenType.Boolean or JTokenType.Null or JTokenType.Undefined)) {
                        // Non-JSON-native primitive encountered (Guid/Date/Uri/TimeSpan/Bytes/etc.)
                        return true;
                    }

                    continue;
                }

                foreach (var child in t.Children())
                    stack.Push(child);
            }

            return false;
        }

        /// <summary>
        ///     Copy-on-write normalization:
        ///     - Returns the original token if no changes are needed (same reference).
        ///     - Rebuilds only the branches that contain non-JSON primitives.
        ///     - Non-native primitives are serialized using the provided serializer,
        ///     producing exactly the representation your API would output.
        /// </summary>
        public static JToken NormalizeIfNeeded(JToken token, JsonSerializer serializer, Dictionary<JToken, JToken>? cache = null) {
            cache ??= new Dictionary<JToken, JToken>(ReferenceEqualityComparer<JToken>.Instance);

            if (cache.TryGetValue(token, out var cached))
                return cached;

            JToken result;

            switch (token) {
                case JObject obj: {
                    bool changed = false;
                    var newProps = new List<JProperty>(capacity: obj.Count);

                    foreach (var p in obj.Properties()) {
                        var norm = NormalizeIfNeeded(p.Value, serializer, cache);
                        if (!ReferenceEquals(norm, p.Value))
                            changed = true;
                        newProps.Add(new JProperty(p.Name, norm));
                    }

                    result = changed ? new JObject(newProps) : obj;
                    break;
                }

                case JArray arr: {
                    bool changed = false;
                    var newItems = new List<JToken>(capacity: arr.Count);

                    for (int i = 0; i < arr.Count; i++) {
                        var norm = NormalizeIfNeeded(arr[i], serializer, cache);
                        if (!ReferenceEquals(norm, arr[i]))
                            changed = true;
                        newItems.Add(norm);
                    }

                    result = changed ? new JArray(newItems) : arr;
                    break;
                }

                case JValue v: {
                    if (v.Type is JTokenType.String or JTokenType.Integer or JTokenType.Float or JTokenType.Boolean or JTokenType.Null or JTokenType.Undefined) {
                        // Native JSON primitive → unchanged
                        result = v;
                        break;
                    }

                    // Non-JSON primitive → serialize with serializer to canonical JSON token
                    // This respects converters, culture, formatting, etc.
                    result = SerializeAsToken(v.Value, serializer);
                    break;
                }

                default:
                    // Rare token kinds (e.g., raw) → round-trip through serializer
                    result = SerializeAsToken(token, serializer);
                    break;
            }

            cache[token] = result;
            return result;
        }

        private static JToken SerializeAsToken(object? dotnetValue, JsonSerializer serializer) {
            var writer = new JTokenWriter();
            serializer.Serialize(writer, dotnetValue);
            return writer.Token ?? JValue.CreateNull();
        }
    }

    /// <summary>
    ///     Reference equality comparer for using JToken as dictionary keys without
    ///     relying on structural equality/hash semantics.
    /// </summary>
    internal sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
        where T : class {
        public static readonly ReferenceEqualityComparer<T> Instance = new();

        private ReferenceEqualityComparer() { }

        public bool Equals(T? x, T? y) => ReferenceEquals(x, y);

        public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }
}