namespace Leap.Data.Tests {
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Leap.Data.Configuration;
    using Leap.Data.MemoryCache;
    using Leap.Data.SqlServer;

    using Xunit;

    public class Playground {
        [Fact]
        public async Task QueryWorks() {
            var sessionFactory = MakeTarget();
            var insertSession = sessionFactory.StartSession();
            var blogTitle = $"Query Blog from {DateTime.UtcNow}";
            var newBlog = new Blog(blogTitle);
            insertSession.Add(newBlog);
            await insertSession.SaveChangesAsync();

            var session = sessionFactory.StartSession();
            var blogs = await session.Get<Blog>().ToArrayAsync();
            Assert.True(blogs.Length > 0);
        }

        [Fact]
        public async Task MultipleWorks() {
            var sessionFactory = MakeTarget();
            var insertSession = sessionFactory.StartSession();
            var blogTitle = $"Blog from {DateTime.UtcNow}";
            var newBlog = new Blog(blogTitle);
            insertSession.Add(newBlog);
            var blog2Title = $"Blog 2 from {DateTime.UtcNow}";
            var newBlog2 = new Blog(blog2Title);
            insertSession.Add(newBlog2);
            await insertSession.SaveChangesAsync();

            var fetchSession = sessionFactory.StartSession();
            var blogsFuture = fetchSession.Get<Blog>().MultipleFuture(newBlog.BlogId, newBlog2.BlogId);
            var blogsNow = await fetchSession.Get<Blog>().MultipleAsync(newBlog.BlogId, newBlog2.BlogId).ToArrayAsync();
            var blogsFromFuture = await blogsFuture.ToArrayAsync();
            Assert.Equal(2, blogsNow.Length);
            Assert.Equal(2, blogsFromFuture.Length);

            var blogNow1 = blogsNow.Single(b => b.BlogId == newBlog.BlogId);
            var blogNow2 = blogsNow.Single(b => b.BlogId == newBlog2.BlogId);
            var blogFuture1 = blogsFromFuture.Single(b => b.BlogId == newBlog.BlogId);
            var blogFuture2 = blogsFromFuture.Single(b => b.BlogId == newBlog2.BlogId);
            Assert.Same(blogNow1, blogFuture1);
            Assert.Same(blogNow2, blogFuture2);
        }

        [Fact]
        public async Task ItRoundTrips() {
            var sessionFactory = MakeTarget();
            var session = sessionFactory.StartSession();
            var blog = new Blog("My blog");
            session.Add(blog);
            var sameBlog = await session.Get<Blog>().SingleAsync(blog.BlogId);
            Assert.Same(blog, sameBlog);
            await session.SaveChangesAsync();

            var secondSession = sessionFactory.StartSession();
            var fetchedBlog = await secondSession.Get<Blog>().SingleAsync(blog.BlogId);
            Assert.NotNull(fetchedBlog);
            Assert.Equal(blog.Title, fetchedBlog.Title);
            Assert.Equal(blog.BlogId, fetchedBlog.BlogId);
        }

        [Fact]
        public async Task ItWorks() {
            var sessionFactory = MakeTarget();
            var insertSession = sessionFactory.StartSession();
            var blogTitle = $"Blog from {DateTime.UtcNow}";
            var newBlog = new Blog(blogTitle);
            insertSession.Add(newBlog);
            await insertSession.SaveChangesAsync();

            var fetchSession = sessionFactory.StartSession();
            var blog = await fetchSession.Get<Blog>().SingleAsync(newBlog.BlogId);
            Assert.Equal(blogTitle, blog.Title);
        }

        [Fact]
        public async Task FutureKeyWorks() {
            var sessionFactory = MakeTarget();
            var session = sessionFactory.StartSession();
            var blogFuture = session.Get<Blog>().SingleFuture(new BlogId { Id = Guid.Parse("77b55913-d2b6-488d-8860-3e8e70cb5146") });
            var blogNow = await session.Get<Blog>().SingleAsync(new BlogId { Id = Guid.Parse("77b55913-d2b6-488d-8860-3e8e70cb5146") });
            var blogFromFuture = await blogFuture.SingleAsync();
            Assert.Same(blogNow, blogFromFuture);
        }

