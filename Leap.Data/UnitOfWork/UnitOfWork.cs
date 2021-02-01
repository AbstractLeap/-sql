namespace Leap.Data.UnitOfWork {
    using System;
    using System.Collections.Generic;

    using Fasterflect;

    using Leap.Data.IdentityMap;
    using Leap.Data.Internal;
    using Leap.Data.Operations;
    using Leap.Data.Schema;
    using Leap.Data.Serialization;

    internal class UnitOfWork {
        private readonly Dictionary<Table, HashSet<IDocument>> attachedEntities = new();

        private readonly Dictionary<object, List<(Table table, IDocument document)>> documentLookup = new();

        private readonly ChangeTracker changeTracker;

        public UnitOfWork(ISerializer serializer, ISchema schema) {
            this.changeTracker = new ChangeTracker(serializer, schema);
        }

        public void AddOrUpdate<TEntity>(Table table, TEntity entity, DatabaseRow row, DocumentState state) {
            var document = new Document<TEntity>(entity, table) { Row = row, State = state };
            if (!this.attachedEntities.TryGetValue(table, out var list)) {
                this.attachedEntities.Add(table, new HashSet<IDocument> { document });
            }
            else {
                list.Add(document);
            }

            if (!this.documentLookup.TryGetValue(entity, out var documents)) {
                this.documentLookup.Add(entity, new List<(Table table, IDocument document)> { (table, document) });
            }
            else {
                documents.Add((table, document));
            }
        }

        public void UpdateState<TEntity>(Table table, TEntity entity, DocumentState state) {
            var document = FindDocument(table, entity);
            if (document == null) {
                throw new Exception("The entity is not attached to the table");
            }

            document.State = state;
        }

        public void UpdateRow<TEntity>(Table table, TEntity entity, DatabaseRow row) {
            var document = FindDocument(table, entity);
            if (document == null) {
                throw new Exception("The entity is not attached to the table");
            }

            document.Row = row;
        }

        public DocumentState GetState<TEntity>(Table table, TEntity entity)
            where TEntity : class {
            var document = FindDocument(table, entity);
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

        public bool IsAttached<TEntity>(Table table, TEntity entity) {
            return FindDocument(table, entity) != null;
        }

        public DatabaseRow GetRow<TEntity>(Table table, TEntity entity) {
            var document = FindDocument(table, entity);
            if (document == null) {
                throw new Exception("The entity is not attached to the table");
            }

            return document.Row;
        }

        private IDocument<TEntity> FindDocument<TEntity>(Table table, TEntity entity) {
            if (this.documentLookup.TryGetValue(entity, out var documents)) {
                foreach (var (documentTable, document) in documents) {
                    if (documentTable.Equals(table)) {
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
                            yield return (IOperation)typeof(AddOperation<>).MakeGenericType(document.EntityType()).CreateInstance(document.GetEntity(), document.Table);
                        }
                        else if (document.State == DocumentState.Deleted) {
                            yield return (IOperation)typeof(DeleteOperation<>).MakeGenericType(document.EntityType()).CreateInstance(document.GetEntity(), document.Table);
                        }
                        else if (document.State == DocumentState.Persisted) {
                            if (this.changeTracker.HasEntityChanged(document)) {
                                yield return (IOperation)typeof(UpdateOperation<>).MakeGenericType(document.EntityType()).CreateInstance(document.GetEntity(), document.Table);
                            }
                        }
                    }
                }
            }
        }
    }

    static class UnitOfWorkExtensions {
        public static void UpdateRow(this UnitOfWork unitOfWork, Type entityType, Table table, object entity, DatabaseRow row) {
            unitOfWork.CallMethod(new[] { entityType }, nameof(UnitOfWork.UpdateRow), table, entity, row);
        }

        public static DatabaseRow GetRow(this UnitOfWork unitOfWork, Type entityType, Table table, object entity) {
            return (DatabaseRow)unitOfWork.CallMethod(new[] { entityType }, nameof(UnitOfWork.GetRow), table, entity);
        }
    }
}