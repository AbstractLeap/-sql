namespace TildeSql.Model {
    using RT.Comb;

    public abstract record GuidBackedEntityId {
        private readonly Guid id;

        public GuidBackedEntityId() {
            id = Provider.Sql.Create();
        }

        public GuidBackedEntityId(string id) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            if (!id.StartsWith(this.Prefix + "_"))
                throw new ArgumentOutOfRangeException(nameof(id), $"Ids of type {this.GetType().FullName} must start with {this.Prefix}_");
            if (!Guid.TryParse(id.Substring(4), out var guid))
                throw new ArgumentOutOfRangeException(nameof(id), "Expected a guid but got " + id.Substring(4));
            this.id = guid;
        }

        protected abstract string Prefix { get; }

        public override string ToString() {
            return $"{this.Prefix}_{this.id}";
        }
    }
}