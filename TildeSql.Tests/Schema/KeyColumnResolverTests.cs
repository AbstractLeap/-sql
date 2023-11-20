namespace TildeSql.Tests.Schema {
    using System;
    using System.Linq;
    using System.Reflection;

    using TildeSql.Schema;

    using Xunit;

    public class KeyColumnResolverTests {
        [Fact]
        public void PrimitiveKeyResolves() {
            var keyMembers = typeof(PrimitiveEntity).GetProperties().Where(p => p.Name == nameof(PrimitiveEntity.Id)).ToArray();
            var collection = new Collection("Foo", keyMembers, true, false);
            var keyColumnResolver = new KeyColumnResolver(typeof(Guid), keyMembers, collection);

            var keyColumns = keyColumnResolver.ResolveKeyColumns();

            Assert.Single(keyColumns);
            var keyColumn = keyColumns.Single();
            Assert.Same(keyMembers[0], keyColumn.Item1.KeyMemberInfo);
            Assert.Equal("Id", keyColumn.Item1.Name);
            var instance = new PrimitiveEntity { Id = Guid.NewGuid() };
            Assert.Equal(instance.Id, keyColumn.Item2.GetValue(instance.Id));
        }

        class PrimitiveEntity : Nameable {
            public Guid Id { get; set; }
        }

        [Fact]
        public void SingleStrongTypedResolves() {
            var keyMembers = typeof(SingleStrongTypeEntity).GetProperties().Where(p => p.Name == nameof(SingleStrongTypeEntity.SingleId)).ToArray();
            var collection = new Collection("Foo", keyMembers, true, false);
            var keyColumnResolver = new KeyColumnResolver(typeof(SingleStrongTypeEntityId), keyMembers, collection);

            var keyColumns = keyColumnResolver.ResolveKeyColumns();

            Assert.Single(keyColumns);
            var keyColumn = keyColumns.Single();
            Assert.Same(keyMembers[0], keyColumn.Item1.KeyMemberInfo);
            Assert.Equal("SingleId", keyColumn.Item1.Name);
            var instance = new SingleStrongTypeEntity { SingleId = new SingleStrongTypeEntityId(Guid.NewGuid()) };
            Assert.Equal(instance.SingleId.Id, keyColumn.Item2.GetValue(instance.SingleId));
        }

        class SingleStrongTypeEntity : Nameable {
            public SingleStrongTypeEntityId SingleId { get; set; }
        }

        record SingleStrongTypeEntityId(Guid Id);

        [Fact]
        public void MultipleStrongTypedResolves() {
            var keyMembers = typeof(MultipleStrongTypeEntity).GetProperties().Where(p => p.Name == nameof(MultipleStrongTypeEntity.MultipleId)).ToArray();
            var collection = new Collection("Foo", keyMembers, true, false);
            var keyColumnResolver = new KeyColumnResolver(typeof(MultipleStrongTypeEntityId), keyMembers, collection);

            var keyColumns = keyColumnResolver.ResolveKeyColumns().ToArray();

            Assert.Equal(2, keyColumns.Length);
            Assert.All(keyColumns, c => Assert.Same(c.Item1.KeyMemberInfo, keyMembers[0]));
            var primitiveKeyColumn = keyColumns[0];
            var strongKeyColumn = keyColumns[1];
            Assert.Equal("Id", primitiveKeyColumn.Item1.Name);
            Assert.Equal("SingleId", strongKeyColumn.Item1.Name);
            var instance = new MultipleStrongTypeEntity { MultipleId = new MultipleStrongTypeEntityId(Guid.NewGuid(), new SingleStrongTypeEntityId(Guid.NewGuid())) };
            Assert.Equal(instance.MultipleId.Id, primitiveKeyColumn.Item2.GetValue(instance.MultipleId));
            Assert.Equal(instance.MultipleId.SingleId.Id, strongKeyColumn.Item2.GetValue(instance.MultipleId));
        }

        class MultipleStrongTypeEntity : Nameable {
            public MultipleStrongTypeEntityId MultipleId { get; set; }
        }

        record MultipleStrongTypeEntityId(Guid Id, SingleStrongTypeEntityId SingleId);

        [Fact]
        public void TupleResolves() {
            var keyMembers = typeof(TupleEntity).GetProperties().Where(p => p.Name.EndsWith("Id")).ToArray();
            var collection = new Collection("Foo", keyMembers, true, false);
            var keyColumnResolver = new KeyColumnResolver(typeof(ValueTuple<SingleStrongTypeEntityId, SingleStrongTypeEntityId>), keyMembers, collection);

            var keyColumns = keyColumnResolver.ResolveKeyColumns().ToArray();

            Assert.Equal(2, keyColumns.Length);
            var leftKeyColumn = keyColumns[0];
            var rightKeyColumn = keyColumns[1];
            Assert.Same(keyMembers[0], leftKeyColumn.Item1.KeyMemberInfo);
            Assert.Same(keyMembers[1], rightKeyColumn.Item1.KeyMemberInfo);
            Assert.Equal("LeftId", leftKeyColumn.Item1.Name);
            Assert.Equal("RightId", rightKeyColumn.Item1.Name);
            var instance = new TupleEntity { LeftId = new SingleStrongTypeEntityId(Guid.NewGuid()), RightId = new SingleStrongTypeEntityId(Guid.NewGuid()) };
            Assert.Equal(instance.LeftId.Id, leftKeyColumn.Item2.GetValue((instance.LeftId, instance.RightId)));
            Assert.Equal(instance.RightId.Id, rightKeyColumn.Item2.GetValue((instance.LeftId, instance.RightId)));
        }

        class TupleEntity : Nameable {
            public SingleStrongTypeEntityId LeftId { get; set; }

            public SingleStrongTypeEntityId RightId { get; set; }
        }

        abstract class Nameable {
            public string Name { get; set; }
        }

        [Fact]
        public void ConflictingSubKeysWorks() {
            var keyMembers = typeof(ConflictingSubKeys).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(p => p.Name.EndsWith("Id")).ToArray();
            var collection = new Collection("Foo", keyMembers, true, false);
            var keyColumnResolver = new KeyColumnResolver(typeof(ValueTuple<FooId, BarId>), keyMembers, collection);

            var keyColumns = keyColumnResolver.ResolveKeyColumns().ToArray();

            Assert.Equal(2, keyColumns.Length);
            var leftKeyColumn = keyColumns[0];
            var rightKeyColumn = keyColumns[1];
            Assert.Same(keyMembers[0], leftKeyColumn.Item1.KeyMemberInfo);
            Assert.Same(keyMembers[1], rightKeyColumn.Item1.KeyMemberInfo);
            Assert.Equal("LeftId", leftKeyColumn.Item1.Name);
            Assert.Equal("RightId", rightKeyColumn.Item1.Name);
            var instance = new TupleEntity { LeftId = new SingleStrongTypeEntityId(Guid.NewGuid()), RightId = new SingleStrongTypeEntityId(Guid.NewGuid()) };
            Assert.Equal(instance.LeftId.Id, leftKeyColumn.Item2.GetValue((instance.LeftId, instance.RightId)));
            Assert.Equal(instance.RightId.Id, rightKeyColumn.Item2.GetValue((instance.LeftId, instance.RightId)));
        }

        class ConflictingSubKeys {
            private readonly FooId fooId;

            private readonly BarId barId;

            public ConflictingSubKeys(FooId fooId, BarId barId) {
                this.fooId = fooId;
                this.barId = barId;
            }

            public FooId FooId => this.fooId;

            public BarId BarId => this.barId;
        }

        record BarId {
            private readonly Guid id;

            private readonly TenantId tenantId;

            public BarId(TenantId tenantId) {
                this.id       = Guid.NewGuid();
                this.tenantId = tenantId;
            }

            public Guid Id => this.id;

            public TenantId TenantId => this.tenantId;
        }

        record FooId {
            private readonly Guid id;

            private readonly TenantId tenantId;

            public FooId(TenantId tenantId) {
                this.id       = Guid.NewGuid();
                this.tenantId = tenantId;
            }

            public Guid Id => this.id;

            public TenantId TenantId => this.tenantId;
        }

        record TenantId {
            private readonly Guid id;

            public TenantId() {
                this.id = Guid.NewGuid();
            }

            public Guid Id => this.id;
        }
    }
}