namespace Leap.Data.Tests {
    using System;
    using System.Threading.Tasks;

    using Leap.Data.Configuration;
    using Leap.Data.Humanizer;
    using Leap.Data.JsonNet;
    using Leap.Data.Schema;
    using Leap.Data.SqlServer;

    using Xunit;

    public class TypedSerialization {
        [Fact]
        public async Task Roundtrips() {
            var person = new Person(new Name("Joe", "Foo")) { MyFoo = new Foo(true) };
            var sf = MakeTarget();
            var insertSession = sf.StartSession();
            insertSession.Add(person);
            await insertSession.SaveChangesAsync();

            var selectSession = sf.StartSession();
            var personAgain = await selectSession.Get<Person>().SingleAsync(person.Id);
            Assert.IsType<Foo>(personAgain.MyFoo);
            Assert.True(personAgain.MyFoo.IsIt);
        }
        
        private static ISessionFactory MakeTarget()
        {
            var schemaBuilder = new SchemaBuilder().AddTypes(typeof(Person)).UseHumanizerPluralization().UseSqlServerConvention();

            var testSchema = schemaBuilder.Build();
            var sessionFactory = new Configuration(testSchema).UseJsonNetFieldSerialization()
                                                              .UseSqlServer("Server=.;Database=leap-data;Trusted_Connection=True;")
                                                              .BuildSessionFactory();
            return sessionFactory;
        }

        class Person
        {
            private readonly Name name;

            private readonly PersonId id;

            public Person(Name name)
            {
                this.name = name;
                this.id = new PersonId();
            }

            public PersonId Id => this.id;

            public Name Name => this.name;
            
            public IFoo MyFoo { get; set; }
        }

        interface IFoo {
            bool IsIt { get; }
        }

        class Foo : IFoo {
            private readonly bool isIt;
            
            public Foo(bool isIt) {
                this.isIt = isIt;
            }

            public bool IsIt => this.isIt;
        }

        class Foo2 : IFoo
        {
            private bool isIt;

            public bool IsIt {
                get => this.isIt;
                set => this.isIt = value;
            }
        }

        record Name
        {
            private readonly string surname;

            private readonly string givenNames;

            public Name(string givenNames, string surname)
            {
                this.givenNames = givenNames;
                this.surname = surname;
            }

            public string Surname => this.surname;

            public string GivenNames => this.givenNames;
        }

        record PersonId
        {
            private readonly Guid id;

            public PersonId(Guid id)
            {
                this.id = id;
            }

            public PersonId()
            {
                this.id = Guid.NewGuid();
            }

            public Guid Id => this.id;
        }
    }
}