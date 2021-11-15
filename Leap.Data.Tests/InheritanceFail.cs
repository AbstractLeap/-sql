namespace Leap.Data.Tests {
    using System;
    using System.Threading.Tasks;

    using Leap.Data.Configuration;
    using Leap.Data.Schema;
    using Leap.Data.SqlServer;
    using Leap.Data.Tests.TestDomain.InheritanceFail;
    using RT.Comb;

    using Xunit;

    public class InheritanceFail {
        [Fact]
        public async Task Fails() {
            var intro = new IntroductionRequest("foo");
            var sf = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var insertSession = sf.StartSession();
            insertSession.Add<MeetingRequest>(intro);
            await insertSession.SaveChangesAsync();

            var selectSession = sf.StartSession();
            var introAgain = await selectSession.Get<IntroductionRequest>().SingleAsync(intro.Id);
            Assert.Equal("foo", introAgain.Name);

            var introAgainAsMeetingRequest = await selectSession.Get<MeetingRequest>().SingleAsync(intro.Id);
            Assert.IsType<IntroductionRequest>(introAgainAsMeetingRequest);
        }

    }
}