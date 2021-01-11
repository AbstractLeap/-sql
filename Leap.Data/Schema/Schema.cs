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

        public void AddTable(Table table) {
            foreach (var entityType in table.EntityTypes) {
                if (this.tableLookup.TryGetValue(entityType, out var dupeTable)) {
                    throw new Exception($"The entity type {entityType} has already been added to the schema with table {dupeTable.Name}");
                }
                
                this.tableLookup.Add(entityType, table);
            }
        }
    }
}