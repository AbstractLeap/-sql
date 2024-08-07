namespace TildeSql.Tests {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using TildeSql.Tests.TestDomain.Blog;
    using TildeSql.Utilities;

    using Xunit;

    public class Playground {
        [Fact]
        public async Task QueryWorks() {
            var sessionFactory = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
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
            var sessionFactory = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var insertSession = sessionFactory.StartSession();
            var blogTitle = $"Blog from {DateTime.UtcNow}";
            var newBlog = new Blog(blogTitle);
            insertSession.Add(newBlog);
            var blog2Title = $"Blog 2 from {DateTime.UtcNow}";
            var newBlog2 = new Blog(blog2Title);
            insertSession.Add(newBlog2);
            await insertSession.SaveChangesAsync();

            var fetchSession = sessionFactory.StartSession();
            var blogsFuture = fetchSession.Get<Blog>().MultipleFuture(new [] { newBlog.BlogId, newBlog2.BlogId });
            var blogsNow = await fetchSession.Get<Blog>().MultipleAsync(new [] { newBlog.BlogId, newBlog2.BlogId }).ToArrayAsync();
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
            var sessionFactory = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
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
            var sessionFactory = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
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
            var sessionFactory = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var session = sessionFactory.StartSession();
            var blogFuture = session.Get<Blog>().SingleFuture(new BlogId { Id = Guid.Parse("77b55913-d2b6-488d-8860-3e8e70cb5146") });
            var blogNow = await session.Get<Blog>().SingleAsync(new BlogId { Id = Guid.Parse("77b55913-d2b6-488d-8860-3e8e70cb5146") });
            var blogFromFuture = await blogFuture.SingleAsync();
            Assert.Same(blogNow, blogFromFuture);
        }

        [Fact]
        public async Task AllTheOperationsInOne() {
            var sessionFactory = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
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
            var sessionFactory = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
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
            var sessionFactory = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
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
            var sessionFactory = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var toBeBlog = await AddBlog(sessionFactory, "Future");
            var nowBlog = await AddBlog(sessionFactory, "Now");

            var session = sessionFactory.StartSession();
            var nowBlogAgain = await session.Get<Blog>().SingleAsync(nowBlog.BlogId);
            var futureRequest = session.Get<Blog>().SingleFuture(toBeBlog.BlogId);
            nowBlogAgain.Title = "Now now";
            await session.SaveChangesAsync();

            Assert.NotNull(await futureRequest.SingleAsync());
        }

        [Fact]
        public async Task AddRemoveInSession() {
            var sessionFactory = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            await using var session = sessionFactory.StartSession();
            var newBlog = new Blog("Gonna remove this");
            session.Add(newBlog);
            session.Delete(newBlog);
            await session.SaveChangesAsync();

            await using var session2 = sessionFactory.StartSession();
            var blog = await session2.Get<Blog>().SingleAsync(newBlog.BlogId);
            Assert.Null(blog);
        }

        [Fact]
        public async Task BatchedUpdates() {
            var sessionFactory = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            await using var insertSession = sessionFactory.StartSession();

            var blogs = new List<Blog>();
            foreach (var i in Enumerable.Range(0, 2000)) {
                var blog = new Blog($"Blog {i + 1}");
                blogs.Add(blog);
                insertSession.Add(blog);
            }

            await insertSession.SaveChangesAsync(); // shouldn't throw due to many params

            foreach (var entry in blogs.AsSmartEnumerable()) {
                var blog = entry.Value;
                if (entry.Index % 3 == 0) {
                    insertSession.Delete(blog);
                }
                else {
                    blog.Title += " (Updated)";
                }
            }

            await insertSession.SaveChangesAsync();

            var insertedBlogIds = blogs.Select(b => b.BlogId).ToHashSet();
            await using var selectSession = sessionFactory.StartSession();
            var remainingBlogs = (await selectSession.Get<Blog>().ToListAsync()).Where(b => insertedBlogIds.Contains(b.BlogId)).ToArray();
            Assert.All(remainingBlogs, b => Assert.EndsWith("(Updated)", b.Title));
            Assert.Equal(1333, remainingBlogs.Length);
        }

        private static async Task<Blog> AddBlog(ISessionFactory sessionFactory, string title) {
            var insertSession = sessionFactory.StartSession();
            var addedBlog = new Blog(title);
            insertSession.Add(addedBlog);
            await insertSession.SaveChangesAsync();
            return addedBlog;
        }
    }
}