namespace Leap.Data.Tests.SqlMigrations {
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using FluentMigrator.Runner;

    using Leap.Data.Schema;
    using Leap.Data.SqlMigrations;
    using Leap.Data.SqlServer;

    using Microsoft.Extensions.DependencyInjection;

    using Xunit;

    public class Run {
        private const string ModelPath = @"F:\Projects\leap-data\Leap.Data.Tests.SqlMigrations\model.json";

        private const string MigrationNamespace = "Leap.Data.Tests.SqlMigrations";

        [Fact]
        public async Task CreateTable() {
            var migrationName = "HelloMigrations";
            var migrationPath = GetMigrationPath(migrationName);
            var schema = new SchemaBuilder().UseSqlServerConvention().AddTypes(typeof(Blog)).Build();
            await Migrator.RunAsync(ModelPath, migrationPath, MigrationNamespace, migrationName, schema);
        }

        [Fact]
        public async Task AddColumn() {
            var migrationName = "AddColumn";
            var migrationPath = GetMigrationPath(migrationName);
            var schemaBuilder = new SchemaBuilder().UseSqlServerConvention().AddTypes(typeof(Blog));
            schemaBuilder.Setup<Blog>().AddProjectionColumn("FirstLetter", blog => blog.Title.FirstOrDefault().ToString());
            var schema = schemaBuilder.Build();
            await Migrator.RunAsync(ModelPath, migrationPath, MigrationNamespace, migrationName, schema);
        }

        [Fact]
        public async Task DropColumn() {
            var migrationName = "DropColumn";
            var migrationPath = GetMigrationPath(migrationName);
            var schema = new SchemaBuilder().UseSqlServerConvention().AddTypes(typeof(Blog)).Build();
            await Migrator.RunAsync(ModelPath, migrationPath, MigrationNamespace, migrationName, schema);
        }

        [Fact]
        public async Task DropTable() {
            var migrationName = "DropTable";
            var migrationPath = GetMigrationPath(migrationName);
            var schema = new SchemaBuilder().UseSqlServerConvention().Build();
            await Migrator.RunAsync(ModelPath, migrationPath, MigrationNamespace, migrationName, schema);
        }

        private static string GetMigrationPath(string migrationName) {
            return $@"F:\Projects\leap-data\Leap.Data.Tests.SqlMigrations\{migrationName}.cs";
        }

        [Fact(Skip = "Just for testing")]
        public async Task Execute() {
            var serviceProvider = CreateServices();

            // Put the database update into a scope to ensure
            // that all resources will be disposed.
            using (var scope = serviceProvider.CreateScope()) {
                var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                // Execute the migrations
                runner.MigrateUp();
            }
        }
        
        [Fact(Skip = "Just for testing")]
        public async Task Down()
        {
            var serviceProvider = CreateServices();

            // Put the database update into a scope to ensure
            // that all resources will be disposed.
            using (var scope = serviceProvider.CreateScope())
            {
                var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                // Execute the migrations
                runner.MigrateDown(202102062214);
            }
        }

        /// <summary>
        ///     Configure the dependency injection services
        /// </summary>
        private static IServiceProvider CreateServices() {
            return new ServiceCollection()
                   // Add common FluentMigrator services
                   .AddFluentMigratorCore()
                   .ConfigureRunner(
                       rb => rb
                             // Add SQLite support to FluentMigrator
                             .AddSqlServer2016()
                             // Set the connection string
                             .WithGlobalConnectionString("Server=.;Database=leap-data;Trusted_Connection=True;")
                             // Define the assembly containing the migrations
                             .ScanIn(typeof(Run).Assembly)
                             .For.Migrations())
                   // Enable logging to console in the FluentMigrator way
                   .AddLogging(lb => lb.AddFluentMigratorConsole())
                   // Build the service provider
                   .BuildServiceProvider(false);
        }
    }

    public record BlogId {
        public BlogId() {
            this.Id = Guid.NewGuid();
        }

        public Guid Id { get; init; }
    }

    public class Blog {
        public Blog(string title) {
            this.BlogId = new BlogId();
            this.Title  = title;
        }

        public BlogId BlogId { get; init; }

        public string Title { get; set; }

        protected bool Equals(Blog other) {
            return Equals(this.BlogId, other.BlogId);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Blog)obj);
        }

        public override int GetHashCode() {
            return (this.BlogId != null ? this.BlogId.GetHashCode() : 0);
        }
    }
}