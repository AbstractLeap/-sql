namespace Leap.Data.Tests
{
    using System;
    using System.Threading.Tasks;
    using Leap.Data;
    using Leap.Data.Configuration;
    using Leap.Data.Humanizer;
    using Leap.Data.Schema;
    using Leap.Data.SqlServer;
    using Leap.Data.Tests.TestDomain.GenericType;
    using Xunit;

    public class GenericType
    {
        [Fact]
        public async Task Roundtrips()
        {
            var thing = new Entity<Foo>(new Foo { Name = "Foofoo" });
            var sf = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var insertSession = sf.StartSession();
            insertSession.Add(thing);
            await insertSession.SaveChangesAsync();

            var selectSession = sf.StartSession();
            var personAgain = await selectSession.Get<Entity<Foo>>().SingleAsync(thing.Id);
            Assert.Equal("Foofoo", thing.Thing.Name);
        }

        class Foo
        {
            public string Name { get; set; }
        }

        class Bar
        {
            public string Description { get; set; }
        }


    }
}
