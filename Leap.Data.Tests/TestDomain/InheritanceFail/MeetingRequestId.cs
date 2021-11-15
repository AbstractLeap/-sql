using RT.Comb;
using System;

namespace Leap.Data.Tests.TestDomain.InheritanceFail
{
    record MeetingRequestId
    {
        private readonly Guid id;

        public MeetingRequestId()
            : this(Provider.Sql.Create()) { }

        public MeetingRequestId(Guid id)
        {
            this.id = id;
        }

        public Guid Id => this.id;

        public override string ToString()
        {
            return $"MeetingRequestId: {this.id}";
        }
    }
}
