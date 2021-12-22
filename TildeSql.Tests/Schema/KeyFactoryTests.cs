namespace TildeSql.Tests.Schema {
    using System;
    using System.Linq;

    using TildeSql.Schema;
    using TildeSql.Schema.KeyFactories;

    using Xunit;

    public class KeyFactoryTests {
        [Fact]
        public void MultipleStrongTypedResolves() {
            var keyMembers = typeof(MultipleStrongTypeEntity).GetProperties().Where(p => p.Name == nameof(MultipleStrongTypeEntity.MultipleId)).ToArray();
            var collection = new Collection("Foo", keyMembers, true, false);
            var keyColumnResolver = new KeyColumnResolver(typeof(MultipleStrongTypeEntityId), keyMembers, collection);

            var keyColumns = keyColumnResolver.ResolveKeyColumns().ToArray();

            var keyFactory = new MultipleKeyFactory(keyColumns.Select(k => k.Item1).ToArray(), typeof(MultipleStrongTypeEntityId));
            var row = new object[] { Guid.NewGuid(), Guid.NewGuid() };
            var id = (MultipleStrongTypeEntityId)keyFactory.Create(row);
            Assert.Equal(row[0], id.Id);
            Assert.Equal(row[1], id.SingleId.Id);
        }

        [Fact]
        public void TupleResolves() {
            var keyMembers = typeof(TupleEntity).GetProperties().Where(p => p.Name.EndsWith("Id")).ToArray();
            var collection = new Collection("Foo", keyMembers, true, false);
            var keyType = typeof(ValueTuple<SingleStrongTypeEntityId, SingleStrongTypeEntityId>);
            var keyColumnResolver = new KeyColumnResolver(keyType, keyMembers, collection);

            var keyColumns = keyColumnResolver.ResolveKeyColumns().ToArray();
            var keyFactory = new TupleKeyFactory(keyColumns.Select(k => k.Item1).ToArray(), keyType);
            var row = new object[] { Guid.NewGuid(), Guid.NewGuid(), "Random streing" };
            var key = (ValueTuple<SingleStrongTypeEntityId, SingleStrongTypeEntityId>)keyFactory.Create(row);
            Assert.Equal(row[0], key.Item1.Id);
            Assert.Equal(row[1], key.Item2.Id);
        }

        record SingleStrongTypeEntityId(Guid Id);

        class MultipleStrongTypeEntity : Nameable {
            public MultipleStrongTypeEntityId MultipleId { get; set; }
        }

        record MultipleStrongTypeEntityId(Guid Id, SingleStrongTypeEntityId SingleId);

        class TupleEntity : Nameable {
            public SingleStrongTypeEntityId LeftId { get; set; }

            public SingleStrongTypeEntityId RightId { get; set; }
        }

        abstract class Nameable {
            public string Name { get; set; }
        }
    }
}