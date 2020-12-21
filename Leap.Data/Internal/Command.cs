namespace Leap.Data.Internal {
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    class Command {
        private readonly List<string> queries = new List<string>();

        private readonly Dictionary<string, ParameterInfo> parameters = new Dictionary<string, ParameterInfo>();

        public void AddQuery(string query) {
            this.queries.Add(query);
        }

        public string AddParameter(object value, DbType? dbType = null, ParameterDirection? direction = null, int? size = null) {
            var name = $"p{this.parameters.Count + 1}";
            this.AddParameter(name, value, dbType, direction, size);
            return name;
        }

        public void AddParameter(string name, object value, DbType? dbType = null, ParameterDirection? direction = null, int? size = null) {
            this.parameters[Clean(name)] = new ParameterInfo(name, value, direction ?? ParameterDirection.Input, dbType, size);
        }

        public IEnumerable<string> Queries => this.queries.AsReadOnly();

        public IEnumerable<ParameterInfo> Parameters => this.parameters.Values.AsEnumerable();

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
    }
}