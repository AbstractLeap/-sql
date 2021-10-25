namespace Leap.Data.Tests {
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Leap.Data.Configuration;
    using Leap.Data.Humanizer;
    using Leap.Data.JsonNet;
    using Leap.Data.Schema;
    using Leap.Data.SqlServer;

    using Xunit;

    public class MultipleFutures {
        [Fact]
        public async Task MultipleFuturesWorks() {
            var testSchema = new SchemaBuilder().AddTypes(typeof(Orange), typeof(Apple)).UseSqlServerConvention().UseHumanizerPluralization().Build();
            var sessionFactory = new Configuration(testSchema).UseJsonNetFieldSerialization()
                                                              .UseSqlServer("Server=.;Database=leap-data;Trusted_Connection=True;")
                                                              .BuildSessionFactory();
            var insertSession = sessionFactory.StartSession();
            var orange = new Orange { Type = "Yellow!" };
            var apple = new Apple { Type   = "Granny smith" };
            insertSession.Add(orange);
            insertSession.Add(apple);
            await insertSession.SaveChangesAsync();

            var selectSession = sessionFactory.StartSession();
            var futureOrange = selectSession.Get<Orange>().MultipleFuture(orange.Id);
            var futureApple = selectSession.Get<Apple>().MultipleFuture(apple.Id);

            var oranges = await futureOrange.ToArrayAsync();
            var apples = await futureApple.ToArrayAsync();

            Assert.Single(oranges);
            Assert.Single(apples);
            Assert.Equal(orange.Id, oranges[0].Id);
            Assert.Equal(orange.Type, oranges[0].Type);
            Assert.Equal(apple.Id, apples[0].Id);
            Assert.Equal(apple.Type, apples[0].Type);
        }

        [Fact]
        public async Task MultipleFuturesInWrongOrderWorks() {
            var testSchema = new SchemaBuilder().AddTypes(typeof(Orange), typeof(Apple)).UseSqlServerConvention().UseHumanizerPluralization().Build();
            var sessionFactory = new Configuration(testSchema).UseJsonNetFieldSerialization()
                                                              .UseSqlServer("Server=.;Database=leap-data;Trusted_Connection=True;")
                                                              .BuildSessionFactory();
            var insertSession = sessionFactory.StartSession();
            var orange = new Orange { Type = "Yellow!" };
            var apple = new Apple { Type   = "Granny smith" };
            insertSession.Add(orange);
            insertSession.Add(apple);
            await insertSession.SaveChangesAsync();

            var selectSession = sessionFactory.StartSession();
            var futureOrange = selectSession.Get<Orange>().MultipleFuture(orange.Id);
            var futureApple = selectSession.Get<Apple>().MultipleFuture(apple.Id);

            var apples = await futureApple.ToArrayAsync();
            var oranges = await futureOrange.ToArrayAsync();

            Assert.Single(oranges);
            Assert.Single(apples);
            Assert.Equal(orange.Id, oranges[0].Id);
            Assert.Equal(orange.Type, oranges[0].Type);
            Assert.Equal(apple.Id, apples[0].Id);
            Assert.Equal(apple.Type, apples[0].Type);
        }

        class Orange {
            public Guid Id { get; set; } = Guid.NewGuid();

            public string Type { get; set; }
        }

        class Apple {
            public Guid Id { get; set; } = Guid.NewGuid();

            public string Type { get; set; }
        }
    }
}