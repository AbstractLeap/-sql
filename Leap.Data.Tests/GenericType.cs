namespace Leap.Data.Tests {
    using System;
    using System.Threading.Tasks;

    using Leap.Data.Configuration;
    using Leap.Data.Humanizer;
    using Leap.Data.Schema;
    using Leap.Data.SqlServer;

    using Xunit;

    public class GenericType {
        [Fact]
        public async Task Roundtrips() {
            var thing = new Entity<Foo>(new Foo { Name = "Foofoo" });
            var sf = MakeTarget();
            var insertSession = sf.StartSession();
            insertSession.Add(thing);
            await insertSession.SaveChangesAsync();

            var selectSession = sf.StartSession();
            var personAgain = await selectSession.Get<Entity<Foo>>().SingleAsync(thing.Id);
            Assert.Equal("Foofoo", thing.Thing.Name);
        }

        private static ISessionFactory MakeTarget() {
            var schemaBuilder = new SchemaBuilder().AddTypes("Entities", typeof(Entity<>)).UseHumanizerPluralization().UseSqlServerConvention();

            var testSchema = schemaBuilder.Build();
            var sessionFactory = new Configuration(testSchema).UseSqlServer("Server=.;Database=leap-data;Trusted_Connection=True;").BuildSessionFactory();
            return sessionFactory;
        }

        class Foo {
            public string Name { get; set; }
        }

        class Bar {
            public string Description { get; set; }
        }

        class Entity<T> {
            public Entity(T thing) {
                this.Thing = thing;
            }

            public Guid Id { get; } = Guid.NewGuid();

            public T Thing { get; set; }

            public DateTime Date = DateTime.UtcNow;
        }
    }
}