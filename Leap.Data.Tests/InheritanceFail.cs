namespace Leap.Data.Tests {
    using System;
    using System.Threading.Tasks;

    using Leap.Data.Configuration;
    using Leap.Data.Schema;
    using Leap.Data.SqlServer;

    using RT.Comb;

    using Xunit;

    public class InheritanceFail {
        [Fact]
        public async Task Fails() {
            var intro = new IntroductionRequest("foo");
            var sf = MakeTarget();
            var insertSession = sf.StartSession();
            insertSession.Add<MeetingRequest>(intro);
            await insertSession.SaveChangesAsync();

            var selectSession = sf.StartSession();
            var introAgain = await selectSession.Get<IntroductionRequest>().SingleAsync(intro.Id);
            Assert.Equal("foo", introAgain.Name);

            var introAgainAsMeetingRequest = await selectSession.Get<MeetingRequest>().SingleAsync(intro.Id);
            Assert.IsType<IntroductionRequest>(introAgainAsMeetingRequest);
        }

        private static ISessionFactory MakeTarget() {
            var testSchema = new SchemaBuilder().AddTypes("MeetingRequests", typeof(MeetingRequest), typeof(LessonRequest), typeof(IntroductionRequest))
                                                .UseSqlServerConvention()
                                                .Build();
            var sessionFactory = new Configuration(testSchema).UseSqlServer("Server=.;Database=leap-data;Trusted_Connection=True;").BuildSessionFactory();
            return sessionFactory;
        }

        record MeetingRequestId {
            private readonly Guid id;

            public MeetingRequestId()
                : this(Provider.Sql.Create()) { }

            public MeetingRequestId(Guid id) {
                this.id = id;
            }

            public Guid Id => this.id;

            public override string ToString() {
                return $"MeetingRequestId: {this.id}";
            }
        }

        class LessonRequest : MeetingRequest {
            public LessonRequest(string name)
                : base(name) { }

            protected override void OnAccepted() { }

            protected override void OnRejected() {
                //TODO: we should handle this event
            }
        }

        class IntroductionRequest : MeetingRequest {
            public IntroductionRequest(string name)
                : base(name) { }

            protected override void OnAccepted() { }

            protected override void OnRejected() { }
        }

        abstract class MeetingRequest {
            protected readonly string name;

            protected readonly MeetingRequestId id;

            private LessonRequestStatus? status;

            public MeetingRequest(string name) {
                this.name = name;
                this.id   = new MeetingRequestId();
            }

            public MeetingRequestId Id => this.id;

            public bool IsAccepted => this.status == LessonRequestStatus.Accepted;

            public bool IsRejected => this.status == LessonRequestStatus.Rejected;

            public string Name => this.name;

            public void Accept() {
                if (!this.status.HasValue) {
                    // we should only update the time period override if we are actually accepting the lesson 

                    this.status = LessonRequestStatus.Accepted;
                    this.OnAccepted();
                }
            }

            public void Reject() {
                if (!this.status.HasValue) {
                    this.status = LessonRequestStatus.Rejected;
                    this.OnRejected();
                }
            }

            protected abstract void OnAccepted();

            protected abstract void OnRejected();

            enum LessonRequestStatus {
                Accepted = 1,

                Rejected = 2
            }
        }
    }
}