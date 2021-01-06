namespace Leap.Data.SqlServer {
    using System;

    using Leap.Data.Configuration;
    using Leap.Data.Internal;
    using Leap.Data.SqlServer.QueryWriter;
    using Leap.Data.SqlServer.UpdateWriter;

    public static class ConfigurationExtensions {
        public static Configuration UseSqlServer(this Configuration configuration, string connectionString, Action<SqlServerConfiguration> setup = null) {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            var sqlServerConfiguration = new SqlServerConfiguration();
            setup?.Invoke(sqlServerConfiguration);

            var connectionFactory = sqlServerConfiguration.ConnectionFactory ?? new DefaultSqlServerConnectionFactory(connectionString);

            configuration.QueryExecutor  = new SqlQueryExecutor(connectionFactory, new SqlServerSqlQueryWriter(configuration.Schema), configuration.Schema);
            configuration.UpdateExecutor = new SqlUpdateExecutor(connectionFactory, new SqlServerSqlUpdateWriter(configuration.Schema, configuration.Serializer), new SqlServerDialect());
            return configuration;
        }
    }
}