namespace TildeSql.Tests {
    using System;
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

        [Fact]
        public async Task CheckWithPrimitiveFieldsInTuple() {
            var sf = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var insertSession = sf.StartSession();
            var idOne = new OneId();
            var now = DateTime.UtcNow;
            var email = "foo@abstractleap.com";
            var thing = new TupleWithPrimitiveKeyTing(idOne, now, email);
            insertSession.Add(thing);

            await insertSession.SaveChangesAsync();

            var querySession1 = sf.StartSession();
            var thingAgain = await querySession1.Get<TupleWithPrimitiveKeyTing>().SingleAsync((idOne, now, email));
            Assert.Equal(email, thingAgain.Email);
            Assert.Equal(idOne, thingAgain.OneId);
            Assert.Equal(now, thingAgain.AtTime);
        }
    }
}