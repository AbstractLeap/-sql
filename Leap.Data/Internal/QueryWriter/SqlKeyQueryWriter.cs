namespace Leap.Data.Internal.QueryWriter {
    using System;
    using System.Text;

    using Fasterflect;

    using Leap.Data.Queries;
    using Leap.Data.Schema;
    using Leap.Data.Utilities;

    internal abstract class SqlKeyQueryWriter : ISqlQueryWriter {
        private readonly ISchema schema;

        public SqlKeyQueryWriter(ISchema schema) {
            this.schema = schema;
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
                this.AppendName(builder, columnEntry.Value.Name);
                if (!columnEntry.IsLast) {
                    builder.Append(",");
                }

                builder.Append(" ");
            }

            builder.Append("from ");
            this.AppendName(builder, table.Name);
            builder.Append(" as t");
            builder.Append(" where ");
            foreach (var column in table.KeyColumns) {
                builder.Append("t.");
                this.AppendName(builder, column.Name);
                builder.Append(" = ");
                var paramName = command.AddParameter(Guid.Parse("77b55913-d2b6-488d-8860-3e8e70cb5146"));
                this.AddParameter(builder, paramName);
            }

            command.AddQuery(builder.ToString());
        }

        protected abstract void AppendName(StringBuilder builder, string name);

        protected abstract void AddParameter(StringBuilder builder, string name);
    }
}