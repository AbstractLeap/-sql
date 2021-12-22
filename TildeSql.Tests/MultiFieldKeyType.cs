namespace TildeSql.Tests {
    using System.Threading.Tasks;

    using TildeSql.Tests.TestDomain.MultiFieldKeyType;

    using Xunit;

    public class MultiFieldKeyType {
        [Fact]
        public async Task PrimitiveNestedRoundTrips() {
            var sf = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
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
            var sf = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
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
    }
}