namespace Leap.Data.Schema {
    using System;
    using System.Collections.Generic;

    class Schema : ISchema {
        private readonly Dictionary<Type, List<Table>> tableLookup = new();

        private readonly Dictionary<string, Table> collectionTableLookup = new();

        private readonly Dictionary<Type, Table> defaultTableLookup = new();

        public IEnumerable<Table> All() {
            return this.collectionTableLookup.Values;
        }

        public Table GetTable(string collectionName) {
            if (this.collectionTableLookup.TryGetValue(collectionName, out var table)) {
                return table;
            }

            return null; // TODO should this throw?
        }

        public Table GetDefaultTable<TEntity>() {
            var entityType = typeof(TEntity);
            if (this.defaultTableLookup.TryGetValue(entityType, out var table)) {
                return table;
            }

            if (this.tableLookup.TryGetValue(entityType, out var tables)) {
                if (tables.Count == 1) {
                    this.defaultTableLookup.Add(entityType, tables[0]); // to speed up future requests
                    return tables[0];
                }

                if (tables.Count > 1) {
                    throw new Exception($"Unable to determine default table for {entityType}. You should specify the default table as you have multiple collections of this type.");
                }
            }
            
            return null; // TODO should this throw?
        }

        public void AddTable(Table table) {
            if (this.collectionTableLookup.ContainsKey(table.CollectionName)) {
                throw new Exception($"A table for the collection {table.CollectionName} already exists");
            }
            
            this.collectionTableLookup.Add(table.CollectionName, table);
            foreach (var entityType in table.EntityTypes) {
                if (!this.tableLookup.TryGetValue(entityType, out var tables)) {
                    tables = new List<Table> { table };
                    this.tableLookup.Add(entityType, tables);
                }
                else {
                    tables.Add(table);
                }
            }
        }
    }
}