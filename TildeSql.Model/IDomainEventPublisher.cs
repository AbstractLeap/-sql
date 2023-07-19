namespace TildeSql.Model {
    public interface IDomainEventPublisher {
        void Publish<T>(T domainEvent) where T : DomainEvent;
    }
}