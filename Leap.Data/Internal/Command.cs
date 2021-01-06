namespace Leap.Data.Internal {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;

    public class Command {
        private readonly List<string> queries = new List<string>();

        private readonly Dictionary<string, ParameterInfo> parameters = new Dictionary<string, ParameterInfo>();

        public event EventHandler<QueryAddedEventArgs> OnQueryAdded;
        
        public void AddQuery(string query) {
            if (this.OnQueryAdded != null) {
                var args = new QueryAddedEventArgs(query);
                this.OnQueryAdded(this, args);
                query = args.Query;
            }

            if (!string.IsNullOrWhiteSpace(query)) {
                this.queries.Add(query);
            }
        }

        public string AddParameter(object value, DbType? dbType = null, ParameterDirection? direction = null, int? size = null) {
            var name = $"p{this.parameters.Count + 1}";
            this.AddParameter(name, value, dbType, direction, size);
            return name;
        }

        public void AddParameter(string name, object value, DbType? dbType = null, ParameterDirection? direction = null, int? size = null) {
            this.parameters[Clean(name)] = new ParameterInfo(name, value, direction ?? ParameterDirection.Input, dbType, size);
        }

        /// <remarks>
        ///     For unit testing
        /// </remarks>
        internal IEnumerable<string> Queries => this.queries.AsReadOnly();

        /// <remarks>
        ///     For unit testing
        /// </remarks>
        internal IEnumerable<ParameterInfo> Parameters => this.parameters.Values.AsEnumerable();

        private static string Clean(string name) {
            if (!string.IsNullOrEmpty(name)) {
                switch (name[0]) {
                    case '@':
                    case ':':
                    case '?':
                        return name.Substring(1);
                }
            }

            return name;
        }

        public void WriteToDbCommand(DbCommand dbCommand) {
            dbCommand.CommandText = string.Join(";", this.queries);
            foreach (var (name, value, parameterDirection, dbType, size) in this.Parameters) {
                var parameter = dbCommand.CreateParameter();
                parameter.ParameterName = name;
                parameter.Value         = value;
                parameter.Direction     = parameterDirection;
                if (dbType.HasValue) {
                    parameter.DbType = dbType.Value;
                }

                if (size.HasValue) {
                    parameter.Size = size.Value;
                }

                dbCommand.Parameters.Add(parameter);
            }
        }
    }

    public class QueryAddedEventArgs : EventArgs {
        public string Query { get; set; }

        public QueryAddedEventArgs(string query) {
            this.Query = query;
        }
    }
}