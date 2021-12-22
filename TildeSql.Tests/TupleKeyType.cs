namespace TildeSql.Tests {
    using System.Threading.Tasks;

    using TildeSql.Tests.TestDomain.TupleKeyType;

    using Xunit;

    public class TupleKeyType {
        [Fact]
        public async Task GetAllTypes() {
            var sf = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
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
    }
}