namespace TildeSql.UnitOfWork {
    using System;
    using System.Collections.Generic;

    using Fasterflect;

    using TildeSql.IdentityMap;
    using TildeSql.Internal;
    using TildeSql.Operations;
    using TildeSql.Schema;
    using TildeSql.Serialization;

    internal class UnitOfWork {
        private readonly Dictionary<Collection, HashSet<IDocument>> attachedEntities = new();

        private readonly Dictionary<object, List<(Collection collection, IDocument document)>> documentLookup = new();

        private readonly ChangeTracker changeTracker;

        public UnitOfWork(ISerializer serializer, ISchema schema) {
            this.changeTracker = new ChangeTracker(serializer, schema);
        }

        public void AddOrUpdate<TEntity>(Collection collection, TEntity entity, DatabaseRow row, DocumentState state) {
            IDocument document = FindDocument(collection, entity);
            if (document != null) {
                document.Row   = row;
                document.State = state;
                return;
            }

            document       = DocumentExtensions.Create(entity, collection); // we want to create a document of the actual type, not the base type in inheritance hierarchies
            document.Row   = row;
            document.State = state;
            if (!this.attachedEntities.TryGetValue(collection, out var list)) {
                this.attachedEntities.Add(collection, new HashSet<IDocument> { document });
            }
            else {
                list.Add(document);
            }

            if (!this.documentLookup.TryGetValue(entity, out var documents)) {
                this.documentLookup.Add(entity, new List<(Collection collection, IDocument document)> { (collection, document) });
            }
            else {
                documents.Add((collection, document));
            }
        }

        public void UpdateState<TEntity>(Collection collection, TEntity entity, DocumentState state) {
            var document = FindDocument(collection, entity);
            if (document == null) {
                throw new Exception("The entity is not attached to the collection");
            }

            document.State = state;
        }

        public void UpdateRow<TEntity>(Collection collection, TEntity entity, DatabaseRow row) {
            var document = FindDocument(collection, entity);
            if (document == null) {
                throw new Exception("The entity is not attached to the collection");
            }

            document.Row = row;
        }

        public DocumentState GetState<TEntity>(Collection collection, TEntity entity)
            where TEntity : class {
            var document = FindDocument(collection, entity);
            return document?.State ?? DocumentState.NotAttached;
        }

        public void SetPersisted() {
            foreach (var attachedEntity in this.attachedEntities) {
                foreach (IDocument document in attachedEntity.Value) {
                    if (document.State == DocumentState.New) {
                        document.State = DocumentState.Persisted;
                    }

                    // TODO should we remove the deleted ones?
                }
            }
        }

        public bool IsAttached<TEntity>(Collection collection, TEntity entity) {
            return FindDocument(collection, entity) != null;
        }

        public DatabaseRow GetRow<TEntity>(Collection collection, TEntity entity) {
            var document = FindDocument(collection, entity);
            if (document == null) {
                throw new Exception("The entity is not attached to the collection");
            }

            return document.Row;
        }

        private IDocument<TEntity> FindDocument<TEntity>(Collection collection, TEntity entity) {
            if (this.documentLookup.TryGetValue(entity, out var documents)) {
                foreach (var (documentCollection, document) in documents) {
                    if (documentCollection.Equals(collection)) {
                        return document as IDocument<TEntity>;
                    }
                }
            }

            return null;
        }

        public IEnumerable<IOperation> Operations {
            get {
                foreach (var attachedEntity in this.attachedEntities) {
                    foreach (var document in attachedEntity.Value) {
                        if (document.State == DocumentState.New) {
                            yield return (IOperation)typeof(AddOperation<>).MakeGenericType(document.EntityType()).CreateInstance(document.GetEntity(), document.Collection);
                        }
                        else if (document.State == DocumentState.Deleted) {
                            yield return (IOperation)typeof(DeleteOperation<>).MakeGenericType(document.EntityType()).CreateInstance(document.GetEntity(), document.Collection);
                        }
                        else if (document.State == DocumentState.Persisted) {
                            if (this.changeTracker.HasEntityChanged(document)) {
                                yield return (IOperation)typeof(UpdateOperation<>).MakeGenericType(document.EntityType()).CreateInstance(document.GetEntity(), document.Collection);
                            }
                        }
                    }
                }
            }
        }
    }

    static class UnitOfWorkExtensions {
        public static void UpdateRow(this UnitOfWork unitOfWork, Type entityType, Collection collection, object entity, DatabaseRow row) {
            unitOfWork.CallMethod(new[] { entityType }, nameof(UnitOfWork.UpdateRow), collection, entity, row);
        }

        public static DatabaseRow GetRow(this UnitOfWork unitOfWork, Type entityType, Collection collection, object entity) {
            return (DatabaseRow)unitOfWork.CallMethod(new[] { entityType }, nameof(UnitOfWork.GetRow), collection, entity);
        }
    }
}