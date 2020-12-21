namespace Leap.Data.Utilities {
    using System.Collections.Generic;

    static class SmartEnumerableExtensions {
        /// <summary>
        ///     Get index details about IEnumerable as you iterate
        /// </summary>
        /// <typeparam name="T">Type of enumerable</typeparam>
        /// <param name="source">Source enumerable</param>
        /// <returns>A new SmartEnumerable of the appropriate type</returns>
        public static SmartEnumerable<T> AsSmartEnumerable<T>(this IEnumerable<T> source) {
            return new SmartEnumerable<T>(source);
        }
    }
}