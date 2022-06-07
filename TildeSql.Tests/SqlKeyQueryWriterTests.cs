namespace TildeSql.Tests {
    using System.Linq;

    using TildeSql.Internal;
    using TildeSql.Queries;
    using TildeSql.SqlServer.QueryWriter;
    using TildeSql.Tests.TestDomain.Blog;

    using Xunit;
    using Xunit.Abstractions;

    public class SqlKeyQueryWriterTests {
        private readonly ITestOutputHelper outputHelper;

        public SqlKeyQueryWriterTests(ITestOutputHelper outputHelper) {
            this.outputHelper = outputHelper;
        }

        [Fact]
        public void ItWorks() {
            var schema = TestSchemaBuilder.Build();
            var keyQueryWriter = new SqlServerSqlKeyQueryWriter(schema);
            var command = new Command();
            keyQueryWriter.Write(new KeyQuery<Blog, BlogId>(new BlogId(), schema.GetDefaultCollection<Blog>()), command);
            this.outputHelper.WriteLine(command.Queries.First());
            Assert.Equal("select t.[BlogId], t.[Document], t.[DocumentType], t.[Version] from [dbo].[Blogs] as t where t.[BlogId] = @BlogId", command.Queries.First());
        }
    }
}