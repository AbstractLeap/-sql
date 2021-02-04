namespace Leap.Data.Tests.SqlMigrations {
    using System;
    using System.Threading.Tasks;

    using Leap.Data.Schema;
    using Leap.Data.SqlMigrations;

    using Xunit;

    public class Run {
        [Fact]
        public async Task Generate() {
            var migrationName = "HelloMigrations";
            var modelPath = @"F:\Projects\leap-data\Leap.Data.Tests.SqlMigrations\model.json";
            var migrationPath = $@"F:\Projects\leap-data\Leap.Data.Tests.SqlMigrations\{migrationName}.txt";
            var migrationNamespace = "Leap.Data.Tests.SqlMigrations";
            var schema = new SchemaBuilder().AddTypes(typeof(Blog)).Build();
            await Migrator.RunAsync(modelPath, migrationPath, migrationNamespace, migrationName, schema);
        }
    }

    public record BlogId
    {
        public BlogId()
        {
            this.Id = Guid.NewGuid();
        }

        public Guid Id { get; init; }
    }

    public class Blog
    {
        public Blog(string title)
        {
            this.BlogId = new BlogId();
            this.Title  = title;
        }

        public BlogId BlogId { get; init; }

        public string Title { get; set; }
        protected bool Equals(Blog other)
        {
            return Equals(this.BlogId, other.BlogId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Blog)obj);
        }

        public override int GetHashCode()
        {
            return (this.BlogId != null ? this.BlogId.GetHashCode() : 0);
        }

    }
}