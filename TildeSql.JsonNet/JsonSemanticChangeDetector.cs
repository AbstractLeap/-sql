namespace TildeSql.JsonNet {
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    using System;
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    /// Detects whether a JSON payload has semantically changed compared to a .NET object,
    /// using Newtonsoft.Json contracts and normalised value comparison.
    /// 
    /// This comparison is structural rather than textual:
    /// - Object property order is ignored.
    /// - Missing properties are treated as equivalent to null.
    /// - Default values for non-nullable types (e.g. 0, false) are treated as null,
    ///   allowing "missing", "null", and default(T) to be considered equal.
    /// - Nullable value types (e.g. int?, bool?) are never normalised, so their
    ///   default values remain meaningful and do NOT collapse to null.
    /// - Arrays are compared by length and element order.
    /// - Numeric values are compared semantically (e.g. 1, 1.0, and 1.00 are equal).
    /// - JsonProperty attributes, converters, and naming strategies are respected
    ///   through the configured ContractResolver.
    /// 
    /// The detector returns true when the JSON and object differ in any meaningful way.
    /// </summary>
    public class JsonSemanticChangeDetector : IChangeDetector {
        private readonly JsonSerializerSettings serializerSettings;

        public JsonSemanticChangeDetector(JsonSerializerSettings serializerSettings) {
            this.serializerSettings = serializerSettings;
        }

        public bool HasChanged(string json, object obj) {
            var serializer = JsonSerializer.Create(this.serializerSettings ?? new JsonSerializerSettings());
            var resolver = serializer.ContractResolver;

            JToken jsonToken = JToken.Parse(json);
            JToken objToken = JToken.FromObject(obj, serializer);

            // We pass the root contract type into comparison
            var declaredContract = resolver.ResolveContract(obj.GetType());

            return !JTokenSemanticEquals(jsonToken, objToken, declaredContract, resolver, serializer);
        }


        // ------------------------------------------------------------------------
        // TOP LEVEL COMPARISON
        // ------------------------------------------------------------------------
        private static bool JTokenSemanticEquals(
            JToken json,
            JToken dotnet,
            JsonContract? declaredContract,
            IContractResolver resolver, JsonSerializer serializer) {
            if (ReferenceEquals(json, dotnet))
                return true;
            if (json is null || dotnet is null)
                return false;

            // Normalize defaults using the declared CLR type 
            json = NormalizeDefault(json, declaredContract);
            dotnet = NormalizeDefault(dotnet, declaredContract);
            
            // Both null like
            if (IsNullLike(json) && IsNullLike(dotnet))
                return true;

            if (declaredContract is JsonPrimitiveContract prim) {
                // dotnet must be a JValue at this point; extract the CLR value
                object? dotnetValue = (dotnet as JValue)?.Value;

                // If the dotnet side is null-like and JSON is null-like, already handled above.
                if (dotnetValue is null)
                    return false; // one side was not null-like; difference

                // Serialize the dotnet primitive as Json.NET would write it, parse into JToken
                var expected = SerializeValueToToken(dotnetValue, serializer);

                // Apply the same default normalization to both
                expected = NormalizeDefault(expected, declaredContract);
                json     = NormalizeDefault(json, declaredContract);

                // Null-like equivalence (after normalization)
                if (IsNullLike(expected) && IsNullLike(json))
                    return true;

                // Numeric semantics: if both are numeric, compare numerically
                if (IsNumeric(expected) && IsNumeric(json))
                    return NumericEquals((JValue)expected, (JValue)json);

                // Otherwise, rely on structural equality
                return JToken.DeepEquals(expected, json);
            }

            // Numeric types are semantically equal even across integer/float mismatch
            if (IsNumeric(json) && IsNumeric(dotnet))
                return NumericEquals((JValue)json, (JValue)dotnet);

            if (json.Type != dotnet.Type) {
                // Only OK if both are null-like (null/missing/default)
                return IsNullLike(json) && IsNullLike(dotnet);
            }

            return json.Type switch {
                JTokenType.Object => JObjectEquals((JObject)json, (JObject)dotnet, declaredContract, resolver, serializer),
                JTokenType.Array => JArrayEquals((JArray)json, (JArray)dotnet, declaredContract, resolver, serializer),
                JTokenType.String => string.Equals((string)((JValue)json).Value, (string)((JValue)dotnet).Value, StringComparison.Ordinal),
                JTokenType.Boolean => (bool)((JValue)json).Value! == (bool)((JValue)dotnet).Value!,
                JTokenType.Null or JTokenType.Undefined => true,
                _ => JToken.DeepEquals(json, dotnet)
            };
        }


        // ------------------------------------------------------------------------
        // OBJECT COMPARISON
        // ------------------------------------------------------------------------
        private static bool JObjectEquals(
            JObject json,
            JObject dotnet,
            JsonContract? declaredContract,
            IContractResolver resolver, JsonSerializer serializer) {
            var objContract = declaredContract as JsonObjectContract;

            var allKeys = new HashSet<string>(StringComparer.Ordinal);

            foreach (var p in json.Properties()) allKeys.Add(p.Name);
            foreach (var p in dotnet.Properties()) allKeys.Add(p.Name);

            foreach (var key in allKeys) {
                bool hasJson = json.TryGetValue(key, out var valueJson);
                bool hasDotnet = dotnet.TryGetValue(key, out var valueDotnet);

                // Look up the property contract so NormalizeDefault knows nullable vs non-nullable
                var propDecl = objContract?.Properties.GetClosestMatchProperty(key);
                var propType = propDecl?.PropertyType;
                var propContract = propType is null ? null : resolver.ResolveContract(propType);

                if (hasJson && hasDotnet) {
                    if (!JTokenSemanticEquals(valueJson!, valueDotnet!, propContract, resolver, serializer))
                        return false;
                }
                else {
                    // missing vs null/default
                    var present = hasJson ? valueJson! : valueDotnet!;
                    present = NormalizeDefault(present, propContract);

                    if (!IsNullLike(present))
                        return false;
                }
            }

            return true;
        }


        // ------------------------------------------------------------------------
        // ARRAY COMPARISON (order matters)
        // ------------------------------------------------------------------------
        private static bool JArrayEquals(
            JArray json,
            JArray dotnet,
            JsonContract? declaredContract,
            IContractResolver resolver, JsonSerializer serializer) {
            if (json.Count != dotnet.Count)
                return false;

            JsonArrayContract? arrContract = declaredContract as JsonArrayContract;

            for (int i = 0; i < json.Count; i++) {
                var elementType = arrContract?.CollectionItemType;
                var elementContract = elementType is null ? null : resolver.ResolveContract(elementType);

                if (!JTokenSemanticEquals(json[i], dotnet[i], elementContract, resolver, serializer))
                    return false;
            }

            return true;
        }


        // =====================================================================
        // PRIMITIVE HELPER
        // =====================================================================

        private static JToken SerializeValueToToken(object? value, JsonSerializer serializer) {
            // Serialize using the same serializer (respects converters, contract resolver, naming, etc.)
            using var sw = new System.IO.StringWriter(CultureInfo.InvariantCulture);
            using (var writer = new JsonTextWriter(sw)) {
                serializer.Serialize(writer, value);
            }

            // Parse back to a JToken so we can structurally compare
            var json = sw.ToString();

            // If the serializer wrote "null" it’s a JSON literal, parse will yield JValue Null.
            return JToken.Parse(json);
        }

        // ------------------------------------------------------------------------
        // NUMERIC SEMANTICS
        // ------------------------------------------------------------------------
        private static bool NumericEquals(JValue a, JValue b) {
            if (decimal.TryParse(a.ToString(), out var da) &&
                decimal.TryParse(b.ToString(), out var db))
                return da == db;

            return false; // the numbers are probably very big or things like NaN and, for safety, we simply say there's been a change
        }

        private static bool IsNumeric(JToken t)
            => t.Type is JTokenType.Integer or JTokenType.Float;


        // ------------------------------------------------------------------------
        // NULL / DEFAULT HANDLING (THE FIXED PART)
        // ------------------------------------------------------------------------
        private static JToken NormalizeDefault(JToken token, JsonContract? declared) {
            if (token is not JValue v)
                return token;

            var value = v.Value;
            if (value == null)
                return JValue.CreateNull();

            if (declared == null)
                return token;

            // Find underlying CLR type
            var clrType =
                declared.UnderlyingType ?? typeof(object);

            var nullableUnderlying = Nullable.GetUnderlyingType(clrType);

            var isNullable = nullableUnderlying != null;
            if (isNullable)
                return token;

            // Non-nullable default normalisation
            return value switch {
                int i when i == 0 => JValue.CreateNull(),
                long l when l == 0L => JValue.CreateNull(),
                double d when d == 0d => JValue.CreateNull(),
                float f when f == 0f => JValue.CreateNull(),
                decimal m when m == 0m => JValue.CreateNull(),
                bool b when b == false => JValue.CreateNull(),
                _ => token
            };
        }

        private static bool IsNullLike(JToken t)
            => t.Type is JTokenType.Null or JTokenType.Undefined;
    }
}