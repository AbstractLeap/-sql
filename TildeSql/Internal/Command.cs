namespace TildeSql.Internal {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class Command {
        private readonly List<string> queries = new List<string>();

        private readonly List<ParameterInfo> parameters = new List<ParameterInfo>();

        /// <summary>
        /// Used to check for duplicate names, case insensitive
        /// </summary>
        private readonly HashSet<string> parameterNames = new HashSet<string>();

        public event EventHandler<QueryAddedEventArgs> OnQueryAdded;

        private static Regex alphaRegex = new Regex("[^a-zA-Z0-9 -]", RegexOptions.Compiled);

        public void AddQuery(string query) {
            if (OnQueryAdded != null) {
                var args = new QueryAddedEventArgs(query);
                OnQueryAdded(this, args);
                query = args.Query;
            }

            if (!string.IsNullOrWhiteSpace(query)) {
                this.queries.Add(query);
            }
        }

        public string AddParameter(object value, DbType? dbType = null, ParameterDirection? direction = null, int? size = null) => this.AddParameter("p", value, dbType, direction, size);

        public string AddParameter(string name, object value, DbType? dbType = null, ParameterDirection? direction = null, int? size = null) {
            var cleanedName = Clean(name);
            var upperCleanedname = cleanedName.ToUpper();
            if (this.parameterNames.Contains(upperCleanedname)) {
                // already got this parameter in the query so we'll rename
                cleanedName = cleanedName + "_" + (this.parameters.Count + 1);
            }

            this.parameters.Add(new ParameterInfo(cleanedName, value, direction ?? ParameterDirection.Input, dbType, size));
            this.parameterNames.Add(upperCleanedname);
            return cleanedName;
        }

        /// <remarks>
        ///     For unit testing
        /// </remarks>
        internal IEnumerable<string> Queries => this.queries.AsReadOnly();

        /// <remarks>
        ///     For unit testing
        /// </remarks>
        internal IEnumerable<ParameterInfo> Parameters => this.parameters.AsEnumerable();

        public int ParameterCount => this.parameters.Count;

        private static string Clean(string name) {
            if (string.IsNullOrWhiteSpace(name)) {
                return name;
            }

            switch (name[0]) {
                case '@':
                case ':':
                case '?':
                    name = name.Substring(1);
                    break;
            }

            name = alphaRegex.Replace(name, string.Empty);
            if (char.IsNumber(name[0])) {
                name = $"p{name}";
            }

            return name;
        }

        public void WriteToDbCommand(DbCommand dbCommand) {
            dbCommand.CommandText = string.Join(";", this.queries);
            foreach (var (name, value, parameterDirection, dbType, size) in this.Parameters) {
                var parameter = dbCommand.CreateParameter();
                parameter.ParameterName = name;
                parameter.Value = value;
                parameter.Direction = parameterDirection;
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