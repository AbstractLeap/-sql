namespace TildeSql.SqlServer {
    using System;

    using TildeSql.Configuration;
    using TildeSql.Internal;
    using TildeSql.SqlServer.QueryWriter;
    using TildeSql.SqlServer.UpdateWriter;

    public static class ConfigurationExtensions {
        public static Configuration UseSqlServer(this Configuration configuration, string connectionString, Action<SqlServerConfiguration> setup = null) {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            var sqlServerConfiguration = new SqlServerConfiguration();
            setup?.Invoke(sqlServerConfiguration);

            var connectionFactoryFactory = sqlServerConfiguration.ConnectionFactoryFactory
                                           ?? (sqlServerConfiguration.ConnectionMode.HasValue && sqlServerConfiguration.ConnectionMode.Value == ConnectionMode.PerSession
                                                   ? new ConnectionPerSessionSqlServerConnectionFactoryFactory(connectionString)
                                                   : new ConnectionPerCommandSqlServerConnectionFactoryFactory(connectionString));

            configuration.QueryExecutorFactory = () => new SqlQueryExecutor(
                connectionFactoryFactory.Get(),
                new SqlServerSqlQueryWriter(configuration.Schema),
                configuration.Schema);
            configuration.UpdateExecutorFactory = () => new SqlUpdateExecutor(
                connectionFactoryFactory.Get(),
                new SqlServerSqlUpdateWriter(configuration.Schema, configuration.Serializer),
                new SqlServerDialect());
            return configuration;
        }
    }
}