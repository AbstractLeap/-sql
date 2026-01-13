namespace TildeSql {
    using System;
    using System.Collections.Generic;
    using System.Text.Json;

    internal static class JsonEquality {
        /// <summary>
        ///     Compares two JSON strings for semantic equality with the following rules:
        ///     - Objects: property order is ignored.
        ///     - Objects: a property that is null on one side and missing on the other is considered equal.
        ///     - Arrays: order matters and lengths must match.
        ///     - Returns false immediately upon the first detected difference.
        /// </summary>
        public static bool JsonEquals(string jsonA, string jsonB) {
            using var docA = JsonDocument.Parse(jsonA);
            using var docB = JsonDocument.Parse(jsonB);
            return EqualsElement(docA.RootElement, docB.RootElement);
        }

        private static bool EqualsElement(JsonElement a, JsonElement b) {
            // Fast path for same kind where possible
            if (a.ValueKind == b.ValueKind) {
                switch (a.ValueKind) {
                    case JsonValueKind.Object:
                        return EqualsObject(a, b);

                    case JsonValueKind.Array:
                        return EqualsArray(a, b);

                    case JsonValueKind.String:
                        return string.Equals(a.GetString(), b.GetString(), StringComparison.Ordinal);

                    case JsonValueKind.Number:
                        return NumberEquals(a, b);

                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        return a.GetBoolean() == b.GetBoolean();

                    case JsonValueKind.Null:
                        return true; // both null

                    case JsonValueKind.Undefined:
                        return true; // both undefined (rare in parsed JSON)
                }
            }

            // Kinds differ: this is only OK for the "null vs. missing" rule at the *property* level,
            // which is handled in EqualsObject. If we reach here, it means two concrete values differ.
            return false;
        }

        private static bool EqualsObject(JsonElement a, JsonElement b) {
            // Collect union of property names
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var p in a.EnumerateObject()) keys.Add(p.Name);
            foreach (var p in b.EnumerateObject()) keys.Add(p.Name);

            foreach (var key in keys) {
                bool aHas = a.TryGetProperty(key, out var aProp);
                bool bHas = b.TryGetProperty(key, out var bProp);

                if (aHas && bHas) {
                    if (!EqualsElement(aProp, bProp)) return false;
                }
                else if (aHas && !bHas) {
                    // Property present only in A: allowed only if it's null
                    if (aProp.ValueKind != JsonValueKind.Null) return false;
                }
                else // !aHas && bHas
                {
                    if (bProp.ValueKind != JsonValueKind.Null) return false;
                }
            }

            return true;
        }

        private static bool EqualsArray(JsonElement a, JsonElement b) {
            int lenA = a.GetArrayLength();
            int lenB = b.GetArrayLength();
            if (lenA != lenB) return false;

            for (int i = 0; i < lenA; i++) {
                if (!EqualsElement(a[i], b[i])) return false;
            }

            return true;
        }

        private static bool NumberEquals(JsonElement a, JsonElement b) {
            // Try integral comparison first
            if (a.TryGetInt64(out long ai) && b.TryGetInt64(out long bi))
                return ai == bi;

            // Then try decimal for exact numeric comparison (treats 1 and 1.0 as equal)
            if (a.TryGetDecimal(out decimal ad) && b.TryGetDecimal(out decimal bd))
                return ad == bd;

            // Fallback: compare raw textual representation (covers extremely large/precise numbers)
            // Note: different formatting like "1.0" vs "1.00" will be considered different here.
            return string.Equals(a.GetRawText(), b.GetRawText(), StringComparison.Ordinal);
        }
    }
}