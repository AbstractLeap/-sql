namespace TildeSql.Tests {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;

    using StackExchange.Profiling.Data;

    using TildeSql.Configuration;
    using TildeSql.SqlServer;
    using TildeSql.Tests.TestDomain.Blog;

    using Xunit;

    public class CacheTest {
        [Fact]
        public async Task CacheMultipleWorks() {
            var profiler = new Profiler();
            var schema = TestSchemaBuilder.Build();
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var sf = TestSessionFactoryBuilder.Build(
                schema,
                sqlSetup: s => s.ConnectionFactoryFactory = new ProfilingDbConnectionFactoryFactory(
                                   new ConnectionPerCommandSqlServerConnectionFactoryFactory(TestSessionFactoryBuilder.SqlServerConnectionString),
                                   profiler),
                configSetup: c => c.UseMemoryCache(memoryCache).EnableCaching<Blog>(TimeSpan.FromMinutes(5)));

            var session = sf.StartSession();
            var blog = new Blog("Caching uncovered");
            session.Add(blog);
            var blog2 = new Blog("Caching again");
            session.Add(blog2);
            await session.SaveChangesAsync();

            memoryCache.Clear();
            profiler.IsActive = true;
            var tasks = new List<ValueTask<Blog[]>>();
            var keys = new[] { blog.BlogId, blog2.BlogId };
            foreach (var i in Enumerable.Range(0, 1000)) {
                var innerSession = sf.StartSession();
                tasks.Add(innerSession.Get<Blog>().MultipleAsync(keys).ToArrayAsync());
            }

            await Task.WhenAll(tasks.Select(v => v.AsTask()));

            Assert.Equal(1, profiler.CommandsExecuted);
            var blogInstances = new HashSet<Blog>(tasks.SelectMany(v => v.Result), ReferenceEqualityComparer.Instance);
            Assert.Equal(2000, blogInstances.Count);
        }

        [Fact]
        public async Task InMemoryCacheWorks() {
            var profiler = new Profiler();
            var schema = TestSchemaBuilder.Build();
            var sf = TestSessionFactoryBuilder.Build(
                schema,
                sqlSetup: s => s.ConnectionFactoryFactory = new ProfilingDbConnectionFactoryFactory(
                                   new ConnectionPerCommandSqlServerConnectionFactoryFactory(TestSessionFactoryBuilder.SqlServerConnectionString),
                                   profiler),
                configSetup: c => c.UseMemoryCache(new MemoryCache(Options.Create(new MemoryCacheOptions()))).EnableCaching<Blog>(TimeSpan.FromMinutes(5)));

            var session = sf.StartSession();
            var blog = new Blog("Caching uncovered");
            session.Add(blog);
            await session.SaveChangesAsync();

            var session2 = sf.StartSession();
            var blog2 = await session2.Get<Blog>().SingleAsync(blog.BlogId);

            profiler.IsActive = true;
            var session3 = sf.StartSession();
            var blog3 = await session3.Get<Blog>().SingleAsync(blog.BlogId);

            Assert.Equal(0, profiler.CommandsExecuted);
        }

        [Fact]
        public async Task StampedeProtectionWorks() {
            var profiler = new Profiler();
            var schema = TestSchemaBuilder.Build();
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var sf = TestSessionFactoryBuilder.Build(
                schema,
                sqlSetup: s => s.ConnectionFactoryFactory = new ProfilingDbConnectionFactoryFactory(
                                   new ConnectionPerCommandSqlServerConnectionFactoryFactory(TestSessionFactoryBuilder.SqlServerConnectionString),
                                   profiler),
                configSetup: c => { c.UseMemoryCache(memoryCache).EnableCaching<Blog>(TimeSpan.FromMinutes(5)); });

            var session = sf.StartSession();
            var blog = new Blog("Caching uncovered");
            session.Add(blog);
            await session.SaveChangesAsync();

            // clear the cache
            // then execute the same query against 1000 different sessions
            // (so we don't hit the identity map)
            // and ensure only 1 actual request hits the database
            memoryCache.Clear();
            profiler.IsActive = true;
            var tasks = new List<ValueTask<Blog>>();
            foreach (var i in Enumerable.Range(0, 1000)) {
                var innerSession = sf.StartSession();
                tasks.Add(innerSession.Get<Blog>().SingleAsync(blog.BlogId));
            }

            await Task.WhenAll(tasks.Select(v => v.AsTask()));

            Assert.Equal(1, profiler.CommandsExecuted);
            Assert.All(tasks, v => Assert.Equal(v.Result.BlogId, blog.BlogId));
            var blogInstances = new HashSet<Blog>(tasks.Select(v => v.Result), ReferenceEqualityComparer.Instance);
            Assert.Equal(1000, blogInstances.Count);
        }
    }

    public class Profiler : IDbProfiler {
        public void ExecuteStart(IDbCommand profiledDbCommand, SqlExecuteType executeType) {
            this.CommandsExecuted++;
        }

        public void ExecuteFinish(IDbCommand profiledDbCommand, SqlExecuteType executeType, DbDataReader reader) { }

        public void ReaderFinish(IDataReader reader) { }

        public void OnError(IDbCommand profiledDbCommand, SqlExecuteType executeType, Exception exception) { }

        public bool IsActive { get; set; }

        public int CommandsExecuted { get; private set; }
    }

    public class ProfilingDbConnectionFactoryFactory : IConnectionFactoryFactory {
        private readonly IConnectionFactoryFactory factory;

        private readonly IDbProfiler profiler;

        public ProfilingDbConnectionFactoryFactory(IConnectionFactoryFactory factory, IDbProfiler profiler) {
            this.factory  = factory;
            this.profiler = profiler;
        }

        public IConnectionFactory Get() {
            return new ProfilingDbConnectionFactory(this.factory.Get(), this.profiler);
            ;
        }
    }

    public class ProfilingDbConnectionFactory : IConnectionFactory {
        private readonly IConnectionFactory implementation;

        private readonly IDbProfiler profiler;

        public ProfilingDbConnectionFactory(IConnectionFactory implementation, IDbProfiler profiler) {
            this.implementation = implementation;
            this.profiler       = profiler;
        }

        public async ValueTask<DbConnection> GetAsync() {
            return new ProfiledDbConnection(await implementation.GetAsync(), profiler);
        }
    }
}