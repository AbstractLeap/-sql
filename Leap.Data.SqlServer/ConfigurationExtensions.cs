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