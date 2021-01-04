using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leap.Data.Tests
{
    using Fasterflect;

    using Leap.Data.Internal;

    public record BlogId
    {
        public BlogId()
    {
        this.Id = Guid.NewGuid();
    }
        
        public Guid Id { get; init; }
    }

    public class Blog {
        public Blog(string title) {
            this.BlogId = new BlogId();
            this.Title  = title;
        }
        
        public BlogId BlogId { get; init; }

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
