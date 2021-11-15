namespace Leap.Data.Tests.TestDomain.InheritanceFail
{
    class LessonRequest : MeetingRequest
    {
        public LessonRequest(string name)
            : base(name) { }

        protected override void OnAccepted() { }

        protected override void OnRejected()
        {
            //TODO: we should handle this event
        }
    }
}
