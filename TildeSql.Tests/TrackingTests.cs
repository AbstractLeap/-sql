namespace TildeSql.Tests {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using TildeSql.Tests.TestDomain.Tracking;

    using Xunit;

    public class TrackingTests {
        [Fact]
        public async Task NoTrackingIsNotSaved() {
            var thing = new TrackThing("foo");
            var sf = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var insertSession = sf.StartSession();
            insertSession.Add(thing);
            await insertSession.SaveChangesAsync();

            var selectSession = sf.StartSession();
            var thingAgain = await selectSession.Get<TrackThing>().SingleAsync(thing.Id, disableTracking: true);

            Assert.NotSame(thing,  thingAgain);

            thingAgain.Name = "bar";
            await selectSession.SaveChangesAsync();

            var selectSession2 = sf.StartSession();
            var thingAgain2 = await selectSession2.Get<TrackThing>().SingleAsync(thing.Id);
            Assert.NotSame(thing, thingAgain2);
            Assert.NotSame(thingAgain, thingAgain2);

            Assert.Equal("foo", thingAgain2.Name);
        }

        [Fact]
        public async Task NotTrackedThenTrackedIsSaved() {
            var thing = new TrackThing("foo");
            var sf = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var insertSession = sf.StartSession();
            insertSession.Add(thing);
            await insertSession.SaveChangesAsync();

            var selectSession = sf.StartSession();
            var thingAgain = await selectSession.Get<TrackThing>().SingleAsync(thing.Id, disableTracking: true);

            Assert.NotSame(thing, thingAgain);


            // get tracked this time
            var thingAgainAgain = await selectSession.Get<TrackThing>().SingleAsync(thing.Id);
            Assert.NotSame(thingAgain, thingAgainAgain); // not tracked so not from identity map
            thingAgainAgain.Name = "bar";

            await selectSession.SaveChangesAsync(); // changes now saved

            var selectSession2 = sf.StartSession();
            var thingAgain2 = await selectSession2.Get<TrackThing>().SingleAsync(thing.Id);
            Assert.NotSame(thing, thingAgain2);
            Assert.NotSame(thingAgain, thingAgain2);

            Assert.Equal("bar", thingAgain2.Name);
        }

        [Fact]
        public async Task OnceTrackedAlwaysTrackedIsSaved() {
            var thing = new TrackThing("foo");
            var sf = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var insertSession = sf.StartSession();
            insertSession.Add(thing);
            await insertSession.SaveChangesAsync();

            var selectSession = sf.StartSession();
            var thingAgain = await selectSession.Get<TrackThing>().SingleAsync(thing.Id);

            Assert.NotSame(thing, thingAgain);

            // get tracked this time
            var thingAgainAgain = await selectSession.Get<TrackThing>().SingleAsync(thing.Id, disableTracking: true);
            Assert.Same(thingAgain, thingAgainAgain); // from identity map

            thingAgainAgain.Name = "bar";
            await selectSession.SaveChangesAsync(); // changes now saved

            var selectSession2 = sf.StartSession();
            var thingAgain2 = await selectSession2.Get<TrackThing>().SingleAsync(thing.Id);
            Assert.NotSame(thing, thingAgain2);
            Assert.NotSame(thingAgain, thingAgain2);

            Assert.Equal("bar", thingAgain2.Name);
        }

        [Fact]
        public async Task NoTrackedQueryDoesNotSave() {
            var sf = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var insertSession = sf.StartSession();
            for (var i = 0; i < 20; i++) {
                insertSession.Add(new TrackThing("foo") { Type = $"type{i % 2}" });
            }

            await insertSession.SaveChangesAsync();

            var selectSession = sf.StartSession();
            var things = await selectSession.Get<TrackThing>().Where("json_value(Document, '$.type') = 'type0'").NoTracking().ToArrayAsync();

            Assert.Equal(10, things.Length);
            foreach (var thing in things) {
                thing.Name = "Bar";
            }

            await selectSession.SaveChangesAsync(); // changes NOT saved

            var selectSession2 = sf.StartSession();
            var thingsAgain = await selectSession.Get<TrackThing>().Where("json_value(Document, '$.type') = 'type0'").NoTracking().ToArrayAsync();

            Assert.All(thingsAgain, t => {
                Assert.Equal("foo", t.Name);
            });
        }
    }
}