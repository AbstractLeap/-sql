namespace TildeSql.JsonNet.Tests.ChangeDetector.Fields {
    using RT.Comb;

    public class AccountInfrastructure {
        private readonly AccountId accountId;

        private readonly string dbConnectionString;

        private readonly bool isSharedDatabase;

        public AccountInfrastructure(AccountId accountId, string dbConnectionString, bool isSharedDatabase) {
            this.accountId          = accountId;
            this.dbConnectionString = dbConnectionString;
            this.isSharedDatabase   = isSharedDatabase;
        }

        public string DatabaseConnectionString => this.dbConnectionString;

        public bool IsSharedDatabase => this.isSharedDatabase;
    }

    public record AccountId {
        private readonly Guid id;

        public AccountId()
            : this(Provider.Sql.Create()) { }

        public AccountId(Guid id) {
            this.id = id;
        }

        public Guid Id => this.id;

        public override string ToString() {
            return $"AccountId: {this.id}";
        }
    }
}