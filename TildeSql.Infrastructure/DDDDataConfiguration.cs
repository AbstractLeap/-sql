namespace TildeSql.Infrastructure {
    using TildeSql.Configuration;
    using TildeSql.Schema;
    using TildeSql.SqlServer;
    using TildeSql.JsonNet;

    public class DDDDataConfiguration : Configuration {
        public DDDDataConfiguration(ISchema schema, string connectionString)
            : base(schema) {
            this.UseSqlServer(connectionString).UseJsonNetFieldSerialization();
        }

        public ISessionFactory BuildSmtpSessionFactory() {
            return this.BuildSessionFactory();
        }
    }
}