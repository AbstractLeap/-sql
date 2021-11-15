using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leap.Data.Tests.TestDomain.InheritanceFail
{

    abstract class MeetingRequest
    {
        protected readonly string name;

        protected readonly MeetingRequestId id;

        private LessonRequestStatus? status;

        public MeetingRequest(string name)
        {
            this.name = name;
            this.id = new MeetingRequestId();
        }

        public MeetingRequestId Id => this.id;

        public bool IsAccepted => this.status == LessonRequestStatus.Accepted;

        public bool IsRejected => this.status == LessonRequestStatus.Rejected;

        public string Name => this.name;

        public void Accept()
        {
            if (!this.status.HasValue)
            {
                // we should only update the time period override if we are actually accepting the lesson 

                this.status = LessonRequestStatus.Accepted;
                this.OnAccepted();
            }
        }

        public void Reject()
        {
            if (!this.status.HasValue)
            {
                this.status = LessonRequestStatus.Rejected;
                this.OnRejected();
            }
        }

        protected abstract void OnAccepted();

        protected abstract void OnRejected();

        enum LessonRequestStatus
        {
            Accepted = 1,

            Rejected = 2
        }
    }
}
