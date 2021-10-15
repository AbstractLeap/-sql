namespace Leap.Data.Tests.Internal {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using Leap.Data.IdentityMap;
    using Leap.Data.Internal;
    using Leap.Data.JsonNet;
    using Leap.Data.Queries;
    using Leap.Data.Schema;
    using Leap.Data.UnitOfWork;

    using Moq;

    using Xunit;

    public class QueryEngineTests {
        [Fact]
        public async Task Works() {
            var schema = new Mock<ISchema>();
            var queryExecutor = new Mock<IQueryExecutor>();
            var serializer = new JsonNetFieldSerializer();
            var collection = new Collection("Entities", new[] { typeof(Entity).GetField("id", BindingFlags.NonPublic | BindingFlags.Instance) }, true, false);
            var entity = new Entity("Foo");
            var row = new DatabaseRowFactory(serializer).Create<Entity, EntityId>(collection, entity).Values;
            queryExecutor.Setup(e => e.ExecuteAsync(It.IsAny<IEnumerable<IQuery>>(), It.IsAny<CancellationToken>())).Returns(ValueTask.CompletedTask);
            queryExecutor.Setup(e => e.GetAsync<Entity>(It.IsAny<IQuery>())).Returns(new[] { row }.ToAsyncEnumerable());

            var queryEngine = new QueryEngine(schema.Object, new IdentityMap(), new UnitOfWork(serializer, schema.Object), queryExecutor.Object, serializer, null, null);
            var query = new KeyQuery<Entity, EntityId>(new EntityId(), collection);
            queryEngine.Add(query);
            var result = await queryEngine.GetResult<Entity>(query).ToArrayAsync();
            Assert.Equal(entity.Id, result[0].Id);
        }

        [Fact]
        public async Task MultipleQueriesWorkOutOfOrder() {
            var schema = new Mock<ISchema>();
            var queryExecutor = new Mock<IQueryExecutor>();
            var serializer = new JsonNetFieldSerializer();
            var collection = new Collection("Entities", new[] { typeof(Entity).GetField("id", BindingFlags.NonPublic | BindingFlags.Instance) }, true, false);
            var entity1 = new Entity("Foo");
            var entity2 = new Entity("Bar");
            var row1 = new DatabaseRowFactory(serializer).Create<Entity, EntityId>(collection, entity1).Values;
            var row2 = new DatabaseRowFactory(serializer).Create<Entity, EntityId>(collection, entity2).Values;
            var query1 = new KeyQuery<Entity, EntityId>(entity1.Id, collection);
            var query2 = new KeyQuery<Entity, EntityId>(entity2.Id, collection);

            queryExecutor.Setup(e => e.ExecuteAsync(It.IsAny<IEnumerable<IQuery>>(), It.IsAny<CancellationToken>())).Returns(ValueTask.CompletedTask);
            queryExecutor.Setup(e => e.GetAsync<Entity>(query1)).Returns(new[] { row1 }.ToAsyncEnumerable());
            queryExecutor.Setup(e => e.GetAsync<Entity>(query2)).Returns(new[] { row2 }.ToAsyncEnumerable());
            var queryEngine = new QueryEngine(schema.Object, new IdentityMap(), new UnitOfWork(serializer, schema.Object), queryExecutor.Object, serializer, null, null);
            queryEngine.Add(query1);
            queryEngine.Add(query2);

            var result2 = await queryEngine.GetResult<Entity>(query2).ToArrayAsync();
            Assert.Equal(entity2.Id, result2[0].Id);
            var result1 = await queryEngine.GetResult<Entity>(query1).ToArrayAsync();
            Assert.Equal(entity1.Id, result1[0].Id);
        }

        [Fact]
        public async Task FutureEntityQueryWorks() {
            var schema = new Mock<ISchema>();
            var queryExecutor = new Mock<IQueryExecutor>();
            var serializer = new JsonNetFieldSerializer();
            var collection = new Collection("Entities", new[] { typeof(Entity).GetField("id", BindingFlags.NonPublic | BindingFlags.Instance) }, true, false);
            var entity = new Entity("Foo");
            var row = new DatabaseRowFactory(serializer).Create<Entity, EntityId>(collection, entity).Values;
            queryExecutor.Setup(e => e.ExecuteAsync(It.IsAny<IEnumerable<IQuery>>(), It.IsAny<CancellationToken>())).Returns(ValueTask.CompletedTask);
            queryExecutor.Setup(e => e.GetAsync<Entity>(It.IsAny<IQuery>())).Returns(new[] { row }.ToAsyncEnumerable());
            var session = new Session(schema.Object, serializer, queryExecutor.Object, null, null, null, null);
            var queryBuilder = new EntityQueryBuilder<Entity>(session, collection);
            var future = queryBuilder.Future();
            var results = await future.ToArrayAsync();
            var resultsAgain = await future.ToArrayAsync();
            queryExecutor.Verify(e => e.ExecuteAsync(It.IsAny<IEnumerable<IQuery>>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(results, resultsAgain);
        }

        class Entity {
            protected bool Equals(Entity other) {
                return this.id.Equals(other.id);
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Entity)obj);
            }

            public override int GetHashCode() {
                return this.id.GetHashCode();
            }

            public static bool operator ==(Entity left, Entity right) {
                return Equals(left, right);
            }

            public static bool operator !=(Entity left, Entity right) {
                return !Equals(left, right);
            }

            private readonly string name;

            private readonly EntityId id;

            public Entity(string name) {
                this.name = name;
                this.id   = new EntityId();
            }

            public EntityId Id => this.id;

            public string Name => this.name;
        }

        record EntityId {
            private readonly Guid id;

            public EntityId() {
                this.id = Guid.NewGuid();
            }

            public EntityId(Guid id) {
                this.id = id;
            }

            public Guid Id => this.id;
        }
    }
}