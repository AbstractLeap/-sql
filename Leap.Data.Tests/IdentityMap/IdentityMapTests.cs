namespace Leap.Data.Tests {
    using System;

    using Leap.Data.IdentityMap;

    using Xunit;

    public class IdentityMapTests {
        [Fact]
        public void ItWorks() {
            var identityMap = new IdentityMap();

            var blog = new Blog("Title");
            identityMap.Add(blog.BlogId, blog);

            Assert.True(identityMap.TryGetValue<Blog, BlogId>(blog.BlogId, out var mappedBlogDocument));
            Assert.NotNull(mappedBlogDocument);
            Assert.Same(blog, mappedBlogDocument);

            Assert.True(identityMap.TryGetValue(typeof(BlogId), blog.BlogId, out Blog weakBlog));
            Assert.NotNull(weakBlog);
            Assert.Same(blog, weakBlog);
        }

        [Fact]
        public void Contravariance() {
            var identityMap = new IdentityMap();
            var id = Guid.NewGuid();
            var fooDoc = new Foo();
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