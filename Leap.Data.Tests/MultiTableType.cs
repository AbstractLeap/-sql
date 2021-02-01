namespace Leap.Data.Tests {
    using System;
    using System.Threading.Tasks;

    using Leap.Data.Configuration;
    using Leap.Data.Humanizer;
    using Leap.Data.JsonNet;
    using Leap.Data.Schema;
    using Leap.Data.SqlServer;

    using Xunit;

    public class MultiTableType {
        private const string NotApprovedCollectionName = "NotApprovedPeople";
        private const string ApprovedCollectionName = "ApprovedPeople";
        private const string ArchivedCollectionName = "ArchivedPeople";


        [Fact]
        public async Task ItWorks() {
            var sf = MakeTarget();
            var insertSession1 = sf.StartSession();
            var newbie = new Person(new Name("Joe", "Wright"));
            insertSession1.Add(newbie, NotApprovedCollectionName);
            await insertSession1.SaveChangesAsync();

            var selectSession = sf.StartSession();
            var newbieAgain = await selectSession.Get<Person>(NotApprovedCollectionName).SingleAsync(newbie.Id);
            Assert.Equal(newbie.Id, newbieAgain.Id);
            Assert.Null(await selectSession.Get<Person>(ApprovedCollectionName).SingleAsync(newbie.Id));
            Assert.Null(await selectSession.Get<Person>(ArchivedCollectionName).SingleAsync(newbie.Id));
            
            selectSession.Delete(newbieAgain, NotApprovedCollectionName);
            selectSession.Add(newbieAgain, ApprovedCollectionName);

            var newbieNotApprovedAgain = await selectSession.Get<Person>(NotApprovedCollectionName).SingleAsync(newbieAgain.Id);
            Assert.Null(newbieNotApprovedAgain);
            var newbieApprovedAgain = await selectSession.Get<Person>(ApprovedCollectionName).SingleAsync(newbieAgain.Id);
            Assert.Same(newbieAgain, newbieApprovedAgain);
            await selectSession.SaveChangesAsync();

            var selectSession2 = sf.StartSession();
            var newbieNotApprovedAgainTheSecond = await selectSession2.Get<Person>(NotApprovedCollectionName).SingleAsync(newbieAgain.Id);
            Assert.Null(newbieNotApprovedAgainTheSecond);
            var newbieApprovedAgainTheSecond = await selectSession2.Get<Person>(ApprovedCollectionName).SingleAsync(newbieAgain.Id);
            Assert.Equal(newbieAgain, newbieApprovedAgainTheSecond);
            Assert.NotSame(newbieAgain, newbieApprovedAgainTheSecond);
        }

        [Fact]
        public async Task EntityInTwoCollectionsIsSameInSession() {
            var sf = MakeTarget();
            var insertSession1 = sf.StartSession();
            var newbie = new Person(new Name("Joe", "Wright"));
            insertSession1.Add(newbie, NotApprovedCollectionName);
            insertSession1.Add(newbie, ApprovedCollectionName);
            await insertSession1.SaveChangesAsync();

            var selectSession = sf.StartSession();
            var newbieNotApproved = await selectSession.Get<Person>(NotApprovedCollectionName).SingleAsync(newbie.Id);
            var newbieApproved = await selectSession.Get<Person>(ApprovedCollectionName).SingleAsync(newbie.Id);
            Assert.Same(newbieNotApproved, newbieApproved);
        }

        private static ISessionFactory MakeTarget() {
            var schemaBuilder = new SchemaBuilder().AddTypes(NotApprovedCollectionName, NotApprovedCollectionName, typeof(Person))
                                                   .AddTypes(ApprovedCollectionName, ApprovedCollectionName, typeof(Person))
                                                   .AddTypes(ArchivedCollectionName, ArchivedCollectionName, typeof(Person))
                                                   .UseHumanizerPluralization();

            var testSchema = schemaBuilder.Build();
            var sessionFactory = new Configuration(testSchema).UseJsonNetFieldSerialization()
                                                              .UseSqlServer("Server=.;Database=leap-data;Trusted_Connection=True;")
                                                              .BuildSessionFactory();
            return sessionFactory;
        }

        class Person {
            protected bool Equals(Person other) {
                return Equals(this.id, other.id);
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Person)obj);
            }

            public override int GetHashCode() {
                return (this.id != null ? this.id.GetHashCode() : 0);
            }

            private readonly Name name;

            private readonly PersonId id;

            public Person(Name name) {
                this.name = name;
                this.id   = new PersonId();
            }

            public PersonId Id => this.id;

            public Name Name => this.name;
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