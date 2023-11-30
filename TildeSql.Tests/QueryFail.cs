namespace TildeSql.Tests {
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using TildeSql.Tests.TestDomain.Blog;

    using Xunit;

    public class QueryFail {
        [Fact]
        public async Task MultipleQueryFailsWithUpdate() {
            var sessionFactory = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var insertSession = sessionFactory.StartSession();
            var now = DateTime.UtcNow;
            var blogTitle = $"{now.Ticks} top blog ideas";
            var newBlog = new Blog(blogTitle);
            insertSession.Add(newBlog);
            await insertSession.SaveChangesAsync();

            var session = sessionFactory.StartSession();
            var blog = await session.Get<Blog>().Where($"json_value(document, '$.\"<Title>k__BackingField\"') like '{now.Ticks}%'").SingleAsync();
            var blogAgain = await session.Get<Blog>().Where($"json_value(document, '$.\"<Title>k__BackingField\"') like '{now.Ticks}%'").SingleAsync();

            Assert.Same(blog, blogAgain);

            blog.Title = blogTitle + " (Updated)";
            await session.SaveChangesAsync();
        }
    }
}