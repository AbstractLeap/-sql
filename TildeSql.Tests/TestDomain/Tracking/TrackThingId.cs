namespace TildeSql.Tests.TestDomain.Tracking {
    using System;

    public record TrackThingId {
        public TrackThingId() {
            this.Id = Guid.NewGuid();
        }

        public TrackThingId(Guid id) {
            this.Id = id;
        }

        public Guid Id { get; init; }
    }
}