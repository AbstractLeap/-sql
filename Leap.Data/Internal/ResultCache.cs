namespace Leap.Data.Internal {
    using System.Collections;
    using System.Collections.Generic;

    using Leap.Data.Queries;

    class ResultCache<T> {
        private readonly IDictionary<IQuery, IList<T>> entries = new Dictionary<IQuery, IList<T>>();

        public void Add(IQuery query, IList<T> result) {
            this.entries[query] = result;
        }

        public bool TryGetValue(IQuery query, out IList<T> result) {
            if (this.entries.TryGetValue(query, out result)) {
                this.entries.Remove(query);
                return true;
            }

            result = null;
            return false;
        }
    }

    class ResultCache {
        private readonly IDictionary<IQuery, IList> entries = new Dictionary<IQuery, IList>();

        public void Add(IQuery query, IList result) {
            this.entries[query] = result;
        }

        public bool TryGetValue<T>(IQuery query, out IList<T> result) {
            if (this.entries.TryGetValue(query, out var list) && list is List<T> typedList) {
                this.entries.Remove(query);
                result = typedList;
                return true;
            }

            result = null;
            return false;
        }
    }
}