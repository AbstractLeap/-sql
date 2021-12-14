using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Leap.Data.Tests.SqlMigrations.Generator
{
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
            var generator = new Leap.Data.SqlMigrations.Generator();
            var diff = new Data.SqlMigrations.Difference();
            diff.AddCreateTable(new Data.SqlMigrations.Model.Table
            {
                Name = "Blogs",
                PrimaryKeyName = "pk_Blogs",
                Schema = "foo",
                Columns = new List<Data.SqlMigrations.Model.Column>
                {
                    new Data.SqlMigrations.Model.Column
                    {
                        Name = "BlogId",
                        IsNullable = false,
                        IsPrimaryKey = false,
                        Size = 64,
                        Type = typeof(string)
                    }
                }
            });
            var code = generator.CreateCode(diff, "Tests.Migrations", "Migration1");
            this.output.WriteLine(code);
        }
    }
}
