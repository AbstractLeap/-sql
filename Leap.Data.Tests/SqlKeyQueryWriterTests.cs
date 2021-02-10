namespace Leap.Data.Tests {
    using System.Linq;

    using Leap.Data.Internal;
    using Leap.Data.Queries;
    using Leap.Data.SqlServer.QueryWriter;

    using Xunit;
    using Xunit.Abstractions;

    public class SqlKeyQueryWriterTests {
        private readonly ITestOutputHelper outputHelper;

        public SqlKeyQueryWriterTests(ITestOutputHelper outputHelper) {
            this.outputHelper = outputHelper;
        }

        [Fact]
        public void ItWorks() {
            var schema = TestSchema.Get();
            var keyQueryWriter = new SqlServerSqlKeyQueryWriter(schema);
            var command = new Command();
            keyQueryWriter.Write(new KeyQuery<Blog, BlogId>(new BlogId(), schema.GetDefaultCollection<Blog>()), command);
            this.outputHelper.WriteLine(command.Queries.First());
            Assert.Equal("select t.[Id], t.[Document], t.[DocumentType], t.[Version] from [dbo].[Blogs] as t where t.[Id] = @p1", command.Queries.First());
        }
    }

    public class SqlServerSqlQueryWriterTests {
        private readonly ITestOutputHelper outputHelper;

        public SqlServerSqlQueryWriterTests(ITestOutputHelper outputHelper) {
            this.outputHelper = outputHelper;
        }

        [Fact]
        public void ItDelegatesKeyQuery() {
            var schema = TestSchema.Get();
            var writer = new SqlServerSqlQueryWriter(schema);
            var command = new Command();
            writer.Write(new KeyQuery<Blog, BlogId>(new BlogId(), schema.GetDefaultCollection<Blog>()), command);
            this.outputHelper.WriteLine(command.Queries.First());
            Assert.Equal("select t.[Id], t.[Document], t.[DocumentType], t.[Version] from [dbo].[Blogs] as t where t.[Id] = @p1", command.Queries.First());
        }
    }
}