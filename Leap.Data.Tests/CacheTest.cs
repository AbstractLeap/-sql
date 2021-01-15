namespace Leap.Data.Tests {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Leap.Data.Configuration;
    using Leap.Data.Internal;
    using Leap.Data.MemoryCache;

    using Moq;

    using Xunit;

    public class CacheTest {
        [Fact]
        public async Task SessionFactoryCacheWorks() {
            var sf = MakeTarget();
            var session = sf.StartSession();
            var blog = new Blog("Caching uncovered");
            session.Add(blog);
            await session.SaveChangesAsync();

            var session2 = sf.StartSession();
            var blogAgain = await session2.Get<Blog>().SingleAsync(blog.BlogId);
            Assert.Equal(blog, blogAgain);
            Assert.NotSame(blogAgain, blog);
        }

        private static ISessionFactory MakeTarget() {
            var testSchema = TestSchema.Get();
            var configuration = new Configuration(testSchema).UseMemoryCache();

            // should never get called
            var mockQueryExecutor = new Mock<IQueryExecutor>();
            configuration.QueryExecutor = mockQueryExecutor.Object;

            var mockUpdateExecutor = new Mock<IUpdateExecutor>();
            mockUpdateExecutor
                .Setup(
                    e => e.ExecuteAsync(
                        It.IsAny<IEnumerable<DatabaseRow>>(),
                        It.IsAny<IEnumerable<(DatabaseRow, DatabaseRow)>>(),
                        It.IsAny<IEnumerable<DatabaseRow>>(),
                        It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);
            configuration.UpdateExecutor = mockUpdateExecutor.Object;

            return configuration.BuildSessionFactory();
        }
    }
}