namespace TildeSql.Infrastructure {
    using Fasterflect;

    public abstract class DomainEventWrapper {
        private long id;

        /// <summary>
        ///     This is an identity column in the database
        /// </summary>
        /// <remarks>
        ///     This will be probably be set to 0 as the JSON document is not updated - you should consult the metadata to get the
        ///     actual column value
        ///     if needed.
        /// </remarks>
        public long Id {
            get => this.id;
            set => this.id = value;
        }
    }

    public class DomainEventWrapper<TDomainEvent> : DomainEventWrapper {
        private readonly TDomainEvent domainEvent;

        public DomainEventWrapper(TDomainEvent domainEvent) {
            this.domainEvent = domainEvent;
        }

        public TDomainEvent DomainEvent => this.domainEvent;
    }

    public static class DomainEventWrapperExtensions {
        public static object GetDomainEvent(this DomainEventWrapper wrapper) {
            return wrapper.GetPropertyValue(nameof(DomainEventWrapper<string>.DomainEvent));
        }
    }
}
