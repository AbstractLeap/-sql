namespace Leap.Data.Tests {
    using System;
    using System.Linq;

    using Leap.Data.IdentityMap;
    using Leap.Data.Schema;

    using Moq;

    using Xunit;

    public class IdentityMapTests {
        [Fact]
        public void ItWorks() {
            var mockSchema = new Mock<ISchema>();
            mockSchema.Setup(s => s.GetTable<Blog>()).Returns(new Table("Blogs", "dbo", typeof(BlogId), Enumerable.Empty<(Type, string)>()));
            var identityMap = new IdentityMap(mockSchema.Object);

            var blog = new Blog("Title");
            identityMap.Add(blog.BlogId, new Document<Blog>(blog));

            Assert.True(identityMap.TryGetValue<Blog, BlogId>(blog.BlogId, out var mappedBlogDocument));
            Assert.NotNull(mappedBlogDocument);
            Assert.Same(blog, mappedBlogDocument.Entity);

            Assert.True(identityMap.TryGetValue(typeof(BlogId), blog.BlogId, out IDocument<Blog> weakBlog));
            Assert.NotNull(weakBlog);
            Assert.Same(blog, weakBlog.Entity);
        }

        [Fact]
        public void Contravariance() {
            var schema = new SchemaBuilder().AddTypes("Foos", typeof(IFoo), typeof(BaseFoo), typeof(Foo), typeof(FooFoo)).Build();
            var identityMap = new IdentityMap(schema);
            var id = Guid.NewGuid();
            var fooDoc = new Document<Foo>(new Foo());
            identityMap.Add(id, fooDoc);

            Assert.True(identityMap.TryGetValue<BaseFoo, Guid>(id, out var baseFoo));
            Assert.Same(fooDoc, baseFoo);
        }

        interface IFoo {
            Guid Id { get; }
        }

        abstract class BaseFoo : IFoo {
            public Guid Id { get; }
        }

        class Foo : BaseFoo { }

        class FooFoo : Foo { }
    }
}