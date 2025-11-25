namespace TildeSql.Tests.TestDomain.Join {
    public class Join {
        private string title;

        public Join(string title) {
            this.JoinId = new JoinId();
            this.Title  = title;
        }

        public JoinId JoinId { get; init; }

        public string Title {
            get => this.title;
            set => this.title = value;
        }
    }
}