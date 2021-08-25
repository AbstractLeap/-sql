namespace Leap.Data.Tests {
    using System;
    using System.Threading.Tasks;

    using Leap.Data.Configuration;
    using Leap.Data.JsonNet;
    using Leap.Data.Schema;
    using Leap.Data.SqlServer;

    using Xunit;

    public class TupleKeyType {
        [Fact]
        public async Task GetAllTypes() {
            var sf = MakeTarget();
            var insertSession = sf.StartSession();
            var idOne = new OneId();
            var idTwo = new TwoId();
            var thing = new TupleKeyTypeThing(idOne, idTwo, "Foo");
            insertSession.Add(thing);

            await insertSession.SaveChangesAsync();

            var querySession1 = sf.StartSession();
            var thingAgain = await querySession1.Get<TupleKeyTypeThing>().SingleAsync((idOne, idTwo));
            Assert.Equal("Foo", thingAgain.Name);
            Assert.Equal(idOne, thingAgain.OneId);
            Assert.Equal(idTwo, thingAgain.TwoId);
        }

        private static ISessionFactory MakeTarget() {
            var testSchema = new SchemaBuilder().AddTypes(typeof(TupleKeyTypeThing)).UseSqlServerConvention().Build();
            var sessionFactory = new Configuration(testSchema).UseJsonNetFieldSerialization()
                                                              .UseSqlServer("Server=.;Database=leap-data;Trusted_Connection=True;")
                                                              .BuildSessionFactory();
            return sessionFactory;
        }

        record OneId {
            private readonly Guid id;

            public OneId() {
                this.id = Guid.NewGuid();
            }

            public OneId(Guid id) {
                this.id = id;
            }
        }

        record TwoId {
            private readonly Guid id;

            public TwoId() {
                this.id = Guid.NewGuid();
            }

            public TwoId(Guid id) {
                this.id = id;
            }
        }

        class TupleKeyTypeThing {
            private readonly OneId oneId;

            private readonly TwoId twoId;

            private readonly string name;

            public TupleKeyTypeThing(OneId one, TwoId two, string name) {
                this.oneId = one;
                this.twoId = two;
                this.name  = name;
            }

            public string Name => this.name;

            public OneId OneId => this.oneId;

            public TwoId TwoId => this.twoId;
        }
    }
}