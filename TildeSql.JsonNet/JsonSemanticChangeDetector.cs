namespace TildeSql.JsonNet {
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    using System;
    using System.Collections.Generic;

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

            JToken tokenA = JToken.Parse(json);
            JToken tokenB = JToken.FromObject(obj, serializer);

            // We pass the root contract type into comparison
            var contract = serializer.ContractResolver.ResolveContract(obj.GetType()) as JsonObjectContract;

            return !JTokenSemanticEquals(tokenA, tokenB, contract, serializer.ContractResolver);
        }


        // ------------------------------------------------------------------------
        // TOP LEVEL COMPARISON
        // ------------------------------------------------------------------------
        private static bool JTokenSemanticEquals(
            JToken a,
            JToken b,
            JsonContract? declaredContract,
            IContractResolver resolver) {
            if (ReferenceEquals(a, b))
                return true;
            if (a is null || b is null)
                return false;

            // Normalize defaults using the declared CLR type (critical fix)
            a = NormalizeDefault(a, declaredContract);
            b = NormalizeDefault(b, declaredContract);

            // Numeric types are semantically equal even across integer/float mismatch
            if (IsNumeric(a) && IsNumeric(b))
                return NumericEquals((JValue)a, (JValue)b);

            if (a.Type != b.Type) {
                // Only OK if both are null-like (null/missing/default)
                return IsNullLike(a) && IsNullLike(b);
            }

            switch (a.Type) {
                case JTokenType.Object:
                    return JObjectEquals((JObject)a, (JObject)b, declaredContract, resolver);

                case JTokenType.Array:
                    return JArrayEquals((JArray)a, (JArray)b, declaredContract, resolver);

                case JTokenType.String:
                    return String.Equals(
                        (string?)((JValue)a).Value,
                        (string?)((JValue)b).Value,
                        StringComparison.Ordinal);

                case JTokenType.Boolean:
                    return (bool)((JValue)a).Value! == (bool)((JValue)b).Value!;

                case JTokenType.Null:
                case JTokenType.Undefined:
                    return true;

                default:
                    return JToken.DeepEquals(a, b);
            }
        }


        // ------------------------------------------------------------------------
        // OBJECT COMPARISON
        // ------------------------------------------------------------------------
        private static bool JObjectEquals(
            JObject a,
            JObject b,
            JsonContract? declaredContract,
            IContractResolver resolver) {
            var objContract = declaredContract as JsonObjectContract;

            var allKeys = new HashSet<string>(StringComparer.Ordinal);

            foreach (var p in a.Properties()) allKeys.Add(p.Name);
            foreach (var p in b.Properties()) allKeys.Add(p.Name);

            foreach (var key in allKeys) {
                bool hasA = a.TryGetValue(key, out var va);
                bool hasB = b.TryGetValue(key, out var vb);

                // Look up the property contract so NormalizeDefault knows nullable vs non-nullable
                var propDecl = objContract?.Properties.GetClosestMatchProperty(key);
                var propType = propDecl?.PropertyType;
                var propContract = propType is null ? null : resolver.ResolveContract(propType);

                if (hasA && hasB) {
                    if (!JTokenSemanticEquals(va!, vb!, propContract, resolver))
                        return false;
                }
                else {
                    // missing vs null/default
                    var present = hasA ? va! : vb!;
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
            JArray a,
            JArray b,
            JsonContract? declaredContract,
            IContractResolver resolver) {
            if (a.Count != b.Count)
                return false;

            JsonArrayContract? arrContract = declaredContract as JsonArrayContract;

            for (int i = 0; i < a.Count; i++) {
                var elementType = arrContract?.CollectionItemType;
                var elementContract = elementType is null ? null : resolver.ResolveContract(elementType);

                if (!JTokenSemanticEquals(a[i], b[i], elementContract, resolver))
                    return false;
            }

            return true;
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