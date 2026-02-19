namespace TildeSql.Schema {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using TildeSql.Utilities;

    public class CollectionBuilder<TEntity> {
        private readonly SchemaBuilder schemaBuilder;

        private readonly string collectionName;

        public CollectionBuilder(SchemaBuilder schemaBuilder, string collectionName) {
            this.schemaBuilder  = schemaBuilder;
            this.collectionName = collectionName;
        }

        public SchemaBuilder AddColumn<TColumn>(string name, int? size = null, int? precision = null, bool isNullable = false, bool isIdentity = false) {
            this.schemaBuilder.AddAction<TEntity>(collection => collection.AddColumn<TColumn>(name, size, precision, isNullable, isIdentity), this.collectionName);
            return this.schemaBuilder;
        }

        public SchemaBuilder AddComputedColumn<TColumn>(string name, string formula, bool persisted = true, bool indexed = true) {
            this.schemaBuilder.AddAction<TEntity>(collection => collection.AddComputedColumn<TColumn>(name, formula, persisted, indexed), this.collectionName);
            return this.schemaBuilder;
        }

        public SchemaBuilder AddProjectionColumn<TColumn>(string name, Func<TEntity, TColumn> projectionFunc) {
            this.schemaBuilder.AddAction<TEntity>(collection => collection.AddProjectionColumn(name, projectionFunc), this.collectionName);
            return this.schemaBuilder;
        }

        public SchemaBuilder PrimaryKey(params string[] memberNames) {
            var members = new List<MemberInfo>();
            var classHierarchy = typeof(TEntity).GetClassHierarchy();
            foreach (var type in classHierarchy) {
                var matchingMembers = type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                          .Where(m => memberNames.Contains(m.Name) && members.All(existing => existing.Name != m.Name))
                                          .ToArray();
                members.AddRange(matchingMembers);
            }

            var missingMembers = memberNames.Where(name => members.All(m => m.Name != name)).ToArray();
            if (missingMembers.Any()) {
                throw new ArgumentOutOfRangeException($"Unable to find members named {string.Join(", ", missingMembers)}");
            }

            this.schemaBuilder.AddKeyMembers(typeof(TEntity), members.ToArray());
            return this.schemaBuilder;
        }

        public SchemaBuilder TrackedByDefault() {
            this.schemaBuilder.AddAction<TEntity>(collection => collection.TrackedByDefault = true, this.collectionName);
            return this.schemaBuilder;
        }

        public SchemaBuilder NotTrackedByDefault() {
            this.schemaBuilder.AddAction<TEntity>(collection => collection.TrackedByDefault = false, this.collectionName);
            return this.schemaBuilder;
        }
    }
}