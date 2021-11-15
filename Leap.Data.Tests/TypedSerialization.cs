namespace Leap.Data.Tests {
    using System.Threading.Tasks;

    using Leap.Data.Tests.TestDomain.TypedSerialization;

    using Xunit;

    public class TypedSerialization {
        [Fact]
        public async Task Roundtrips() {
            var person = new GenericContainer(new Name("Joe", "Foo")) { MyFoo = new Foo(true) };
            var sf = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var insertSession = sf.StartSession();
            insertSession.Add(person);
            await insertSession.SaveChangesAsync();

            var selectSession = sf.StartSession();
            var personAgain = await selectSession.Get<GenericContainer>().SingleAsync(person.Id);
            Assert.IsType<Foo>(personAgain.MyFoo);
            Assert.True(personAgain.MyFoo.IsIt);
        }
    }
}