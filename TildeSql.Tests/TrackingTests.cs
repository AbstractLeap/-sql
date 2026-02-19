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

            // select thing, different session so not in identity map -> different instance
            var selectSession = sf.StartSession();
            var thingAgain = await selectSession.Get<TrackThing>().SingleAsync(thing.Id, enableTracking: false);

            Assert.NotSame(thing,  thingAgain);

            thingAgain.Name = "bar"; // does not save
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
            var thingAgain = await selectSession.Get<TrackThing>().SingleAsync(thing.Id, enableTracking: false);

            Assert.NotSame(thing, thingAgain);


            // get tracked this time
            var thingAgainAgain = await selectSession.Get<TrackThing>().SingleAsync(thing.Id);
            Assert.Same(thingAgain, thingAgainAgain); // come from identity map
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
            var thingAgainAgain = await selectSession.Get<TrackThing>().SingleAsync(thing.Id, enableTracking: false);
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
            var thingsAgain = await selectSession2.Get<TrackThing>().Where("json_value(Document, '$.type') = 'type0'").NoTracking().ToArrayAsync();

            Assert.All(thingsAgain, t => {
                Assert.Equal("foo", t.Name);
            });
        }

        [Fact]
        public async Task NoTrackedMultiQueryDoesOverrideSingle() {
            var sf = TestSessionFactoryBuilder.Build(TestSchemaBuilder.Build());
            var things = new List<TrackThing>();
            var insertSession = sf.StartSession();
            for (var i = 0; i < 2; i++) {
                var trackThing = new TrackThing("cat") { Type = $"multitype" };
                insertSession.Add(trackThing);
                things.Add(trackThing);
            }

            await insertSession.SaveChangesAsync();

            var selectSession = sf.StartSession();
            var thing1 = await selectSession.Get<TrackThing>().SingleAsync(things[0].Id);
            var firstTwoThings = await selectSession.Get<TrackThing>().MultipleAsync([things[0].Id, things[1].Id], enableTracking: false).ToArrayAsync();
            foreach (var thing in firstTwoThings) {
                thing.Name = "bat";
            }

            await selectSession.SaveChangesAsync(); // changes NOT saved

            var selectSession2 = sf.StartSession();
            var firstTwoThingsAgain = await selectSession2.Get<TrackThing>().MultipleAsync([things[0].Id, things[1].Id], enableTracking: false).ToArrayAsync();

            var thing1Again = firstTwoThingsAgain.Single(t => t.Id == thing1.Id);
            Assert.Equal("bat", thing1Again.Name);

            var notChangedThing = firstTwoThingsAgain.Single(t => t.Id != thing1Again.Id);
            Assert.Equal("cat", notChangedThing.Name);
        }
    }
}