        [Fact]
        public async Task AllTheOperationsInOne() {
            var sessionFactory = MakeTarget();
            var firstSession = sessionFactory.StartSession();
            var firstBlog = new Blog("My first blog");
            firstSession.Add(firstBlog);
            var secondBlog = new Blog("My second blog");
            firstSession.Add(secondBlog);
            await firstSession.SaveChangesAsync();

            var secondSession = sessionFactory.StartSession();
            var firstBlogAgain = await secondSession.Get<Blog>().SingleAsync(firstBlog.BlogId);
            var secondBlogAgain = await secondSession.Get<Blog>().SingleAsync(secondBlog.BlogId);
            var thirdBlog = new Blog("My third blog");
            secondSession.Add(thirdBlog);
            secondSession.Delete(firstBlogAgain);
            secondBlogAgain.Title = "My updated second blog";
            await secondSession.SaveChangesAsync();

            var thirdSession = sessionFactory.StartSession();
            var firstBlogAgainAgain = await thirdSession.Get<Blog>().SingleAsync(firstBlog.BlogId);
            var secondBlogAgainAgain = await thirdSession.Get<Blog>().SingleAsync(secondBlog.BlogId);
            var thirdBlogAgainAgain = await thirdSession.Get<Blog>().SingleAsync(thirdBlog.BlogId);
            Assert.Null(firstBlogAgainAgain);
            Assert.NotNull(secondBlogAgainAgain);
            Assert.Equal(secondBlogAgain.Title, secondBlogAgainAgain.Title);
            Assert.NotNull(thirdBlogAgainAgain);
        }

        [Fact]
        public async Task DeletedShouldBeRemovedCompletely() {
            var sessionFactory = MakeTarget();
            var insertSession = sessionFactory.StartSession();
            var addedBlog = new Blog("Blog to be deleted");
            insertSession.Add(addedBlog);
            await insertSession.SaveChangesAsync();

            var deleteSession = sessionFactory.StartSession();
            var toBeDeleted = await deleteSession.Get<Blog>().SingleAsync(addedBlog.BlogId);
            deleteSession.Delete(toBeDeleted);
            await deleteSession.SaveChangesAsync();

            var shouldHaveBeenDeleted = await deleteSession.Get<Blog>().SingleAsync(toBeDeleted.BlogId);
            Assert.Null(shouldHaveBeenDeleted);
        }

        [Fact]
        public async Task OptimisticConcurrenyFail() {
            var sessionFactory = MakeTarget();
            var addedBlog = await AddBlog(sessionFactory, "Pessimistic blog");

            var session1 = sessionFactory.StartSession();
            var session1Blog = await session1.Get<Blog>().SingleAsync(addedBlog.BlogId);
            var session2 = sessionFactory.StartSession();
            var session2Blog = await session2.Get<Blog>().SingleAsync(addedBlog.BlogId);
            Assert.Equal(session1Blog.BlogId, session2Blog.BlogId);
            Assert.NotSame(session1Blog, session2Blog);

            session1Blog.Title = "Optimistic Blog";
            await session1.SaveChangesAsync();

            session2Blog.Title = "Doomed to failure";
            await Assert.ThrowsAsync<AggregateException>(async () => await session2.SaveChangesAsync());
        }

        [Fact]
        public async Task NonExecutedFuturesExecutedBeforeUpdate() {
            var sessionFactory = MakeTarget();
            var toBeBlog = await AddBlog(sessionFactory, "Future");
            var nowBlog = await AddBlog(sessionFactory, "Now");

            var session = sessionFactory.StartSession();
            var nowBlogAgain = await session.Get<Blog>().SingleAsync(nowBlog.BlogId);
            var futureRequest = session.Get<Blog>().SingleFuture(toBeBlog.BlogId);
            nowBlogAgain.Title = "Now now";
            await session.SaveChangesAsync();
            
            Assert.NotNull(await futureRequest.SingleAsync());
        }

        private static async Task<Blog> AddBlog(ISessionFactory sessionFactory, string title) {
            var insertSession = sessionFactory.StartSession();
            var addedBlog = new Blog(title);
            insertSession.Add(addedBlog);
            await insertSession.SaveChangesAsync();
            return addedBlog;
        }

        private static ISessionFactory MakeTarget() {
            var testSchema = TestSchema.Get();
            var sessionFactory = new Configuration(testSchema)
                                 .UseSqlServer("Server=.;Database=leap-data;Trusted_Connection=True;")
                                 .UseMemoryCache()
                                 .BuildSessionFactory();
            return sessionFactory;
        }
    }
}