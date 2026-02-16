namespace TildeSql.JsonNet {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    ///     Detects whether a JSON payload has semantically changed compared to a .NET object,
    ///     using Newtonsoft.Json contracts and normalised value comparison.
    ///     This comparison is structural rather than textual:
    ///     - Object property order is ignored.
    ///     - Missing properties are treated as equivalent to null.
    ///     - Default values for non-nullable types (e.g. 0, false) are treated as null,
    ///     allowing "missing", "null", and default(T) to be considered equal.
    ///     - Nullable value types (e.g. int?, bool?) are never normalised, so their
    ///     default values remain meaningful and do NOT collapse to null.
    ///     - Arrays are compared by length and element order.
    ///     - Numeric values are compared semantically (e.g. 1, 1.0, and 1.00 are equal).
    ///     - JsonProperty attributes, converters, and naming strategies are respected
    ///     through the configured ContractResolver.
    ///     The detector returns true when the JSON and object differ in any meaningful way.
    /// </summary>
    public class JsonSemanticChangeDetector : IChangeDetector {
        private readonly JsonSerializerSettings serializerSettings;

        public JsonSemanticChangeDetector(JsonSerializerSettings serializerSettings) {
            this.serializerSettings = serializerSettings;
        }

        public bool HasChanged(string json, object obj) {
            var serializer = JsonSerializer.Create(this.serializerSettings);

            using var sr = new StringReader(json);
            using var reader = new JsonTextReader(sr) { DateParseHandling = DateParseHandling.None };

            var jsonToken = JToken.ReadFrom(reader);

            return !SemanticEquals(jsonToken, dotnetValue: obj, declaredClrType: obj.GetType(), serializer: serializer);
        }

        // =====================================================================
        // CORE SEMANTIC EQUALITY
        // =====================================================================

        private static bool SemanticEquals(JToken json, object? dotnetValue, Type declaredClrType, JsonSerializer serializer) {
            // Determine actual runtime type
            Type effectiveType = GetEffectiveRuntimeType(dotnetValue, declaredClrType);
            var runtimeContract = serializer.ContractResolver.ResolveContract(effectiveType);

            var clr = runtimeContract.UnderlyingType;
            var underlying = Nullable.GetUnderlyingType(clr);
            bool isNullable = underlying != null;
            Type coreType = underlying ?? clr;

            // ------------------------------------------------------------
            // 1. Normalize both sides (runtime-type-aware)
            // ------------------------------------------------------------
            json        = NormalizeJsonDefault(json, isNullable, coreType);
            dotnetValue = NormalizeDotnetDefault(dotnetValue, isNullable, coreType);

            // ------------------------------------------------------------
            // 2. Null-like equivalence
            // ------------------------------------------------------------
            if (IsNullLike(json) && dotnetValue == null)
                return true;

            // ------------------------------------------------------------
            // 3. Property has custom converter
            // ------------------------------------------------------------
            var contractualConverter = runtimeContract.Converter ?? runtimeContract.InternalConverter;
            if (contractualConverter != null)
                return CompareViaConverter(json, dotnetValue, runtimeContract, serializer, isNullable, coreType);

            var converter = FindMatchingConverter(coreType, serializer);
            if (converter != null)
                return CompareViaConverter(json, dotnetValue, runtimeContract, serializer, isNullable, coreType);

            // ------------------------------------------------------------
            // 4. Primitive contract
            // ------------------------------------------------------------
            if (runtimeContract is JsonPrimitiveContract primC)
                return ComparePrimitive(json, dotnetValue, primC, serializer, isNullable, coreType);

            // ------------------------------------------------------------
            // 5. Object
            // ------------------------------------------------------------
            if (runtimeContract is JsonObjectContract objC)
                return CompareObject(json, dotnetValue!, objC, serializer);

            // ------------------------------------------------------------
            // 6. Array
            // ------------------------------------------------------------
            if (runtimeContract is JsonArrayContract arrC)
                return CompareArray(json, dotnetValue!, arrC, serializer);

            // ------------------------------------------------------------
            // 7. Fallback - compare serialized token
            // ------------------------------------------------------------
            var expectedToken = SerializeToToken(dotnetValue, serializer);
            return JToken.DeepEquals(json, expectedToken);
        }

        // =====================================================================
        // PRIMITIVE COMPARISON
        // =====================================================================

        private static bool ComparePrimitive(JToken json, object? dotnetValue, JsonPrimitiveContract contract, JsonSerializer serializer, bool isNullable, Type coreType) {
            if (dotnetValue == null)
                return IsNullLike(json);

            // bool / bool?
            if (dotnetValue is bool b) {
                if (json.Type == JTokenType.Boolean)
                    return (bool)((JValue)json).Value! == b;
                return false;
            }

            // GUID / GUID?
            if (dotnetValue is Guid g) {
                if (json.Type == JTokenType.Guid)
                    return (Guid)((JValue)json).Value! == g;

                if (json.Type == JTokenType.String)
                    return Guid.TryParse((string)json, out var gj) && gj == g;

                return false;
            }

            // DateTime (DT-2: exact equality)
            if (dotnetValue is DateTime dt) {
                if (json.Type == JTokenType.String) {
                    if (!DateTime.TryParse((string)json, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dj)) return false;
                    return dj == dt; // exact, no normalization
                }

                if (json.Type == JTokenType.Date) {
                    // Json.NET JValue.Value may be DateTime or DateTimeOffset depending on DateParseHandling
                    var jv = (JValue)json;
                    if (jv.Value is DateTime dj) return dj == dt;
                    if (jv.Value is DateTimeOffset djo) return false; // strict DT‑2: different type semantics
                    return false;
                }

                return false;
            }

            // DateTimeOffset (DT-2: exact equality; no instant normalization)
            if (dotnetValue is DateTimeOffset dto) {
                if (json.Type == JTokenType.String) {
                    if (!DateTimeOffset.TryParse((string)json, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var djoj)) return false;
                    return dto.EqualsExact(djoj);
                }

                if (json.Type == JTokenType.Date) {
                    var jv = (JValue)json;
                    if (jv.Value is DateTimeOffset djo) {
                        return dto.EqualsExact(djo);
                    }

                    // If parser produced DateTime instead of DateTimeOffset, we treat it as a mismatch under DT-2.
                    // If you prefer to "upgrade" DateTime to DTO by assuming local/UTC, that would contradict DT-2.
                    if (jv.Value is DateTime) return false;

                    return false;
                }

                return false;
            }

            // TimeSpan (exact)
            if (dotnetValue is TimeSpan ts) {
                if (json.Type == JTokenType.TimeSpan)
                    return (TimeSpan)((JValue)json).Value! == ts;

                if (json.Type == JTokenType.String)
                    return TimeSpan.TryParse((string)json, out var tjs) && tjs == ts;

                return false;
            }

            // Uri (exact abs-uri compare)
            if (dotnetValue is Uri uri) {
                if (json.Type == JTokenType.Uri) {
                    var u = (Uri)((JValue)json).Value!;
                    return Uri.Compare(uri, u, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.Ordinal) == 0;
                }

                if (json.Type == JTokenType.String) {
                    if (!Uri.TryCreate((string)json, UriKind.RelativeOrAbsolute, out var u))
                        return false;

                    return Uri.Compare(uri, u, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.Ordinal) == 0;
                }

                return false;
            }

            // byte[] (B1: strict base64)
            if (dotnetValue is byte[] bytes) {
                if (json.Type != JTokenType.String) return false;
                try {
                    var decoded = Convert.FromBase64String((string)json);
                    return decoded.AsSpan().SequenceEqual(bytes);
                }
                catch {
                    return false;
                }
            }

            // Numeric family (covers all integral + floating types via Convert)
            if (IsNumericClr(coreType)) {
                if (json.Type == JTokenType.Integer) {
                    // Fast integral path
                    var jsonInt64 = Convert.ToInt64(((JValue)json).Value!, CultureInfo.InvariantCulture);
                    var dotInt64 = Convert.ToInt64(dotnetValue, CultureInfo.InvariantCulture);
                    return jsonInt64 == dotInt64;
                }

                if (json.Type == JTokenType.Float) {
                    // Float/decimal path with no strings
                    try {
                        var jsonDec = Convert.ToDecimal(((JValue)json).Value!, CultureInfo.InvariantCulture);
                        var dotDec = Convert.ToDecimal(dotnetValue, CultureInfo.InvariantCulture);
                        return jsonDec == dotDec;
                    }
                    catch {
                        return false;
                    }
                }

                // If JSON is numeric string (rare), we still keep strict typing by falling back
                // to serializer form below (no liberal parsing here).
            }

            // Serialize .NET primitive → parse → JToken
            var expected = JToken.FromObject(dotnetValue, serializer);

            // Normalize both sides
            expected = NormalizeJsonDefault(expected, isNullable, coreType);
            json     = NormalizeJsonDefault(json, isNullable, coreType);

            // Null-like?
            if (IsNullLike(expected) && IsNullLike(json))
                return true;

            // Numeric semantics
            if (IsNumeric(expected) && IsNumeric(json))
                return NumericEquals((JValue)expected, (JValue)json);

            // Strict structural
            return JToken.DeepEquals(expected, json);
        }

        // =====================================================================
        // CONVERTER COMPARISON 
        // =====================================================================

        private static bool CompareViaConverter(JToken json, object? dotnetValue, JsonContract runtimeContract, JsonSerializer serializer, bool isNullable, Type coreType) {
            // If .NET value is null: we already short-circuited null-like above; if here, JSON is not null-like.
            if (dotnetValue == null) return false;

            // Delegate shape to the converter by going through the serializer once:
            // This yields exactly what your converter would write for this node.
            JToken expected = JToken.FromObject(dotnetValue, serializer);

            // Apply the same default normalization (non-nullable default => null)
            expected = NormalizeJsonDefault(expected, isNullable, coreType);
            json     = NormalizeJsonDefault(json, isNullable, coreType);

            // Null-like after normalization → equal
            if (IsNullLike(expected) && IsNullLike(json))
                return true;

            // Numeric semantics if both numeric (covers converters that emit numbers)
            if (IsNumeric(expected) && IsNumeric(json))
                return NumericEquals((JValue)expected, (JValue)json);

            // Final structural compare (string vs string, object vs object, etc.)
            return JToken.DeepEquals(expected, json);
        }

        private static JsonConverter? FindMatchingConverter(Type runtimeType, JsonSerializer serializer) {
            foreach (var conv in serializer.Converters) {
                if (conv.CanConvert(runtimeType))
                    return conv;
            }

            return null;
        }

        // =====================================================================
        // OBJECT COMPARISON (object graph traversal)
        // =====================================================================

        private static bool CompareObject(JToken json, object dotnetObj, JsonObjectContract contract, JsonSerializer serializer) {
            if (json is not JObject jsonObj)
                return false;

            var objPropertyNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var prop in contract.Properties) {
                if (prop.Ignored || !prop.Readable)
                    continue;

                string jsonName = prop.PropertyName;
                objPropertyNames.Add(jsonName);

                bool hasJson = jsonObj.TryGetValue(jsonName, out var jsonValue);
                object? dotValue = prop.ValueProvider.GetValue(dotnetObj);
                Type childDeclared = prop.PropertyType;

                if (hasJson) {
                    if (!SemanticEquals(jsonValue!, dotValue, childDeclared, serializer))
                        return false;
                }
                else {
                    // Missing JSON property → treat as null
                    if (!SemanticEquals(JValue.CreateNull(), dotValue, childDeclared, serializer))
                        return false;
                }
            }

            // find json properties that are missing in .net obj
            foreach (var jsonProp in jsonObj.Properties()) {
                var name = jsonProp.Name;
                if (objPropertyNames.Contains(name)) continue;
                if (IsMetadataProperty(name)) continue;

                if (!IsNullLike(jsonProp.Value)) 
                    return false;
            }

            return true;
        }

        // =====================================================================
        // ARRAY COMPARISON
        // =====================================================================

        private static bool CompareArray(JToken json, object dotnetObj, JsonArrayContract contract, JsonSerializer serializer) {
            if (json is not JArray jsonArr)
                return false;

            // Cheap count when available
            if (dotnetObj is ICollection coll) {
                if (jsonArr.Count != coll.Count)
                    return false;
            }
            else if (dotnetObj is IReadOnlyCollection<object?> roColl) {
                if (jsonArr.Count != roColl.Count)
                    return false;
            }
            else {
                int count = 0;
                foreach (var _ in (IEnumerable)dotnetObj) count++;
                if (jsonArr.Count != count)
                    return false;
            }

            var it = ((IEnumerable)dotnetObj).GetEnumerator();
            for (int i = 0; i < jsonArr.Count; i++) {
                if (!it.MoveNext()) return false; // should not happen after count check

                var dotItem = it.Current;
                var declaredItem = contract.CollectionItemType ?? typeof(object);

                if (!SemanticEquals(jsonArr[i], dotItem, declaredItem, serializer))
                    return false;
            }

            return true;
        }

        // =====================================================================
        // NUMERIC SEMANTICS
        // =====================================================================

        private static bool NumericEquals(JValue a, JValue b) {
            // Fast integral path
            if (a.Type == JTokenType.Integer && b.Type == JTokenType.Integer) {
                long ai = Convert.ToInt64(a.Value!, CultureInfo.InvariantCulture);
                long bi = Convert.ToInt64(b.Value!, CultureInfo.InvariantCulture);
                return ai == bi;
            }

            // General numeric path (decimal for equality semantics)
            try {
                var da = Convert.ToDecimal(a.Value!, CultureInfo.InvariantCulture);
                var db = Convert.ToDecimal(b.Value!, CultureInfo.InvariantCulture);
                return da == db;
            }
            catch {
                return false;
            }
        }

        private static bool IsNumeric(JToken t) => t.Type == JTokenType.Integer || t.Type == JTokenType.Float;

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
        // DEFAULT NORMALIZATION
        // =====================================================================

        private static JToken NormalizeJsonDefault(JToken token, bool isNullable, Type coreType) {
            if (token.Type is not (JTokenType.Integer or JTokenType.Float or JTokenType.Boolean or JTokenType.String or JTokenType.Null or JTokenType.Undefined))
                return token;

            if (token is not JValue v)
                return token;

            var value = v.Value;
            if (value == null)
                return JValue.CreateNull();

            if (isNullable)
                return token;

            return value switch {
                int i when i == 0 => JValue.CreateNull(),
                long l when l == 0L => JValue.CreateNull(),
                double d when d == 0d => JValue.CreateNull(),
                float f when f == 0f => JValue.CreateNull(),
                decimal m when m == 0m => JValue.CreateNull(),
                bool b when !b => JValue.CreateNull(),
                _ => token
            };
        }

        private static object? NormalizeDotnetDefault(object? value, bool isNullable, Type coreType) {
            if (value == null)
                return null;

            if (isNullable)
                return value;

            if (coreType == typeof(int) && (int)value == 0) return null;
            if (coreType == typeof(long) && (long)value == 0L) return null;
            if (coreType == typeof(float) && (float)value == 0f) return null;
            if (coreType == typeof(double) && (double)value == 0d) return null;
            if (coreType == typeof(decimal) && (decimal)value == 0m) return null;
            if (coreType == typeof(bool) && !(bool)value) return null;

            return value;
        }

        // =====================================================================
        // UTILITIES
        // =====================================================================

        private static Type GetEffectiveRuntimeType(object? dotnetValue, Type declared) {
            // Case 1: declared is not nullable<T>
            var underlying = Nullable.GetUnderlyingType(declared);
            if (underlying == null)
                return dotnetValue?.GetType() ?? declared;

            // Case 2: declared is Nullable<T>
            if (dotnetValue == null)
                return declared; // remain nullable<T>

            // dotnetValue is boxed T → preserve nullable wrapper
            return declared;
        }

        private static bool IsNullLike(JToken t) => t.Type == JTokenType.Null || t.Type == JTokenType.Undefined;

        private static JToken SerializeToToken(object? value, JsonSerializer serializer) {
            using var sw = new StringWriter(CultureInfo.InvariantCulture);
            using var writer = new JsonTextWriter(sw);
            serializer.Serialize(writer, value);

            string json = sw.ToString();
            return JToken.Parse(json);
        }

        private static readonly HashSet<string> MetadataProps = new(StringComparer.Ordinal)
        {
            "$type", "$id", "$ref", "$values"
            // You can widen this to: treat all $-prefixed keys as metadata
            // by replacing the predicate with: name => name.Length > 0 && name[0] == '$'
        };

        private static bool IsMetadataProperty(string name)
            => MetadataProps.Contains(name);
    }
}