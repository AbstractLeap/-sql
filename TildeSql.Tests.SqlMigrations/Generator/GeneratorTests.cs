namespace TildeSql.Tests.SqlMigrations.Generator
{
    using System.Collections.Generic;

    using TildeSql.SqlMigrations;
    using TildeSql.SqlMigrations.Model;

    using Xunit;

    public class GeneratorTests
    {
        private readonly ITestOutputHelper output;

        public GeneratorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Works()
        {
            var generator = new Generator();
            var diff = new Difference();
            diff.AddCreateTable(new Table
            {
                Name = "Blogs",
                PrimaryKeyName = "pk_Blogs",
                Schema = "foo",
                Columns = new List<Column>
                {
                    new Column
                    {
                        Name = "BlogId",
                        IsNullable = false,
                        IsPrimaryKey = false,
                        Size = 64,
                        Type = typeof(string)
                    }
                },
                Indexes = new List<Index>()
            });
            var code = generator.CreateCode(diff, "Tests.Migrations", "Migration1");
            this.output.WriteLine(code);
        }
    }
}
