namespace Leap.Data.Tests {
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Leap.Data.Internal.QueryWriter.SqlServer;
    using Leap.Data.Internal.UpdateWriter.SqlServer;

    using Microsoft.Data.SqlClient;

    using Moq;

    using Newtonsoft.Json;

    using Xunit;

    public class Playground {
        [Fact]
        public async Task MultipleWorks() {
            var insertSession = MakeTarget();
            var blogTitle = $"Blog from {DateTime.UtcNow}";
            var newBlog = new Blog(blogTitle);
            insertSession.Add(newBlog);
            var blog2Title = $"Blog 2 from {DateTime.UtcNow}";
            var newBlog2 = new Blog(blog2Title);
            insertSession.Add(newBlog2);
            await insertSession.SaveChangesAsync();

            var fetchSession = MakeTarget();
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
            var session = MakeTarget();
            var blog = new Blog("My blog");
            session.Add(blog);
            var sameBlog = await session.Get<Blog>().SingleAsync(blog.BlogId);
            Assert.Same(blog, sameBlog);
            await session.SaveChangesAsync();


            var secondSession = MakeTarget();
            var fetchedBlog = await secondSession.Get<Blog>().SingleAsync(blog.BlogId);
            Assert.NotNull(fetchedBlog);
            Assert.Equal(blog.Title, fetchedBlog.Title);
            Assert.Equal(blog.BlogId, fetchedBlog.BlogId);
        }
        
        [Fact]
        public async Task ItWorks() {
            var insertSession = MakeTarget();
            var blogTitle = $"Blog from {DateTime.UtcNow}";
            var newBlog = new Blog(blogTitle);
            insertSession.Add(newBlog);
            await insertSession.SaveChangesAsync();

            var fetchSession = MakeTarget();
            var blog = await fetchSession.Get<Blog>().SingleAsync(newBlog.BlogId);
            Assert.Equal(blogTitle, blog.Title);
        }

        [Fact]
        public async Task FutureKeyWorks() {
            var session = MakeTarget();
            var blogFuture = session.Get<Blog>().SingleFuture(new BlogId() { Id = Guid.Parse("77b55913-d2b6-488d-8860-3e8e70cb5146") });
            var blogNow = await session.Get<Blog>().SingleAsync(new BlogId() { Id = Guid.Parse("77b55913-d2b6-488d-8860-3e8e70cb5146") });
            var blogFromFuture = await blogFuture.SingleAsync();
            Assert.Same(blogNow, blogFromFuture);
        }
        
        [Fact]
        public async Task AllTheOperationsInOne()
        {
            var session = MakeTarget();
            var firstBlog = new Blog("My first blog");
            session.Add(firstBlog);
            var secondBlog = new Blog("My second blog");
            session.Add(secondBlog);
            await session.SaveChangesAsync();
            
            var secondSession = MakeTarget();
            var firstBlogAgain = await secondSession.Get<Blog>().SingleAsync(firstBlog.BlogId);
            var secondBlogAgain = await secondSession.Get<Blog>().SingleAsync(secondBlog.BlogId);
            var thirdBlog = new Blog("My third blog");
            secondSession.Add(thirdBlog);
            secondSession.Delete(firstBlogAgain);
            secondBlogAgain.Title = "My updated second blog";
            await secondSession.SaveChangesAsync();

            var thirdSession = MakeTarget();
            var firstBlogAgainAgain = await thirdSession.Get<Blog>().SingleAsync(firstBlog.BlogId);
            var secondBlogAgainAgain = await thirdSession.Get<Blog>().SingleAsync(secondBlog.BlogId);
            var thirdBlogAgainAgain = await thirdSession.Get<Blog>().SingleAsync(thirdBlog.BlogId);
            Assert.Null(firstBlogAgainAgain);
            Assert.NotNull(secondBlogAgainAgain);
            Assert.Equal(secondBlogAgain.Title, secondBlogAgainAgain.Title);
            Assert.NotNull(thirdBlogAgainAgain);
        }

        private static Session MakeTarget() {
            var connectionFactory = new Mock<IConnectionFactory>();
            var connection = new SqlConnection("Server=.;Database=leap-data;Trusted_Connection=True;");
            connectionFactory.Setup(c => c.Get()).Returns(connection);

            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(s => s.Deserialize(It.IsAny<Type>(), It.IsAny<string>())).Returns((Type type, string json) => JsonConvert.DeserializeObject(json, type));
            mockSerializer.Setup(s => s.Serialize(It.IsAny<object>())).Returns((object obj) => JsonConvert.SerializeObject(obj));

            var mockSchema = MockSchema.GetMockSchema();
            
            var session = new Session(connectionFactory.Object, mockSchema, mockSerializer.Object, new SqlServerSqlQueryWriter(mockSchema), new SqlServerSqlUpdateWriter(mockSchema, mockSerializer.Object));
            return session;
        }
    }
}