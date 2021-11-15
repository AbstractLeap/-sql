namespace Leap.Data.Tests {
    using System.Threading.Tasks;

    using Leap.Data.Tests.TestDomain.PlayExtraColumns;

    using Xunit;

    public class PlayExtraColumns {
        [Fact]
        public async Task ItWorks() {
            var target = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var person = new Person(new Name("Mark", "Jerzykowski"));
            person.Email = new Email("mark@abstractleap.com");
            var insertSession = target.StartSession();
            insertSession.Add(person);
            await insertSession.SaveChangesAsync();

            var selectSession = target.StartSession();
            var personAgain = await selectSession.Get<Person>().SingleAsync(person.Id);
            Assert.Equal(person.Id, personAgain.Id);
            Assert.Equal(person.Name, personAgain.Name);
            Assert.Equal(person.Email, personAgain.Email);
            var metaData = selectSession.Inspect(personAgain);
            var emailColumnValue = metaData.GetColumnValue<string>("Email");
            var fullNameColumnValue = metaData.GetColumnValue<string>("Fullname");
            Assert.Equal(person.Email.Address, emailColumnValue);
            Assert.Equal("Mark Jerzykowski", fullNameColumnValue);
        }
    }
}