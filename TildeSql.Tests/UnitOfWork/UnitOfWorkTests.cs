namespace TildeSql.UnitOfWork {
    using System;
    using System.Linq;

    using TildeSql.IdentityMap;
    using TildeSql.Internal;
    using TildeSql.Operations;
    using TildeSql.Schema;
    using TildeSql.Serialization;
    using TildeSql.UnitOfWork;

    using Xunit;

    public class UnitOfWorkTests {
        [Fact]
        public void AddedEntityIsRetrievable() {
            var collection = new Collection("Things", new[] { typeof(Thing).GetProperty(nameof(Thing.Id)) }, true, false);
            collection.AddClassType(typeof(Thing));
            var unitOfWork = new UnitOfWork(new SystemJsonSerializer(), new Schema(), null);
            var thing = new Thing { Name = "Foo" };
            var databaseRowFactory = new DatabaseRowFactory(new SystemJsonSerializer());
            var row = databaseRowFactory.Create<Thing, Guid>(collection, thing);
            unitOfWork.AddOrUpdate(collection, thing, row, DocumentState.New);
            Assert.Same(row, unitOfWork.GetRow(collection, thing));
            Assert.True(unitOfWork.IsAttached(collection, thing));
            Assert.Equal(DocumentState.New, unitOfWork.GetState(collection, thing));
        }

        [Fact]
        public void UpdateStateDoes() {
            var collection = new Collection("Things", new[] { typeof(Thing).GetProperty(nameof(Thing.Id)) }, true, false);
            collection.AddClassType(typeof(Thing));
            var unitOfWork = new UnitOfWork(new SystemJsonSerializer(), new Schema(),null);
            var thing = new Thing { Name = "Foo" };
            var databaseRowFactory = new DatabaseRowFactory(new SystemJsonSerializer());
            var row = databaseRowFactory.Create<Thing, Guid>(collection, thing);
            unitOfWork.AddOrUpdate(collection, thing, row, DocumentState.New);
            unitOfWork.UpdateState(collection, thing, DocumentState.Persisted);
            Assert.Same(row, unitOfWork.GetRow(collection, thing));
            Assert.True(unitOfWork.IsAttached(collection, thing));
            Assert.Equal(DocumentState.Persisted, unitOfWork.GetState(collection, thing));
        }

        [Fact]
        public void UpdateRowDoes() {
            var collection = new Collection("Things", new[] { typeof(Thing).GetProperty(nameof(Thing.Id)) }, true, false);
            collection.AddClassType(typeof(Thing));
            var unitOfWork = new UnitOfWork(new SystemJsonSerializer(), new Schema(), null);
            var thing = new Thing { Name = "Foo" };
            var databaseRowFactory = new DatabaseRowFactory(new SystemJsonSerializer());
            var row = databaseRowFactory.Create<Thing, Guid>(collection, thing);
            unitOfWork.AddOrUpdate(collection, thing, row, DocumentState.New);
            thing.Name = "Bar";
            var updatedRow = databaseRowFactory.Create<Thing, Guid>(collection, thing);
            unitOfWork.UpdateRow(collection, thing, updatedRow);
            Assert.Same(updatedRow, unitOfWork.GetRow(collection, thing));
            Assert.True(unitOfWork.IsAttached(collection, thing));
            Assert.Equal(DocumentState.New, unitOfWork.GetState(collection, thing));
        }

        [Fact]
        public void SetPersistedDoes() {
            var collection = new Collection("Things", new[] { typeof(Thing).GetProperty(nameof(Thing.Id)) }, true, false);
            collection.AddClassType(typeof(Thing));
            var unitOfWork = new UnitOfWork(new SystemJsonSerializer(), new Schema(), null);
            var thing = new Thing { Name = "Foo" };
            var databaseRowFactory = new DatabaseRowFactory(new SystemJsonSerializer());
            var row = databaseRowFactory.Create<Thing, Guid>(collection, thing);
            unitOfWork.AddOrUpdate(collection, thing, row, DocumentState.New);
            Assert.Equal(DocumentState.New, unitOfWork.GetState(collection, thing));
            unitOfWork.SetPersisted();
            Assert.Same(row, unitOfWork.GetRow(collection, thing));
            Assert.True(unitOfWork.IsAttached(collection, thing));
            Assert.Equal(DocumentState.Persisted, unitOfWork.GetState(collection, thing));
        }

        [Fact]
        public void AddedEntityIsAddOperation() {
            var collection = new Collection("Things", new[] { typeof(Thing).GetProperty(nameof(Thing.Id)) }, true, false);
            collection.AddClassType(typeof(Thing));
            var unitOfWork = new UnitOfWork(new SystemJsonSerializer(), new Schema(), null);
            var thing = new Thing { Name = "Foo" };
            var databaseRowFactory = new DatabaseRowFactory(new SystemJsonSerializer());
            var row = databaseRowFactory.Create<Thing, Guid>(collection, thing);
            unitOfWork.AddOrUpdate(collection, thing, row, DocumentState.New);
            var ops = unitOfWork.Operations.ToArray();
            Assert.Single(ops);
            Assert.IsType<AddOperation<Thing>>(ops[0]);
        }

        [Fact]
        public void DeletedEntityIsDeleteOperation() {
            var collection = new Collection("Things", new[] { typeof(Thing).GetProperty(nameof(Thing.Id)) }, true, false);
            collection.AddClassType(typeof(Thing));
            var unitOfWork = new UnitOfWork(new SystemJsonSerializer(), new Schema(), null);
            var thing = new Thing { Name = "Foo" };
            var databaseRowFactory = new DatabaseRowFactory(new SystemJsonSerializer());
            var row = databaseRowFactory.Create<Thing, Guid>(collection, thing);
            unitOfWork.AddOrUpdate(collection, thing, row, DocumentState.New);
            unitOfWork.UpdateState(collection, thing, DocumentState.Deleted);
            var ops = unitOfWork.Operations.ToArray();
            Assert.Single(ops);
            Assert.IsType<DeleteOperation<Thing>>(ops[0]);
        }

        [Fact]
        public void PersistedEntityIsNoOperation() {
            var collection = new Collection("Things", new[] { typeof(Thing).GetProperty(nameof(Thing.Id)) }, true, false);
            collection.AddClassType(typeof(Thing));
            var unitOfWork = new UnitOfWork(new SystemJsonSerializer(), new Schema(), null);
            var thing = new Thing { Name = "Foo" };
            var databaseRowFactory = new DatabaseRowFactory(new SystemJsonSerializer());
            var row = databaseRowFactory.Create<Thing, Guid>(collection, thing);
            unitOfWork.AddOrUpdate(collection, thing, row, DocumentState.Persisted);
            var ops = unitOfWork.Operations.ToArray();
            Assert.Empty(ops);
        }

        [Fact]
        public void ChangedPersistedEntityIsUpdateOperation() {
            var collection = new Collection("Things", new[] { typeof(Thing).GetProperty(nameof(Thing.Id)) }, true, false);
            collection.AddClassType(typeof(Thing));
            var unitOfWork = new UnitOfWork(new SystemJsonSerializer(), new Schema(), null);
            var thing = new Thing { Name = "Foo" };
            var databaseRowFactory = new DatabaseRowFactory(new SystemJsonSerializer());
            var row = databaseRowFactory.Create<Thing, Guid>(collection, thing);
            unitOfWork.AddOrUpdate(collection, thing, row, DocumentState.Persisted);
            thing.Name = "Bar";
            var ops = unitOfWork.Operations.ToArray();
            Assert.Single(ops);
            Assert.IsType<UpdateOperation<Thing>>(ops[0]);
        }

        [Fact]
        public void Inheritance() {
            var collection = new Collection("Things", new[] { typeof(Thing).GetProperty(nameof(Thing.Id)) }, true, false);
            collection.AddClassType(typeof(Thing));
            collection.AddClassType(typeof(ChildThing));
            var unitOfWork = new UnitOfWork(new SystemJsonSerializer(), new Schema(), null);
            var thing = new ChildThing { Name = "Foo" };
            var databaseRowFactory = new DatabaseRowFactory(new SystemJsonSerializer());
            var row = databaseRowFactory.Create<ChildThing, Guid>(collection, thing);
            unitOfWork.AddOrUpdate(collection, thing, row, DocumentState.New);
            Assert.Same(row, unitOfWork.GetRow<Thing>(collection, thing)); // note the change in T here to the base type
            Assert.True(unitOfWork.IsAttached(collection, thing));
            Assert.Equal(DocumentState.New, unitOfWork.GetState(collection, thing));
        }

        class Thing : IEquatable<Thing> {
            public bool Equals(Thing other) {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return this.Id.Equals(other.Id);
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Thing)obj);
            }

            public override int GetHashCode() {
                return this.Id.GetHashCode();
            }

            public static bool operator ==(Thing left, Thing right) {
                return Equals(left, right);
            }

            public static bool operator !=(Thing left, Thing right) {
                return !Equals(left, right);
            }

            public Thing() {
                this.Id = Guid.NewGuid();
            }

            public Guid Id { get; set; }

            public string Name { get; set; }
        }

        class ChildThing : Thing {
            public bool Truth { get; set; }
        }
    }
}