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

            configuration.QueryExecutorFactory = () => {
                // This factory is created once per session. Each batch gets a new executor, but PerSession mode
                // shares this connection factory and therefore one physical connection for the session lifetime.
                var connectionFactory = connectionFactoryFactory.Get();
                return new QueryExecutorFactory {
                    CreateExecutor = () => new SqlQueryExecutor(
                        connectionFactory,
                        new SqlServerSqlQueryWriter(configuration.Schema),
                        configuration.Schema),
                    AsyncDisposable = connectionFactory as IAsyncDisposable,
                    Disposable = connectionFactory as IDisposable
                };
            };
            configuration.UpdateExecutorFactory = () => new SqlUpdateExecutor(
                connectionFactoryFactory.Get(),
                new SqlServerSqlUpdateWriter(configuration.Schema, configuration.Serializer),
                new SqlServerDialect());
            return configuration;
        }
    }
}