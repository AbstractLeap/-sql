namespace TildeSql.Tests.TestDomain.Tracking {
    public class TrackThing {
        private string name;

        private readonly TrackThingId id;

        private string type;

        public TrackThing(string name) {
            this.id = new TrackThingId();
            this.name = name;
        }

        public TrackThingId Id => this.id;

        public string Type {
            get => this.type;
            set => this.type = value;
        }

        public string Name {
            get => this.name;
            set => this.name = value;
        }
    }
}