namespace TildeSql.Configuration {
    using System;
    using System.Threading.Tasks;

    using TildeSql.Internal;

    /// <summary>
    ///     Creates query executors for individual batches and owns any resource shared by those executors
    ///     for the lifetime of a session.
    /// </summary>
    public interface IQueryExecutorFactory : IAsyncDisposable, IDisposable {
        IPersistenceQueryExecutor CreateExecutor();
    }

    public sealed class QueryExecutorFactory : IQueryExecutorFactory {
        private readonly Func<IPersistenceQueryExecutor> createExecutor;

        private readonly object ownedResource;

        public QueryExecutorFactory(Func<IPersistenceQueryExecutor> createExecutor, object ownedResource = null) {
            this.createExecutor = createExecutor ?? throw new ArgumentNullException(nameof(createExecutor));
            this.ownedResource  = ownedResource;
        }

        public IPersistenceQueryExecutor CreateExecutor() {
            return this.createExecutor();
        }

        public async ValueTask DisposeAsync() {
            if (this.ownedResource is IAsyncDisposable asyncDisposable) {
                await asyncDisposable.DisposeAsync();
                return;
            }

            if (this.ownedResource is IDisposable disposable) {
                disposable.Dispose();
            }
        }

        public void Dispose() {
            if (this.ownedResource is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}
