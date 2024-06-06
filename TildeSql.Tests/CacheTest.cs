﻿namespace TildeSql.Tests {
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Moq;

    using TildeSql.Configuration;
    using TildeSql.Internal;
    using TildeSql.Tests.TestDomain.Blog;

    using Xunit;
    using TildeSql.Internal.Caching;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;

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
            var testSchema = TestSchemaBuilder.Build();
            var configuration = new Configuration(testSchema)
                .UseMemoryCache(new MemoryCache(Options.Create(new MemoryCacheOptions())))
                .EnableCaching<Blog>(TimeSpan.FromMinutes(5));

            // should never get called
            var mockQueryExecutor = new Mock<IPersistenceQueryExecutor>();
            configuration.QueryExecutorFactory = () => mockQueryExecutor.Object;

            var mockUpdateExecutor = new Mock<IUpdateExecutor>();
            mockUpdateExecutor
                .Setup(
                    e => e.ExecuteAsync(
                        It.IsAny<IEnumerable<DatabaseRow>>(),
                        It.IsAny<IEnumerable<(DatabaseRow, DatabaseRow)>>(),
                        It.IsAny<IEnumerable<DatabaseRow>>(),
                        It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);
            configuration.UpdateExecutorFactory = () => mockUpdateExecutor.Object;

            return configuration.BuildSessionFactory();
        }
    }
}