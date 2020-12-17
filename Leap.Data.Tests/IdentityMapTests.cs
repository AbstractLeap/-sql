namespace Leap.Data.Tests {
    using Leap.Data.IdentityMap;
    using Leap.Data.Schema;

    using Moq;

    using Xunit;

    public class IdentityMapTests
    {
        [Fact]
        public void ItWorks() {
            var mockSchema = new Mock<ISchema>();
            mockSchema.Setup(s => s.GetTable<Blog>()).Returns(new Table { Name = "Blogs", Schema = "dbo", KeyType = typeof(BlogId) });
            var identityMap = new IdentityMap(mockSchema.Object);

            var blog = new Blog("Title");
            identityMap.Add(blog.BlogId, blog);
            
            Assert.True(identityMap.TryGetValue<Blog, BlogId>(blog.BlogId, out var mappedBlog));
            Assert.NotNull(mappedBlog);
            Assert.Same(blog, mappedBlog);
            
            Assert.True(identityMap.TryGetValue(typeof(BlogId), blog.BlogId, out Blog weakBlog));
            Assert.NotNull(weakBlog);
            Assert.Same(blog, weakBlog);
        }
    }
}