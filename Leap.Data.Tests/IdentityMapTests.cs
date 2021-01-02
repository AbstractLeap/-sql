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
            mockSchema.Setup(s => s.GetTable<Blog>()).Returns(new Table (mockSchema.Object) { Name = "Blogs", Schema = "dbo", KeyType = typeof(BlogId) });
            var identityMap = new IdentityMap(mockSchema.Object);

            var blog = new Blog("Title");
            identityMap.Add(blog.BlogId, new Document<Blog>(null, blog));
            
            Assert.True(identityMap.TryGetValue<Blog, BlogId>(blog.BlogId, out var mappedBlogDocument));
            Assert.NotNull(mappedBlogDocument);
            Assert.Same(blog, mappedBlogDocument.Entity);
            
            Assert.True(identityMap.TryGetValue(typeof(BlogId), blog.BlogId, out Document<Blog> weakBlog));
            Assert.NotNull(weakBlog);
            Assert.Same(blog, weakBlog.Entity);
        }
    }
}