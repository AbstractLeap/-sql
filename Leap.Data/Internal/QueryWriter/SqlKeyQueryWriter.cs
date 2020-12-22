namespace Leap.Data.Internal.QueryWriter {
    using System;
    using System.Text;

    using Fasterflect;

    using Leap.Data.Internal.Common;
    using Leap.Data.Queries;
    using Leap.Data.Schema;
    using Leap.Data.Utilities;

    internal abstract class SqlKeyQueryWriter : SqlBaseWriter, ISqlQueryWriter {
        private readonly ISchema schema;

        private readonly ISqlDialect sqlDialect;

        protected SqlKeyQueryWriter(ISchema schema, ISqlDialect sqlDialect)
            : base(sqlDialect, new KeyColumnValueExtractor(schema)) {
            this.schema     = schema;
            this.sqlDialect = sqlDialect;
        }

        public void Write(IQuery query, Command command) {
            if (query.GetType().Name != typeof(KeyQuery<,>).Name) {
                throw new Exception($"{query.GetType()} is not KeyQuery<>");
            }

            var genericTypeParameters = query.GetType().GetGenericArguments();
            this.CallMethod(genericTypeParameters, nameof(Write), new[] { query.GetType(), typeof(Command) }, Flags.InstancePrivate | Flags.ExactBinding, query, command);
        }

        private void Write<TEntity, TKey>(KeyQuery<TEntity, TKey> query, Command command)
            where TEntity : class {
            var table = this.schema.GetTable<TEntity>();
            var builder = new StringBuilder("select ");
            foreach (var columnEntry in table.Columns.AsSmartEnumerable()) {
                builder.Append("t.");
                this.sqlDialect.AppendName(builder, columnEntry.Value.Name);
                if (!columnEntry.IsLast) {
                    builder.Append(",");
                }

                builder.Append(" ");
            }

            builder.Append("from ");
            this.sqlDialect.AppendName(builder, table.Name);
            builder.Append(" as t");
            builder.Append(" where ");
            this.WriteWhereClauseForSingleEntity<TEntity, TKey>(query.Key, command, builder, true);

            command.AddQuery(builder.ToString());
        }
    }
}