namespace Leap.Data.Internal {
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using Leap.Data.Queries;

    class ResultCache<T> {
        private readonly IDictionary<Guid, IList<T>> entries = new Dictionary<Guid, IList<T>>();

        public void Add(IQuery query, IList<T> result) {
            this.entries[query.Identifier] = result;
        }

        public bool TryGetValue(IQuery query, out IList<T> result) {
            if (this.entries.TryGetValue(query.Identifier, out result)) {
                this.entries.Remove(query.Identifier);
                return true;
            }

            result = null;
            return false;
        }
    }

    class ResultCache {
        private readonly IDictionary<Guid, IList> entries = new Dictionary<Guid, IList>();

        public void Add(IQuery query, IList result) {
            this.entries[query.Identifier] = result;
        }

        public bool TryGetValue<T>(IQuery query, out IList<T> result) {
            if (this.entries.TryGetValue(query.Identifier, out var list) && list is List<T> typedList) {
                this.entries.Remove(query.Identifier);
                result = typedList;
                return true;
            }

            result = null;
            return false;
        }
    }
}