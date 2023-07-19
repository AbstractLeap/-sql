namespace TildeSql.Model {
    public abstract class DomainEvent {
        public DateTimeOffset DatePublished = DateTimeOffset.UtcNow;
    }
}