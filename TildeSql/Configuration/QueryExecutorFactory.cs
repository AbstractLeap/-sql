namespace TildeSql.Configuration {
    using System;
    using System.Threading.Tasks;

    using TildeSql.Internal;

    public sealed class QueryExecutorFactory : IAsyncDisposable, IDisposable {
        /// <summary>
        ///     Creates a new executor for each query batch in a session.
        /// </summary>
        public Func<IPersistenceQueryExecutor> CreateExecutor { get; init; }

        /// <summary>
        ///     Resource shared by all query executors in a session, such as a per-session connection factory.
        /// </summary>
        public IAsyncDisposable AsyncDisposable { get; init; }

        public IDisposable Disposable { get; init; }

        public async ValueTask DisposeAsync() {
            if (this.AsyncDisposable != null) {
                await this.AsyncDisposable.DisposeAsync();
            }
            else {
                this.Disposable?.Dispose();
            }
        }

        public void Dispose() {
            this.Disposable?.Dispose();
        }
    }
}
