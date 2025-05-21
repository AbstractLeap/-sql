namespace TildeSql.Tests.TestDomain.Paging {
    public class Paged {
        private string title;

        public Paged(string title) {
            this.title = title;
            this.Id    = new PagedId();
        }

        public PagedId Id { get; init; }

        public string Title {
            get => this.title;
            set => this.title = value;
        }
    }
}