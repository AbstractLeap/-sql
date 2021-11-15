namespace Leap.Data.Tests {
    using System.Threading.Tasks;

    using Leap.Data.Tests.TestDomain.Identity;

    using Xunit;

    public class Identity {
        [Fact]
        public async Task Roundtrips() {
            var thing = new EntityWithIdentity { Name = "Bob" };
            var sf = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
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
    }
}