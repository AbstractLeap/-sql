namespace TildeSql.Tests {
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using TildeSql.Tests.TestDomain.Blog;

    public class Api {
        /// <summary>
        ///     This method simply gives a "feeling" of how to use the api
        /// </summary>
        /// <returns></returns>
        public async Task AestheticTest() {
            var blogId1 = new BlogId();
            var blogId2 = new BlogId();
            var session = this.GetSession();
            var entity = await session.Get<Blog>().SingleAsync(blogId1);
            var entitiesEnumerable = session.Get<Blog>().MultipleAsync(new [] { blogId1, blogId2 });
            await foreach (var asyncEntity in entitiesEnumerable) { }

            var entities = await entitiesEnumerable.ToListAsync();

            var futureEntityFuture = session.Get<Blog>().SingleFuture(blogId1);
            var futureEntity = await futureEntityFuture.SingleAsync();

            var futureEntitiesFuture = session.Get<Blog>().MultipleFuture(new[] { blogId1, blogId2 });
            var futureEntities = await futureEntitiesFuture.ToListAsync();

            var blog = new Blog("My blog");
            session.Add(blog);
            session.Delete(blog);

            var queryEntitiesFuture = session.Get<Blog>().Where(string.Empty).Limit(10).Future();
            var queryEntitiesFromFuture = await queryEntitiesFuture.ToListAsync();

            var queryEntities = await session.Get<Blog>().Where(string.Empty).ToListAsync();

            entity.Title = "Foo";
            await session.SaveChangesAsync();
        }

        ISession GetSession() {
            throw new NotImplementedException();
        }
    }
}