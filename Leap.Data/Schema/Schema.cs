namespace Leap.Data.Schema {
    using System;
    using System.Collections.Generic;

    class Schema : ISchema {
        private readonly Dictionary<Type, Table> tableLookup = new();
        
        public Table GetTable<TEntity>() {
            if (this.tableLookup.TryGetValue(typeof(TEntity), out var table)) {
                return table;
            }

            return null;
        }

        public void TryAddTable(Type entityType, Table table) {
            if (this.tableLookup.ContainsKey(entityType)) {
                return;
            }
            
            this.tableLookup.Add(entityType, table);
        }
    }
}