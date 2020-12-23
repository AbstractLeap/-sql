namespace Leap.Data.Internal {
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using Leap.Data.Queries;

    class ResultCache {
        private readonly IDictionary<Guid, IList> entries = new Dictionary<Guid, IList>();

        public void Add(IQuery query, IList result) {
            this.entries[query.Identifier] = result;
        }

        public bool TryGetValue<T>(IQuery query, out List<T> result) {
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