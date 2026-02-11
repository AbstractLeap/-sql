namespace TildeSql.JsonNet.Tests.ChangeDetector.Fields {
    public class RenamedFields {
        // Backing field name will become JSON key "renamed" via resolver (_renamed -> "renamed")
        private string? renamed;

        public string? Name {
            get => this.renamed;
            set => this.renamed = value;
        }
    }
}