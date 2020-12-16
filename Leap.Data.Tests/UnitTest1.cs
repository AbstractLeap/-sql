using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Leap.Data.Tests
{
    using System.Linq;

    using Moq;

    public class IdentityMapTests
    {
        [Fact]
        public void ItWorks() {
            var mockSchema = new Mock<ISchema>();
            mockSchema.Setup(s => s.GetTable<Blog>()).Returns(new Table { Name = "Blogs", Schema = "dbo", KeyType = typeof(BlogId) });
            var identityMap = new IdentityMap(mockSchema.Object);

            var blog = new Blog(new BlogId("f"), "Title");
            identityMap.Add(blog.BlogId, blog);
            
            Assert.True(identityMap.TryGetValue<Blog, BlogId>(blog.BlogId, out var mappedBlog));
            Assert.NotNull(mappedBlog);
            Assert.Same(blog, mappedBlog);
            
            Assert.True(identityMap.TryGetValue(typeof(BlogId), blog.BlogId, out Blog weakBlog));
            Assert.NotNull(weakBlog);
            Assert.Same(blog, weakBlog);
        }
    }

    public class UnitTest1
    {
        [Fact]
        public async Task Test1() {
            var session = this.GetSession();
            var entity = await session.Get<Blog>().ByKeyAsync(new BlogId(""));
            var entitiesEnumerable = session.Get<Blog>().ByKeyAsync(new BlogId(""), new BlogId(""));
            await foreach (var asyncEntity in entitiesEnumerable) {
                
            }

            var entities = await entitiesEnumerable.ToListAsync();

            var futureEntityFuture = session.Get<Blog>().ByKeyInTheFuture(new BlogId(""));
            var futureEntity = await futureEntityFuture.SingleAsync();

            var futureEntitiesFuture = session.Get<Blog>().ByKeyInTheFuture(new BlogId(""), new BlogId(""));
            var futureEntities = await futureEntitiesFuture.ToListAsync();
            
            session.Add(new Blog(new BlogId(""), "Title"));
            session.Delete(new Blog(new BlogId(""), "Title"));

            var queryEntitiesFuture = session.Get<Blog>().Where(string.Empty).Limit(10).InTheFuture();
            var queryEntitiesFromFuture = await queryEntitiesFuture.ToListAsync();

            var queryEntities = await session.Get<Blog>().Where(string.Empty).ToListAsync();

            entity.Title = "Foo";
            await session.SaveChangesAsync();
        }

        ISession GetSession() {
            throw new NotImplementedException();
        }
    }

    public record BlogId
    {
        public BlogId(string prefix)
    {
        this.Prefix = prefix;
        this.Id = Guid.NewGuid();
    }

        public string Prefix { get; init; }

        public Guid Id { get; init; }
    }

    public class Blog {
        public Blog(BlogId blogId, string title) {
            BlogId = blogId;
            Title = title;
        }

        public BlogId BlogId { get; }

        public string Title { get; set; }
    }

    public partial interface IBlogRepository : IAsyncEnumerable<Blog>
    {
        Task<Blog> GetAsync(BlogId blogId, CancellationToken cancellationToken = default);

        Task<IAsyncEnumerable<Blog>> GetLatestAsync(CancellationToken cancellationToken = default);

        void Add(Blog blog);

        void Remove(Blog blog);
    }

    public class BlogStorage
    {
        public Task<Blog> LoadAsync(BlogId id)
        {
            throw new NotImplementedException();
        }

        public Task<IAsyncEnumerable<Blog>> LoadManyAsync(string whereClause = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Delete(Blog blog)
        {
            throw new NotImplementedException();
        }

        public void Store(Blog blog)
        {
            throw new NotImplementedException();
        }
    }

    public partial class BlogRepository : IBlogRepository
    {
        private readonly BlogStorage blogStorage;

        public BlogRepository(BlogStorage blogStorage)
        {
            this.blogStorage = blogStorage;
        }

        public void Add(Blog blog)
        {
            throw new NotImplementedException();
        }

        public Task<Blog> GetAsync(BlogId blogId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerator<Blog> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IAsyncEnumerable<Blog>> GetLatestAsync(CancellationToken cancellationToken = default)
        {
            return blogStorage.LoadManyAsync("");
        }

        public void Remove(Blog blog)
        {
            throw new NotImplementedException();
        }
    }

    // This is what we want to generate
//    public class BlogsRepository
//    {
//        private ISession session;

//        public BlogsRepository(ISession session)
//        {
//            this.session = session;
//        }

//        public async Task<Blog> LoadAsync(BlogId blogId)
//        {
//            var connection = await this.session.GetOrOpenConnectionAsync();
//            var command = connection.CreateCommand();
//            command.CommandText = @"
//select ""BlogId"",
//    ""Document"",
//from ""Blogs""
//where ""BlogId"" = @blogid";
//            command.Parameters.Add("blogid", NpgsqlTypes.NpgsqlDbType.Integer);
//            await command.PrepareAsync();
//            command.Parameters["blogid"].Value = blogId;
//            var reader = await command.ExecuteReaderAsync();
//            if (!reader.HasRows)
//            {
//                return null;
//            }

//            await reader.ReadAsync();
//            var id = await reader.GetFieldValueAsync<BlogId>(0);
//            var document = await reader.GetFieldValueAsync<string>(1);
//            var versionId = await reader.GetFieldValueAsync<Guid>(2);
//            await reader.DisposeAsync();
//            await command.DisposeAsync();
//            var blog = System.Text.Json.JsonSerializer.Deserialize<Blog>(document);
//            session.Register(new Document<Blog, BlogId>
//            {
//                Id = id,
//                Aggregate = blog,
//                JsonDocument = document,
//                VersionId = versionId
//            });
//            return blog;
//        }
//    }

    //public static class BlogsRepositoryExtensions {
    //    public static BlogRepository Blogs(this ISession session) {
    //        return new BlogRepository(session);
    //    }
    //}
}
