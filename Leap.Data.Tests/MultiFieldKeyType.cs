namespace Leap.Data.Tests {
    using System;
    using System.Threading.Tasks;

    using Leap.Data.Configuration;
    using Leap.Data.JsonNet;
    using Leap.Data.Schema;
    using Leap.Data.SqlServer;

    using Xunit;

    public class MultiFieldKeyType {
        [Fact]
        public async Task PrimitiveNestedRoundTrips() {
            var sf = MakeTarget<MultiFieldIdEntity>();
            var insertSession = sf.StartSession();
            var thing = new MultiFieldIdEntity("Foo");
            insertSession.Add(thing);

            await insertSession.SaveChangesAsync();

            var querySession1 = sf.StartSession();
            var thingAgain = await querySession1.Get<MultiFieldIdEntity>().SingleAsync(thing.Id);
            Assert.Equal("Foo", thingAgain.Name);
            Assert.Equal(thing.Id.LeftId, thingAgain.Id.LeftId);
            Assert.Equal(thing.Id.RightId, thingAgain.Id.RightId);
        }

        [Fact]
        public async Task NonPrimitiveNestedRoundTrips() {
            var sf = MakeTarget<MultiNonPrimitiveIdFieldIdEntity>();
            var insertSession = sf.StartSession();
            var thing = new MultiNonPrimitiveIdFieldIdEntity("Foo");
            insertSession.Add(thing);

            await insertSession.SaveChangesAsync();

            var querySession1 = sf.StartSession();
            var thingAgain = await querySession1.Get<MultiNonPrimitiveIdFieldIdEntity>().SingleAsync(thing.Id);
            Assert.Equal("Foo", thingAgain.Name);
            Assert.Equal(thing.Id.LeftId, thingAgain.Id.LeftId);
            Assert.Equal(thing.Id.RightId, thingAgain.Id.RightId);
        }

        private static ISessionFactory MakeTarget<TEntity>() {
            var testSchema = new SchemaBuilder().AddTypes(typeof(TEntity)).UseSqlServerConvention().Build();
            var sessionFactory = new Configuration(testSchema).UseJsonNetFieldSerialization()
                                                              .UseSqlServer("Server=.;Database=leap-data;Trusted_Connection=True;")
                                                              .BuildSessionFactory();
            return sessionFactory;
        }

        record MultiFieldId {
            private readonly Guid leftId;

            private readonly Guid rightId;

            public MultiFieldId() {
                this.leftId  = Guid.NewGuid();
                this.rightId = Guid.NewGuid();
            }

            public MultiFieldId(Guid leftId, Guid rightId) {
                this.leftId  = leftId;
                this.rightId = rightId;
            }

            public Guid LeftId => this.leftId;

            public Guid RightId => this.rightId;
        }

        class MultiFieldIdEntity {
            private readonly MultiFieldId id;

            private readonly string name;

            public MultiFieldIdEntity(string name) {
                this.id   = new MultiFieldId();
                this.name = name;
            }

            public string Name => this.name;

            public MultiFieldId Id => this.id;
        }

        record InsideId {
            private readonly Guid id;

            public InsideId() {
                this.id = Guid.NewGuid();
            }

            public InsideId(Guid id) {
                this.id = id;
            }

            public Guid Id => this.id;
        }

        record MultiNonPrimitiveId {
            private readonly InsideId leftId;

            private readonly InsideId rightId;

            public MultiNonPrimitiveId() {
                this.leftId  = new InsideId();
                this.rightId = new InsideId();
            }

            public MultiNonPrimitiveId(InsideId leftId, InsideId rightId) {
                this.leftId  = leftId;
                this.rightId = rightId;
            }

            public InsideId LeftId => this.leftId;

            public InsideId RightId => this.rightId;
        }

        class MultiNonPrimitiveIdFieldIdEntity {
            private readonly MultiNonPrimitiveId id;

            private readonly string name;

            public MultiNonPrimitiveIdFieldIdEntity(string name) {
                this.id   = new MultiNonPrimitiveId();
                this.name = name;
            }

            public string Name => this.name;

            public MultiNonPrimitiveId Id => this.id;
        }
    }
}