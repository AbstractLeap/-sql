namespace Leap.Data.Tests.TestDomain.InheritanceFail
{
    class IntroductionRequest : MeetingRequest
    {
        public IntroductionRequest(string name)
            : base(name) { }

        protected override void OnAccepted() { }

        protected override void OnRejected() { }
    }
}
