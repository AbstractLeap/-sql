namespace Leap.Data.Tests {
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Leap.Data.Configuration;
    using Leap.Data.Schema;
    using Leap.Data.Schema.Conventions;
    using Leap.Data.SqlServer;

    using Xunit;

    public class InheritancePlay {
        [Fact]
        public async Task GetAllTypes() {
            var sf = MakeTarget();
            var insertSession = sf.StartSession();
            var cat = new Cat("Trevor");
            insertSession.Add(cat);

            var poodle = new Poodle("Bubbles");
            insertSession.Add(poodle);

            var terrier = new Terrier("Jack");
            insertSession.Add(terrier);
            await insertSession.SaveChangesAsync();

            var querySession1 = sf.StartSession();
            var allAnimals = await querySession1.Get<IAnimal>().ToListAsync();
            Assert.Contains(cat, allAnimals);
            Assert.Contains(poodle, allAnimals);
            Assert.Contains(terrier, allAnimals);
        }

        [Fact]
        public async Task QuerySubType() {
            var sf = MakeTarget();
            var insertSession = sf.StartSession();
            var cat = new Cat("Trevor");
            insertSession.Add(cat);

            var poodle = new Poodle("Bubbles");
            insertSession.Add(poodle);

            var terrier = new Terrier("Jack");
            insertSession.Add(terrier);
            await insertSession.SaveChangesAsync();

            var querySession1 = sf.StartSession();
            var allDogs = await querySession1.Get<Dog>().ToListAsync();
            Assert.Contains(poodle, allDogs);
            Assert.Contains(terrier, allDogs);
        }

        [Fact]
        public async Task Contravariance() {
            var sf = MakeTarget();
            var session = sf.StartSession();
            var poodle = new Poodle("Paul");
            session.Add(poodle);

            var dog = await session.Get<Dog>().SingleAsync(poodle.Id);
            Assert.Same(poodle, dog);
        }

        [Fact]
        public async Task ContravarianceFail() {
            var sf = MakeTarget();
            var session = sf.StartSession();
            var poodle = new Cat("Paul");
            session.Add(poodle);

            await Assert.ThrowsAsync<Exception>(async () => await session.Get<Dog>().SingleAsync(poodle.Id));
        }

        private static ISessionFactory MakeTarget() {
            var testSchema = new SchemaBuilder().AddTypes(typeof(IAnimal), typeof(Animal), typeof(Dog), typeof(Terrier), typeof(Poodle), typeof(Cat))
                                                .UseConvention(new Convention())
                                                .UseSqlServerConvention()
                                                .Build();
            var sessionFactory = new Configuration(testSchema).UseSqlServer("Server=.;Database=leap-data;Trusted_Connection=True;").BuildSessionFactory();
            return sessionFactory;
        }

        class Convention : DefaultSchemaConvention {
            public override string GetCollectionName(Type type) {
                return "Animals";
            }
        }

        record AnimalId {
            public AnimalId() {
                this.Id = Guid.NewGuid();
            }

            public AnimalId(Guid id) {
                this.Id = id;
            }

            public Guid Id { get; init; }
        }

        interface IAnimal {
            AnimalId Id { get; }

            void Eat();
        }

        abstract class Animal : IAnimal {
            protected bool Equals(Animal other) {
                return Equals(this.Id, other.Id);
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Animal)obj);
            }

            public override int GetHashCode() {
                return (this.Id != null ? this.Id.GetHashCode() : 0);
            }

            public Animal() {
                this.Id = new AnimalId();
            }

            public AnimalId Id { get; init; }

            public abstract void Eat();
        }

        abstract class Pet : Animal {
            public string Name { get; init; }
        }

        class Cat : Pet {
            public Cat(string name) {
                this.Name = name;
            }

            public override void Eat() {
                throw new NotImplementedException();
            }
        }

        abstract class Dog : Pet {
            public Dog(string name) {
                this.Name = name;
            }

            public override void Eat() {
                throw new NotImplementedException();
            }
        }

        class Terrier : Dog {
            public Terrier(string name)
                : base(name) { }
        }

        class Poodle : Dog {
            public Poodle(string name)
                : base(name) { }
        }
    }
}