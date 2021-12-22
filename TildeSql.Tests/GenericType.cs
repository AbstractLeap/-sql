namespace TildeSql.Tests
{
    using System;
    using System.Threading.Tasks;
    using TildeSql;
    using TildeSql.Configuration;
    using TildeSql.Humanizer;
    using TildeSql.Schema;
    using TildeSql.SqlServer;
    using TildeSql.Tests.TestDomain.GenericType;

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
