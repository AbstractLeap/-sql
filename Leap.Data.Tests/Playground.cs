namespace Leap.Data.Tests {
    using System;
    using System.Threading.Tasks;

    using Leap.Data.Internal.QueryWriter.SqlServer;

    using Microsoft.Data.SqlClient;

    using Moq;

    using Newtonsoft.Json;

    using Xunit;

    public class Playground {
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
            var session = MakeTarget();
            var blog = await session.Get<Blog>().SingleAsync(new BlogId() { Id = Guid.Parse("77b55913-d2b6-488d-8860-3e8e70cb5146") });
            Assert.Equal("Foo", blog.Title);
        }

        [Fact]
        public async Task FutureKeyWorks() {
            var session = MakeTarget();
            var blogFuture = session.Get<Blog>().SingleFuture(new BlogId());
            var blogNow = await session.Get<Blog>().SingleAsync(new BlogId());
            var blogFromFuture = await blogFuture.SingleAsync();
            Assert.Same(blogNow, blogFromFuture);
        }

        private static Session MakeTarget() {
            var connectionFactory = new Mock<IConnectionFactory>();
            var connection = new SqlConnection("Server=.;Database=leap-data;Trusted_Connection=True;");
            connectionFactory.Setup(c => c.Get()).Returns(connection);

            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(s => s.Deserialize(It.IsAny<Type>(), It.IsAny<string>())).Returns((Type type, string json) => JsonConvert.DeserializeObject(json, type));
            mockSerializer.Setup(s => s.Serialize(It.IsAny<object>())).Returns((object obj) => JsonConvert.SerializeObject(obj));

            var mockSchema = MockSchema.GetMockSchema();
            
            var session = new Session(connectionFactory.Object, mockSchema, mockSerializer.Object, new SqlServerSqlQueryWriter(mockSchema));
            return session;
        }
    }
}