namespace Leap.Data.Tests {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    using Basic.Reference.Assemblies;

    using FluentMigrator;
    using FluentMigrator.Builders.Create;
    using FluentMigrator.Runner;

    using Leap.Data.SqlMigrations;
    using Leap.Data.SqlMigrations.Model;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.DependencyInjection;

    internal class DatabaseGenerator {
        [ModuleInitializer]
        public static void Recreate() {
            var masterConnectionStringBuilder = new SqlConnectionStringBuilder(TestSessionFactoryBuilder.SqlServerConnectionString);
            var database = masterConnectionStringBuilder.InitialCatalog;
            masterConnectionStringBuilder.InitialCatalog = "master";
            using (var conn = new SqlConnection(masterConnectionStringBuilder.ConnectionString)) {
                conn.Open();

                using (var recreateDbCommand = conn.CreateCommand()) {
                    recreateDbCommand.CommandText = @$"IF EXISTS (SELECT 1 FROM sys.databases WHERE [name] = N'{database}')
                    BEGIN
                        ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{database}];
                    END;
                    create database [{database}]";
                    recreateDbCommand.ExecuteNonQuery();
                }

                conn.Close();
            }

            var schema = TestSchemaBuilder.Build();
            var diff = new Differ().Diff(new Database(), schema.ToDatabaseModel());
            var migrationCode = new Generator().CreateCode(diff, "Leap.Data.Tests.Migration", "Tests");
            var syntaxTree = CSharpSyntaxTree.ParseText(migrationCode);
            var references = new List<MetadataReference> {
                MetadataReference.CreateFromFile(typeof(Migration).Assembly.Location), MetadataReference.CreateFromFile(typeof(ICreateExpressionRoot).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create(
                "Leap.Data.Tests.Migration.dll",
                new[] { syntaxTree },
                references.Union(ReferenceAssemblies.Net50),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            Assembly assembly;
            using (var ms = new MemoryStream()) {
                var result = compilation.Emit(ms);
                ms.Seek(0, SeekOrigin.Begin);
                assembly = Assembly.Load(ms.ToArray());
            }

            var serviceProvider = CreateServices(TestSessionFactoryBuilder.SqlServerConnectionString, assembly);
            using (var scope = serviceProvider.CreateScope()) {
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                runner.MigrateUp();
            }
        }

        private static IServiceProvider CreateServices(string connectionString, Assembly migrationAssembly) {
            var services = new ServiceCollection()
                           // Add common FluentMigrator services
                           .AddFluentMigratorCore()
                           .ConfigureRunner(
                               rb => rb
                                     // Add SQLite support to FluentMigrator
                                     .AddSqlServer2016()
                                     // Set the connection string
                                     .WithGlobalConnectionString(connectionString)
                                     // Define the assembly containing the migrations
                                     .ScanIn(migrationAssembly)
                                     .For.Migrations())
                           // Enable logging to console in the FluentMigrator way
                           .AddLogging(lb => lb.AddFluentMigratorConsole());

            return services.BuildServiceProvider(false);
        }
    }
}