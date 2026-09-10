namespace TildeSql.Tests {
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Loader;

    using FluentMigrator;
    using FluentMigrator.Builders.Create;
    using FluentMigrator.Runner;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.DependencyInjection;

    using TildeSql.SqlMigrations;
    using TildeSql.SqlMigrations.Model;

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
            var migrationCode = new Generator().CreateCode(diff, "TildeSql.Tests.Migration", "Tests");
            var syntaxTree = CSharpSyntaxTree.ParseText(migrationCode);
            var runtimeReferences = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? string.Empty)
               .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
               .Select(path => MetadataReference.CreateFromFile(path));
            var references = runtimeReferences.Concat(new[] {
                MetadataReference.CreateFromFile(typeof(Migration).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ICreateExpressionRoot).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(MigratorExtensions).Assembly.Location)
            });

            var compilation = CSharpCompilation.Create(
                "TildeSql.Tests.Migration.dll",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            Assembly assembly;
            using (var ms = new MemoryStream()) {
                var result = compilation.Emit(ms);
                if (!result.Success) {
                    var errors = string.Join(
                        Environment.NewLine,
                        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
                    throw new InvalidOperationException($"Dynamic migration compilation failed:{Environment.NewLine}{errors}");
                }

                ms.Seek(0, SeekOrigin.Begin);
                assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
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