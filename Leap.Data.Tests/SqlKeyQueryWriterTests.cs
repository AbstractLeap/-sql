namespace Leap.Data.Tests {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Leap.Data.Internal;
    using Leap.Data.Queries;
    using Leap.Data.SqlServer.QueryWriter;
    using Leap.Data.Tests.TestDomain.Blog;

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
            Assert.Equal("select t.[BlogId], t.[Document], t.[DocumentType], t.[Version] from [dbo].[Blogs] as t where t.[BlogId] = @p1", command.Queries.First());
        }
    }

    public class SqlServerSqlQueryWriterTests {
        private readonly ITestOutputHelper outputHelper;

        public SqlServerSqlQueryWriterTests(ITestOutputHelper outputHelper) {
            this.outputHelper = outputHelper;
        }

        [Fact]
        public void ItDelegatesKeyQuery() {
            var schema = TestSchemaBuilder.Build();
            var writer = new SqlServerSqlQueryWriter(schema);
            var command = new Command();
            writer.Write(new KeyQuery<Blog, BlogId>(new BlogId(), schema.GetDefaultCollection<Blog>()), command);
            this.outputHelper.WriteLine(command.Queries.First());
            Assert.Equal("select t.[BlogId], t.[Document], t.[DocumentType], t.[Version] from [dbo].[Blogs] as t where t.[BlogId] = @p1", command.Queries.First());
        }

        [Fact]
        public void WhereClauseEnumerableExpanded() {
            var schema = TestSchemaBuilder.Build();
            var writer = new SqlServerSqlQueryWriter(schema);
            var command = new Command();
            writer.Write(
                new EntityQuery<Blog>(schema.GetDefaultCollection<Blog>()) {
                    WhereClause = "Foo in @Foo", WhereClauseParameters = new Dictionary<string, object> { { "Foo", new[] { Guid.NewGuid(), Guid.NewGuid() } } }
                },
                command);
            this.outputHelper.WriteLine(command.Queries.First());
            Assert.Equal("select t.[BlogId], t.[Document], t.[DocumentType], t.[Version] from [dbo].[Blogs] as t where (Foo in (@Foo_1,@Foo_2))", command.Queries.First());
            Assert.Equal(2, command.Parameters.Count());
        }

        [Fact]
        public void WhereClauseStringNotExpanded() {
            var schema = TestSchemaBuilder.Build();
            var writer = new SqlServerSqlQueryWriter(schema);
            var command = new Command();
            writer.Write(
                new EntityQuery<Blog>(schema.GetDefaultCollection<Blog>()) {
                    WhereClause = "Foo = @Foo", WhereClauseParameters = new Dictionary<string, object> { { "Foo", "I am an enumerable string" } }
                },
                command);
            this.outputHelper.WriteLine(command.Queries.First());
            Assert.Equal("select t.[BlogId], t.[Document], t.[DocumentType], t.[Version] from [dbo].[Blogs] as t where (Foo = @Foo)", command.Queries.First());
            Assert.Equal(1, command.Parameters.Count());
        }
    }
}