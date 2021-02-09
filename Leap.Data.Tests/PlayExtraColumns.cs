namespace Leap.Data.Tests {
    using System;
    using System.Threading.Tasks;

    using Leap.Data.Configuration;
    using Leap.Data.Humanizer;
    using Leap.Data.JsonNet;
    using Leap.Data.Schema;
    using Leap.Data.SqlServer;

    using Xunit;

    public class PlayExtraColumns {
        [Fact]
        public async Task ItWorks() {
            var target = MakeTarget();
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

        private static ISessionFactory MakeTarget() {
            var schemaBuilder = new SchemaBuilder().AddTypes(typeof(Person)).UseHumanizerPluralization().UseSqlServerConvention();
            schemaBuilder.Setup<Person>().AddComputedColumn<string>("Email", "$.email.emailAddress");
            schemaBuilder.Setup<Person>().AddProjectionColumn("Fullname", person => (person.Name.GivenNames ?? string.Empty) + " " + (person.Name.Surname ?? string.Empty));

            var testSchema = schemaBuilder.Build();
            var sessionFactory = new Configuration(testSchema).UseJsonNetFieldSerialization()
                                                              .UseSqlServer("Server=.;Database=leap-data;Trusted_Connection=True;")
                                                              .BuildSessionFactory();
            return sessionFactory;
        }

        class Person {
            private readonly Name name;

            private readonly PersonId id;

            private Email email;

            public Person(Name name) {
                this.name = name;
                this.id   = new PersonId();
            }

            public PersonId Id => this.id;

            public Name Name => this.name;

            public Email Email {
                get => this.email;
                set => this.email = value;
            }
        }

        class Email {
            private readonly string address;

            public Email(string emailAddress) {
                this.address = emailAddress;
            }

            public string Address => this.address;

            protected bool Equals(Email other) {
                return this.Address == other.Address;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Email)obj);
            }

            public override int GetHashCode() {
                return (this.Address != null ? this.Address.GetHashCode() : 0);
            }
        }

        record Name {
            private readonly string surname;

            private readonly string givenNames;

            public Name(string givenNames, string surname) {
                this.givenNames = givenNames;
                this.surname    = surname;
            }

            public string Surname => this.surname;

            public string GivenNames => this.givenNames;
        }

        record PersonId {
            private readonly Guid id;

            public PersonId(Guid id) {
                this.id = id;
            }
            
            public PersonId() {
                this.id = Guid.NewGuid();
            }

            public Guid Id => this.id;
        }
    }
}