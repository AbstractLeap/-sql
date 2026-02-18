namespace TildeSql.JsonNet.Tests.ChangeDetector.Fields {
    public class AddressFields {
        private string? line1;

        private string? postcode;

        public string? Line1 {
            get => this.line1;
            set => this.line1 = value;
        }

        public string? Postcode {
            get => this.postcode;
            set => this.postcode = value;
        }
    }
}