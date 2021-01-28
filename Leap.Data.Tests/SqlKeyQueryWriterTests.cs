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
            var keyQueryWriter = new SqlServerSqlKeyQueryWriter(TestSchema.Get());
            var command = new Command();
            keyQueryWriter.Write(new KeyQuery<Blog, BlogId>(new BlogId()), command);
            this.outputHelper.WriteLine(command.Queries.First());
            Assert.Equal("select t.[Id], t.[Document], t.[DocumentType], t.[Version] from [Blogs] as t where t.[Id] = @p1", command.Queries.First());
        }
    }

    public class SqlServerSqlQueryWriterTests {
        private readonly ITestOutputHelper outputHelper;

        public SqlServerSqlQueryWriterTests(ITestOutputHelper outputHelper) {
            this.outputHelper = outputHelper;
        }

        [Fact]
        public void ItDelegatesKeyQuery() {
            var writer = new SqlServerSqlQueryWriter(TestSchema.Get());
            var command = new Command();
            writer.Write(new KeyQuery<Blog, BlogId>(new BlogId()), command);
            this.outputHelper.WriteLine(command.Queries.First());
            Assert.Equal("select t.[Id], t.[Document], t.[DocumentType], t.[Version] from [Blogs] as t where t.[Id] = @p1", command.Queries.First());
        }
    }
}