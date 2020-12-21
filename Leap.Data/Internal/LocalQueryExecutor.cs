namespace Leap.Data.Internal {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Fasterflect;

    using Leap.Data.IdentityMap;
    using Leap.Data.Queries;
    using Leap.Data.Utilities;

    class LocalQueryExecutor {
        private readonly IdentityMap identityMap;

        public LocalQueryExecutor(IdentityMap identityMap) {
            this.identityMap = identityMap;
        }

        public bool CanExecute(IQuery query) {
            var genericTypeDefinition = query.GetType().GetGenericTypeDefinition();
            return genericTypeDefinition == typeof(KeyQuery<,>);
        }

        public ValueTask<Maybe> ExecuteAsync(IQuery query) {
            var queryType = query.GetType();
            var genericTypeDefinition = queryType.GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(KeyQuery<,>)) {
                return (ValueTask<Maybe>)this.CallMethod(queryType.GetGenericArguments(), nameof(this.TryGetInstanceFromIdentityMap), query);
            }

            throw new NotSupportedException();
        }

        private ValueTask<Maybe> TryGetInstanceFromIdentityMap<TEntity, TKey>(KeyQuery<TEntity, TKey> keyQuery)
            where TEntity : class {
            if (this.identityMap.TryGetValue(keyQuery.Key, out TEntity entity)) {
                return new ValueTask<Maybe>(new Maybe(new List<TEntity> { entity }));
            }

            return new ValueTask<Maybe>(Maybe.NotSuccessful);
        }
    }
}