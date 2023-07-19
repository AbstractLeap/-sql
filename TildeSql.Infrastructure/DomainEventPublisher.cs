namespace TildeSql.Infrastructure {
    using Microsoft.Extensions.DependencyInjection;

    using TildeSql.Model;

    public class DomainEventPublisher : IDomainEventPublisher {
        private readonly Func<IServiceProvider> serviceProviderFactory;

        public DomainEventPublisher(Func<IServiceProvider> serviceProviderFactory) {
            this.serviceProviderFactory = serviceProviderFactory;
        }

        public void Publish<T>(T domainEvent) where T : DomainEvent {
            this.serviceProviderFactory().GetRequiredService<ISession>().Add(domainEvent);
        }
    }


}