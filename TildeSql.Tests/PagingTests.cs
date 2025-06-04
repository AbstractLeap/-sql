namespace TildeSql.Tests {
    using System.Linq;
    using System.Threading.Tasks;

    using TildeSql.Tests.TestDomain.Blog;
    using TildeSql.Tests.TestDomain.Paging;

    using Xunit;

    public class PagingTests {
        [Fact]
        public async Task TotalWorks() {
            var sessionFactory = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var insertSession = sessionFactory.StartSession();
            foreach (var i in Enumerable.Range(1, 9)) {
                var blogTitle = $"Blog Title{i % 2}";
                var newBlog = new Blog(blogTitle);
                insertSession.Add(newBlog);
            }

            await insertSession.SaveChangesAsync();

            var session = sessionFactory.StartSession();
            var blogs = await session.Get<Blog>().Where("json_value(document, '$.title') like '%Title0'").OrderBy("BlogId").Limit(2, out var countAccessor).ToArrayAsync();
            Assert.Equal(4, countAccessor.Count);
            Assert.Equal(2, blogs.Length);
        }

        [Fact]
        public async Task NoWhereWorks() {
            var sessionFactory = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var insertSession = sessionFactory.StartSession();
            foreach (var i in Enumerable.Range(1, 9)) {
                var title = $"Paged Title{i % 2}";
                var thing = new Paged(title);
                insertSession.Add(thing);
            }

            await insertSession.SaveChangesAsync();

            var session = sessionFactory.StartSession();
            var things = await session.Get<Paged>().OrderBy("Id").Limit(2, out var countAccessor).ToArrayAsync();
            Assert.Equal(9, countAccessor.Count);
            Assert.Equal(2, things.Length);
        }
    }
}