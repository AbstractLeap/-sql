namespace Leap.Data {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Fasterflect;

    class QueryEngine : IAsyncDisposable {
        private readonly IConnectionFactory connectionFactory;

        private readonly ISchema schema;

        private readonly IdentityMap identityMap;

        private readonly ISerializer serializer;

        private readonly IList<IQuery> queries = new List<IQuery>();

        private readonly IList<IQuery> nonCompletedQueries = new List<IQuery>();

        private readonly LocalQueryExecutor localQueryExecutor = new LocalQueryExecutor();

        private readonly IDictionary<Guid, object> resultBag = new Dictionary<Guid, object>();

        private DbCommand command;

        private DbDataReader dataReader;

        private int nonCompleteQueryReaderIndex = 0;
        
        public bool IsComplete { get; private set; }

        public QueryEngine(IConnectionFactory connectionFactory, ISchema schema, IdentityMap identityMap, ISerializer serializer) {
            this.connectionFactory = connectionFactory;
            this.schema            = schema;
            this.identityMap       = identityMap;
            this.serializer        = serializer;
        }

        public void Add(IQuery query) {
            this.queries.Add(query);
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default) {
            foreach (var query in this.queries) {
                if (this.localQueryExecutor.CanExecute(query)) {
                    var attemptedResult = await this.localQueryExecutor.ExecuteAsync(query);
                    if (attemptedResult.WasSuccessful) {
                        this.resultBag.Add(query.Identifier, attemptedResult.Result);
                    }
                    else {
                        this.nonCompletedQueries.Add(query);
                    }
                }
                else {
                    this.nonCompletedQueries.Add(query);
                }
            }
            
            if (!this.nonCompletedQueries.Any()) {
                this.IsComplete = true;
                return;
            }
            
            var connection = this.connectionFactory.Get();
            if (connection.State != ConnectionState.Open) {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }
            
            // TODO figure out connection/transaction lifecycle
            this.command = connection.CreateCommand();
            var sqlQueryWriter = new SqlQueryWriter();
            foreach (var nonCompletedQuery in this.nonCompletedQueries) {
                sqlQueryWriter.Write(nonCompletedQuery, this.command);
            }

            this.dataReader = await this.command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        }
        
        public async IAsyncEnumerable<T> GetResult<T>(IQuery query)
            where T : class {
            // do we have the result already?
            if (this.resultBag.TryGetValue(query.Identifier, out var result)) {
                if (result is List<T> resultList) {
                    foreach (var entity in resultList) {
                        yield return entity;
                    }
                }

                throw new Exception($"Tried to get result of type {typeof(T)} but actual type was {result.GetType()}");
            }
            
            // we don't have the result, so we must go through the reader until we do.
            var nonCompleteQuery = this.nonCompletedQueries[this.nonCompleteQueryReaderIndex];
            while (nonCompleteQuery.Identifier != query.Identifier) {
                while (await this.dataReader.ReadAsync().ConfigureAwait(false)) {
                    // NOTE TO SELF you've done the wrong thing. this should be put in the resultBag, not yielded, that should be done later
                    // hydrate database row
                    var table = this.schema.GetTable<T>();
                    var values = new object[table.Columns.Count];
                    this.dataReader.GetValues(values);
                    var row = new DatabaseRow(table, values);
                    var id = table.KeyType.TryCreateInstance(table.Columns.Select(c => c.Name).ToArray(), values);
                    
                    // TODO invalidate old versions
                    // check ID map for instance
                    if (this.identityMap.TryGetValue(table.KeyType, id, out T mappedEntity)) {
                        yield return mappedEntity;
                    }

                    var json = row.GetValue<string>(SpecialColumns.Document);
                    var typeName = row.GetValue<string>(SpecialColumns.DocumentType);
                    var documentType = Type.GetType(typeName); // TODO better type handling across assemblies
                    if (!(this.serializer.Deserialize(documentType, json) is T entity)) {
                        throw new Exception($"Unable to cast object of type {typeName} to {typeof(T)}");
                    }
                    
                    this.identityMap.Add(id, entity);
                    yield return entity;
                    
                    // TODO second level cache
                    // TODO store database row somewhere for dirty tracking
                }
            }
        }
        
        public async ValueTask DisposeAsync() {
            if (this.dataReader != null) {
                await this.dataReader.DisposeAsync().ConfigureAwait(false);
            }
            
            if (this.command != null) {
                await this.command.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}