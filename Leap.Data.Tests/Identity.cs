namespace Leap.Data.Tests {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Leap.Data.Configuration;
    using Leap.Data.Humanizer;
    using Leap.Data.Schema;
    using Leap.Data.Schema.Conventions;
    using Leap.Data.SqlServer;

    using Xunit;

    public class Identity {
        [Fact]
        public async Task Roundtrips() {
            var thing = new EntityWithIdentity { Name = "Bob" };
            var sf = MakeTarget();
            var insertSession = sf.StartSession();
            insertSession.Add(thing);
            await insertSession.SaveChangesAsync();
            Assert.NotEqual(0, thing.Id);
            var inspector = insertSession.Inspect(thing);
            Assert.NotEqual(0, inspector.GetColumnValue<long>("Id"));
            Assert.Equal(thing.Id, inspector.GetColumnValue<long>("Id"));

            var selectSession = sf.StartSession();
            var personAgain = await selectSession.Get<EntityWithIdentity>().SingleAsync(thing.Id);
            Assert.NotEqual(0, personAgain.Id);
            var secondInspector = selectSession.Inspect(personAgain);
            Assert.NotEqual(0, secondInspector.GetColumnValue<long>("Id"));
            Assert.Equal(personAgain.Id, secondInspector.GetColumnValue<long>("Id"));
        }

        private static ISessionFactory MakeTarget()
        {
            var schemaBuilder = new SchemaBuilder().AddTypes("EntityWithIdentities", typeof(EntityWithIdentity)).UseHumanizerPluralization().UseSqlServerConvention().UseConvention(new ComputedConvention());

            var testSchema = schemaBuilder.Build();
            var sessionFactory = new Configuration(testSchema).UseSqlServer("Server=.;Database=leap-data;Trusted_Connection=True;").BuildSessionFactory();
            return sessionFactory;
        }

        class EntityWithIdentity
        {
            public long Id { get; set; }
            
            public string Name { get; set; }
        }

        class ComputedConvention : IKeyComputedSchemaConvention {
            public bool IsKeyComputed(string collectionName, IEnumerable<Type> entityTypes) {
                return true;
            }
        }
    }
}