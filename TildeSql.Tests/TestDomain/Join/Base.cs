namespace TildeSql.Tests.TestDomain.Join {
    public class Base {
        private readonly JoinId joinId;

        private string title;

        public Base(string title, JoinId joinId) {
            this.joinId = joinId;
            this.BaseId = new BaseId();
            this.Title  = title;
        }

        public BaseId BaseId { get; init; }

        public string Title {
            get => this.title;
            set => this.title = value;
        }

        public JoinId JoinId => this.joinId;
    }
}