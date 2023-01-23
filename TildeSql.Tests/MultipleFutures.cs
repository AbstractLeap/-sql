namespace TildeSql.Tests {
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using TildeSql.Tests.TestDomain.MultipleFutures;

    using Xunit;

    public class MultipleFutures {
        [Fact]
        public async Task MultipleFuturesWorks() {
            var sessionFactory = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var insertSession = sessionFactory.StartSession();
            var orange = new Orange { Type = "Yellow!" };
            var apple = new Apple { Type   = "Granny smith" };
            insertSession.Add(orange);
            insertSession.Add(apple);
            await insertSession.SaveChangesAsync();

            var selectSession = sessionFactory.StartSession();
            var futureOrange = selectSession.Get<Orange>().MultipleFuture(new [] { orange.Id });
            var futureApple = selectSession.Get<Apple>().MultipleFuture(new [] { apple.Id });

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
            var sessionFactory = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var insertSession = sessionFactory.StartSession();
            var orange = new Orange { Type = "Yellow!" };
            var apple = new Apple { Type   = "Granny smith" };
            insertSession.Add(orange);
            insertSession.Add(apple);
            await insertSession.SaveChangesAsync();

            var selectSession = sessionFactory.StartSession();
            var futureOrange = selectSession.Get<Orange>().MultipleFuture(new [] { orange.Id });
            var futureApple = selectSession.Get<Apple>().MultipleFuture(new [] { apple.Id });

            var apples = await futureApple.ToArrayAsync();
            var oranges = await futureOrange.ToArrayAsync();

            Assert.Single(oranges);
            Assert.Single(apples);
            Assert.Equal(orange.Id, oranges[0].Id);
            Assert.Equal(orange.Type, oranges[0].Type);
            Assert.Equal(apple.Id, apples[0].Id);
            Assert.Equal(apple.Type, apples[0].Type);
        }

        [Fact]
        public async Task NullFutureWorks() {
            var sessionFactory = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var insertSession = sessionFactory.StartSession();
            var apple = new Apple { Type = "Granny smith" };
            insertSession.Add(apple);
            await insertSession.SaveChangesAsync();

            var selectSession = sessionFactory.StartSession();
            var futureOrange = selectSession.Get<Orange>().MultipleFuture(new [] { Guid.NewGuid() });
            var futureApple = selectSession.Get<Apple>().MultipleFuture(new [] { apple.Id });

            var oranges = await futureOrange.ToArrayAsync();
            var apples = await futureApple.ToArrayAsync();

            Assert.Empty(oranges);
            Assert.Single(apples);
            Assert.Equal(apple.Id, apples[0].Id);
            Assert.Equal(apple.Type, apples[0].Type);
        }

        [Fact]
        public async Task NullMultipleFutureWorks() {
            var sessionFactory = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var insertSession = sessionFactory.StartSession();
            var orange = new Orange { Type = "Green!" };
            var apple = new Apple { Type   = "Granny smith" };
            insertSession.Add(orange);
            insertSession.Add(apple);
            await insertSession.SaveChangesAsync();

            var selectSession = sessionFactory.StartSession();
            var futureEmptyOrange = selectSession.Get<Orange>().MultipleFuture(new [] { Guid.NewGuid() });
            var futureApple = selectSession.Get<Apple>().Where("id = @Id", new { apple.Id }).Future();
            var futureOrange = selectSession.Get<Orange>().MultipleFuture(new [] { orange.Id });

            var emptyOranges = await futureEmptyOrange.ToArrayAsync();
            var fetchedApple = await futureApple.FirstOrDefaultAsync();
            var oranges = await futureOrange.ToArrayAsync();

            Assert.Empty(emptyOranges);
            Assert.NotNull(fetchedApple);
            Assert.Single(oranges);
            Assert.Equal(apple.Id, fetchedApple.Id);
            Assert.Equal(apple.Type, fetchedApple.Type);
            Assert.Equal(orange.Id, oranges[0].Id);
            Assert.Equal(orange.Type, oranges[0].Type);
        }

        [Fact]
        public async Task NullFutureInWrongOrderWorks() {
            var sessionFactory = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var insertSession = sessionFactory.StartSession();
            var apple = new Apple { Type = "Granny smith" };
            insertSession.Add(apple);
            await insertSession.SaveChangesAsync();

            var selectSession = sessionFactory.StartSession();
            var futureOrange = selectSession.Get<Orange>().MultipleFuture(new[] { Guid.NewGuid() });
            var futureApple = selectSession.Get<Apple>().MultipleFuture(new [] { apple.Id });

            var apples = await futureApple.ToArrayAsync();
            var oranges = await futureOrange.ToArrayAsync();

            Assert.Empty(oranges);
            Assert.Single(apples);
            Assert.Equal(apple.Id, apples[0].Id);
            Assert.Equal(apple.Type, apples[0].Type);
        }
    }
}