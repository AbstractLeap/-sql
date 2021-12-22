namespace TildeSql.Tests {
    using System.Threading.Tasks;

    using TildeSql.Tests.TestDomain.MultiTableType;

    using Xunit;

    public class MultiTableType {
        public const string NotApprovedCollectionName = "NotApprovedPeople";

        public const string ApprovedCollectionName = "ApprovedPeople";

        public const string ArchivedCollectionName = "ArchivedPeople";

        [Fact]
        public async Task ItWorks() {
            var sf = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
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
            var sf = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
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
    }
}