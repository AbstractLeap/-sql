namespace Leap.Data.Internal.Common {
    using System.Text;

    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Utilities;

    abstract class SqlBaseWriter {
        private readonly ISqlDialect sqlDialect;

        private readonly IKeyColumnValueExtractor keyColumnValueExtractor;

        protected SqlBaseWriter(ISqlDialect sqlDialect, IKeyColumnValueExtractor keyColumnValueExtractor) {
            this.sqlDialect              = sqlDialect;
            this.keyColumnValueExtractor = keyColumnValueExtractor;
        }

        protected void WriteWhereClauseForSingleEntity<TEntity, TKey>(TKey key, Command command, StringBuilder builder, bool useAlias = false)
            where TEntity : class {
            foreach (var keyColumnEntry in this.keyColumnValueExtractor.Extract<TEntity, TKey>(key).AsSmartEnumerable()) {
                if (useAlias) {
                    builder.Append("t.");
                }

                this.sqlDialect.AppendName(builder, keyColumnEntry.Value.Key.Name);
                builder.Append(" = ");
                var paramName = command.AddParameter(keyColumnEntry.Value.Value);
                this.sqlDialect.AddParameter(builder, paramName);
                if (!keyColumnEntry.IsLast) {
                    builder.Append(" and ");
                }
            }
        }
    }
}