namespace TildeSql {
    internal static class JsonSolidusEscapeIgnoringStringComparator {
        public static bool StringEquals(string left, string right) {
            if (ReferenceEquals(left, right)) {
                return true;
            }

            if (left == null || right == null) {
                return false;
            }

            var i = 0;
            var j = 0;
            while (i < left.Length && j < right.Length) {
                if (left[i] == right[j]) {
                    j++;
                    i++;
                    continue;
                }

                if (left[i] == '\\') {
                    if (i == left.Length - 1) {
                        return false;
                    }

                    if (left[i + 1] == '/' && right[j] == '/') {
                        i += 2;
                        j++;
                        continue;
                    }

                    return false;
                }

                if (right[j] == '\\') {
                    if (j == right.Length - 1) {
                        return false;
                    }

                    if (right[j + 1] == '/' && left[i] == '/') {
                        i++;
                        j += 2;
                        continue;
                    }

                    return false;
                }

                return false;
            }

            return i == left.Length && j == right.Length;
        }
    }
}