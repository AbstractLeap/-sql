namespace TildeSql.Tests {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using TildeSql.Internal;
    using TildeSql.Queries;
    using TildeSql.SqlServer.QueryWriter;
    using TildeSql.Tests.TestDomain.Blog;

    using Xunit;

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
            Assert.Equal("select t.[BlogId], t.[Document], t.[DocumentType], t.[Version] from [dbo].[Blogs] as t where t.[BlogId] = @BlogId", command.Queries.First());
        }

        [Fact]
        public void WhereClauseEnumerableExpanded() {
            var schema = TestSchemaBuilder.Build();
            var writer = new SqlServerSqlQueryWriter(schema);
            var command = new Command();
            writer.Write(
                new EntityQuery<Blog>(schema.GetDefaultCollection<Blog>()) {
                    WhereClause = "Foo in @Foo",
                    WhereClauseParameters = new Dictionary<string, object> { { "Foo", new[] { Guid.NewGuid(), Guid.NewGuid() } } }
                },
                command);
            this.outputHelper.WriteLine(command.Queries.First());
            Assert.Equal("select t.[BlogId], t.[Document], t.[DocumentType], t.[Version] from [dbo].[Blogs] as t where (Foo in (@Foo,@Foo_2))", command.Queries.First());
            Assert.Equal(2, command.Parameters.Count());
            Assert.Equal("Foo", command.Parameters.ElementAt(0).Name);
            Assert.Equal("Foo_2", command.Parameters.ElementAt(1).Name);
        }

        [Fact]
        public void WhereClauseStringNotExpanded() {
            var schema = TestSchemaBuilder.Build();
            var writer = new SqlServerSqlQueryWriter(schema);
            var command = new Command();
            writer.Write(
                new EntityQuery<Blog>(schema.GetDefaultCollection<Blog>()) {
                    WhereClause = "Foo = @Foo",
                    WhereClauseParameters = new Dictionary<string, object> { { "Foo", "I am an enumerable string" } }
                },
                command);
            this.outputHelper.WriteLine(command.Queries.First());
            Assert.Equal("select t.[BlogId], t.[Document], t.[DocumentType], t.[Version] from [dbo].[Blogs] as t where (Foo = @Foo)", command.Queries.First());
            Assert.Single(command.Parameters);
        }

        [Fact]
        public void SimilarVariableNamesReplacedCorrectlyWithMultipleQueries() {
            var schema = TestSchemaBuilder.Build();
            var writer = new SqlServerSqlQueryWriter(schema);
            var command = new Command();
            writer.Write(
                new EntityQuery<Blog>(schema.GetDefaultCollection<Blog>()) {
                    WhereClause = "Foo = @Foo and Food = @Food",
                    WhereClauseParameters = new Dictionary<string, object> {
                        { "Foo", "I am an enumerable string" } ,
                        { "Food", "I am a pear" }

                    }
                },
                command);
            writer.Write(
                new EntityQuery<Blog>(schema.GetDefaultCollection<Blog>()) {
                    WhereClause = "Foo = @Foo and Food = @Food",
                    WhereClauseParameters = new Dictionary<string, object> {
                        { "Foo", "I am an enumerable string" } ,
                        { "Food", "I am a pear" }

                    }
                },
                command);
            this.outputHelper.WriteLine(command.Queries.First());
            Assert.Equal("select t.[BlogId], t.[Document], t.[DocumentType], t.[Version] from [dbo].[Blogs] as t where (Foo = @Foo and Food = @Food)", command.Queries.First());
            Assert.Equal("select t.[BlogId], t.[Document], t.[DocumentType], t.[Version] from [dbo].[Blogs] as t where (Foo = @Foo_3 and Food = @Food_4)", command.Queries.ElementAt(1));
            Assert.Equal(4, command.Parameters.Count());
        }
    }
}