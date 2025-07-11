namespace TildeSql.Internal {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Memory;

    using TildeSql.IdentityMap;
    using TildeSql.Internal.Caching;
    using TildeSql.Internal.Common;
    using TildeSql.Queries;
    using TildeSql.Schema;
    using TildeSql.Serialization;
    using TildeSql.UnitOfWork;

    class QueryEngine : IAsyncDisposable, IDisposable {
        private readonly AsyncLock mutex = new();

        private readonly ISchema schema;

        private readonly IdentityMap identityMap;

        private readonly UnitOfWork unitOfWork;

        private readonly IPersistenceQueryExecutor persistenceQueryExecutor;
        
        private readonly CacheExecutor cacheExecutor;

        private readonly CacheSetter cacheSetter;

        private readonly IdentityMapExecutor identityMapExecutor;

        private readonly ISerializer serializer;

        /// <summary>
        ///     queries to be executed
        /// </summary>
        private readonly List<IQuery> queriesToExecute = new();

        private readonly HashSet<IQuery> persistenceQueryExecutorQueries = new();

        private readonly HashSet<IQuery> cacheExecutorQueries;

        private readonly HashSet<IQuery> identityMapQueries = new();

        private readonly Dictionary<IQuery, IQuery[]> queryForwardMap = new();

        public QueryEngine(
            ISchema schema,
            IdentityMap identityMap,
            UnitOfWork unitOfWork,
            IPersistenceQueryExecutor persistenceQueryExecutor,
            ISerializer serializer,
            IMemoryCache memoryCache,
            IDistributedCache distributedCache,
            ICacheSerializer cacheSerializer,
            CacheOptions cacheOptions) {
            this.schema                   = schema;
            this.identityMap              = identityMap;
            this.unitOfWork               = unitOfWork;
            this.persistenceQueryExecutor = persistenceQueryExecutor;
            this.serializer               = serializer;
            this.identityMapExecutor      = new IdentityMapExecutor(this.identityMap, unitOfWork);
            if (memoryCache != null || distributedCache != null) {
                this.cacheExecutor = new CacheExecutor(memoryCache, distributedCache, cacheSerializer, cacheOptions);
                this.cacheSetter = new CacheSetter(memoryCache, distributedCache, cacheSerializer, cacheOptions);
                this.cacheExecutorQueries = new();
            }
        }

        public void Add(IQuery query) {
            this.queriesToExecute.Add(query);
        }

        public async IAsyncEnumerable<T> GetResult<T>(IQuery query)
            where T : class {
            if (!this.queryForwardMap.ContainsKey(query)
                && !this.identityMapQueries.Contains(query)
                && !(this.cacheExecutorQueries?.Contains(query) ?? false)
                && !this.persistenceQueryExecutorQueries.Contains(query)) {
                var @lock = await this.mutex.LockAsync();

                try {
                    // query has not been executed, so let's flush existing queries and then add
                    await this.FlushPersistenceAsync();
                    if (!this.queriesToExecute.Contains(query)) {
                        this.Add(query);
                    }

                    await this.ExecuteAsync();
                }
                finally {
                    @lock?.Dispose();
                }
            }

            if (this.queryForwardMap.TryGetValue(query, out var queries)) {
                foreach (var forwardedQuery in queries) {
                    await foreach (var result in this.GetResult<T>(forwardedQuery)) {
                        yield return result;
                    }
                }

                yield break;
            }

            if (this.identityMapQueries.Contains(query)) {
                await foreach (var entity in this.identityMapExecutor.GetAsync<T>(query)) {
                    yield return entity;
                }

                yield break;
            }

            var extractTotal = query is ICountQuery { CountAccessor: not null };
            var rowsRead = 0;
            if (this.cacheExecutorQueries?.Contains(query) ?? false) {
                await foreach (var row in cacheExecutor.GetAsync<T>(query)) {
                    rowsRead++;
                    var entity = HydrateDocument(row);
                    if (entity != null) {
                        yield return entity;
                    }
                }

                if (extractTotal && rowsRead == 0) {
                    ((ICountSetter)(((ICountQuery)query).CountAccessor)).SetTotal(0);
                }

                yield break;
            }

            await foreach (var row in this.persistenceQueryExecutor.GetAsync(query)) {
                rowsRead++;
                var entity = HydrateDocument(row);
                if (entity != null) {
                    yield return entity;
                }
            }

            if (extractTotal && rowsRead == 0) {
                ((ICountSetter)(((ICountQuery)query).CountAccessor)).SetTotal(0);
            }

            yield break;

            T HydrateDocument(object[] row) {
                // need to hydrate the entity from the database row and add to the document
                var collection = query.Collection;
                var id = collection.KeyFactory.Create(row);

                if (extractTotal) {
                    var total = (long)row[^1];
                    ((ICountSetter)(((ICountQuery)query).CountAccessor)).SetTotal(total);
                    extractTotal = false; // same for every row but we only need it once
                }

                // TODO invalidate old versions
                // check ID map for instance
                if (this.identityMap.TryGetValue(collection.KeyType, id, out T entityInstance)) {
                    if (this.unitOfWork.GetState(collection, entityInstance) == DocumentState.Deleted) {
                        return null;
                    }

                    this.unitOfWork.AddOrUpdate(collection, entityInstance, new DatabaseRow(collection, row), DocumentState.Persisted);
                    return entityInstance;
                }

                var json = RowValueHelper.GetValue<string>(collection, row, SpecialColumns.Document);
                var typeName = RowValueHelper.GetValue<string>(collection, row, SpecialColumns.DocumentType);
                var documentType = collection.TypeSerializer.Deserialize(typeName);
                if (this.serializer.Deserialize(documentType, json) is not T entity) {
                    throw new Exception($"Unable to cast object of type {typeName} to {typeof(T)}");
                }

                this.identityMap.Add(collection.KeyType, id, entity);
                this.unitOfWork.AddOrUpdate(collection, entity, new DatabaseRow(collection, row), DocumentState.Persisted);
                return entity;
            }
        }

        public async ValueTask EnsureCleanAsync(CancellationToken cancellationToken = default) {
            using var @lock = await this.mutex.LockAsync(cancellationToken);
            await this.FlushPersistenceAsync(); // clear out any non-read queries
            await this.ExecuteAsync(cancellationToken); // execute any non-executed queries
            await this.FlushPersistenceAsync(); // ensure that they're also read
        }

        /// <summary>
        ///     Ensures that no further processing needs to be done by the persistence
        ///     (makes the persistence ready to accept new requests)
        /// </summary>
        /// <returns></returns>
        private async ValueTask FlushPersistenceAsync() {
            if (this.persistenceQueryExecutor != null) {
                await this.persistenceQueryExecutor.FlushAsync();
            }
        }

        /// <summary>
        ///     Executes the queries against the various caches and the persistent layer
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ExecuteAsync(CancellationToken cancellationToken = default) {
            if (!this.queriesToExecute.Any()) {
                return;
            }

            var queriesStillToExecute = this.queriesToExecute;
            var identityMapExecutionResult = this.identityMapExecutor.Execute(queriesStillToExecute, cancellationToken);
            foreach (var executedQuery in identityMapExecutionResult.ExecutedQueries) {
                this.identityMapQueries.Add(executedQuery);
            }

            foreach (var (original, executed, remaining) in identityMapExecutionResult.PartiallyExecutedQueries) {
                this.queryForwardMap[original] = [executed, remaining];
            }

            queriesStillToExecute = identityMapExecutionResult.NonExecutedQueries.Union(identityMapExecutionResult.PartiallyExecutedQueries.Select(pq => pq.Remaining)).ToList();

            if (queriesStillToExecute.Any() && this.cacheExecutor != null) {
                await ExecuteAgainstCacheAsync().ToArrayAsync(cancellationToken);
            }

            async IAsyncEnumerable<IQuery> ExecuteAgainstCacheAsync() {
                var cacheExecutionResult = await this.cacheExecutor.ExecuteAsync(queriesStillToExecute, cancellationToken);
                foreach (var executedQuery in cacheExecutionResult.ExecutedQueries) {
                    this.cacheExecutorQueries.Add(executedQuery);
                    yield return executedQuery;
                }

                foreach (var (original, executed, remaining) in cacheExecutionResult.PartiallyExecutedQueries) {
                    this.queryForwardMap[original] = [executed, remaining];
                }

                queriesStillToExecute = cacheExecutionResult.NonExecutedQueries.Union(cacheExecutionResult.PartiallyExecutedQueries.Select(pq => pq.Remaining)).ToList();
            }

            if (queriesStillToExecute.Any()) {
                if (this.persistenceQueryExecutor == null) {
                    throw new Exception("No persistence query mechanism has been configured");
                }

                if (this.cacheExecutor != null) {
                    // stampede protection, we take a lock on all cacheable queries
                    var cacheableQueries = queriesStillToExecute.Where(q => q.CacheKey != null).OrderBy(q => q.CacheKey).ToArray();
                    var locks = new Dictionary<string, IDisposable>();
                    var locker = new AsyncDuplicateLock();
                    try {
                        foreach (var cacheableQuery in cacheableQueries) {
                            if (!locks.ContainsKey(cacheableQuery.CacheKey)) {
                                locks.Add(cacheableQuery.CacheKey, await locker.LockAsync(cacheableQuery.CacheKey));
                            }
                        }

                        // now that we have locks, another session may have populated the cache for these queries, so we check again
                        // and if we find the query in the cache we release the lock
                        await foreach (var query in ExecuteAgainstCacheAsync()) {
                            locks[query.CacheKey].Dispose();
                            locks.Remove(query.CacheKey);
                        }

                        if (queriesStillToExecute.Any()) {
                            await ExecuteAgainstPersistenceAsync();
                            this.queriesToExecute.Clear();
                            return;
                        }
                    }
                    finally {
                        foreach (var keyLock in locks) {
                            keyLock.Value.Dispose();
                        }
                    }
                }

                await ExecuteAgainstPersistenceAsync();
            }

            this.queriesToExecute.Clear();

            async Task ExecuteAgainstPersistenceAsync() {
                await this.persistenceQueryExecutor.ExecuteAsync(queriesStillToExecute, cancellationToken);
                foreach (var executedQuery in queriesStillToExecute) {
                    this.persistenceQueryExecutorQueries.Add(executedQuery);
                    if (this.cacheSetter != null && executedQuery.CacheKey != null) {
                        // we flush the results in to the result cache for cache queries so that we can pop in the cache and release the stampede locks
                        var results = await this.persistenceQueryExecutor.GetAsync(executedQuery).ToArrayAsync(cancellationToken);
                        await cacheSetter.SetAsync(executedQuery, results);
                    }
                }
            }
        }

        public async ValueTask DisposeAsync() {
            if (this.persistenceQueryExecutor is IAsyncDisposable disposable) {
                await disposable.DisposeAsync();
            }
        }

        public void Dispose() {
            if (this.persistenceQueryExecutor is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}