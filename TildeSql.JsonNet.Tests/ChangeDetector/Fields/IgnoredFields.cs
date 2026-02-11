namespace TildeSql.JsonNet.Tests.ChangeDetector.Fields {
    using Newtonsoft.Json;

    public class IgnoredFields {
        private string? name;

        [JsonIgnore] // retained intentionally
        private string? secret;

        public string? Name {
            get => this.name;
            set => this.name = value;
        }

        // Helper to set secret during tests without exposing it for serialization
        public void SetSecret(string? s) => this.secret = s;
    }
}