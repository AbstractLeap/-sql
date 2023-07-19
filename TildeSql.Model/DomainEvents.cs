namespace TildeSql.Model {
    public static class DomainEvents {
        private static IDomainEventPublisher publisher;

        public static void SetPublisher(IDomainEventPublisher domainEventPublisher) {
            publisher = domainEventPublisher;
        }

        public static void Raise(DomainEvent domainEvent) {
            if (publisher == null) {
                throw new InvalidOperationException("No publisher has been set");
            }

            publisher.Publish(domainEvent);
        }
    }
}