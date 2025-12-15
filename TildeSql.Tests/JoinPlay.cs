namespace TildeSql.Tests {
    using System.Linq;
    using System.Threading.Tasks;

    using TildeSql.Tests.TestDomain.Join;

    using Xunit;

    public class JoinPlay {
        [Fact]
        public async Task Joins() {
            var sf = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var insertSession = sf.StartSession();
            var join = new Join("Joined");
            insertSession.Add(join);

            var baseThing = new Base("Bubbles", join.JoinId);
            insertSession.Add(baseThing);

            var otherBaseThing = new Base("Other", null);
            insertSession.Add(otherBaseThing);

            await insertSession.SaveChangesAsync();

            var querySession1 = sf.StartSession();
            var allJoinedBases = await querySession1.Get<Base>()
                                                    .InnerJoin("Joins j")
                                                    .On("json_value(t.Document, '$.joinId.\"<Id>k__BackingField\"') = j.JoinId")
                                                    .ToListAsync();
            Assert.Single(allJoinedBases);
            Assert.Equal("Bubbles", allJoinedBases[0].Title);
            Assert.Equal(join.JoinId, allJoinedBases[0].JoinId);
        }
    }
}