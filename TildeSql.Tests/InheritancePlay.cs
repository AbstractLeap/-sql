namespace TildeSql.Tests {
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using TildeSql.Configuration;
    using TildeSql.Schema;
    using TildeSql.Schema.Conventions;
    using TildeSql.SqlServer;
    using TildeSql.Tests.TestDomain.InheritancePlay;

    using Xunit;

    public class InheritancePlay {
        [Fact]
        public async Task GetAllTypes() {
            var sf = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
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
        public async Task QuerySubType()
        {
            var sf = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
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
        public async Task Contravariance()
        {
            var sf = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var session = sf.StartSession();
            var poodle = new Poodle("Paul");
            session.Add(poodle);

            var dog = await session.Get<Dog>().SingleAsync(poodle.Id);
            Assert.Same(poodle, dog);
        }

        [Fact]
        public async Task ContravarianceFail()
        {
            var sf = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var session = sf.StartSession();
            var poodle = new Cat("Paul");
            session.Add(poodle);
            Assert.Null(await session.Get<Dog>().SingleAsync(poodle.Id));
        }
        
    }
}