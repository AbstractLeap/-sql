namespace TildeSql.JsonNet.Tests.ChangeDetector.Fields {
    public class WrapperFields {
        private int[]? values;

        public int[]? Values {
            get => this.values;
            set => this.values = value;
        }
    }
}