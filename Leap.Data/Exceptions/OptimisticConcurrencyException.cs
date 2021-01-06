namespace Leap.Data.Exceptions {
    using System;
    using System.Runtime.Serialization;

    using Leap.Data.Operations;

    [Serializable]
    public class OptimisticConcurrencyException : Exception {
        public IOperation Operation { get; }

        public OptimisticConcurrencyException(IOperation operation) {
            this.Operation = operation;
        }

        public OptimisticConcurrencyException(IOperation operation, string message)
            : base(message) {
            this.Operation = operation;
        }

        public OptimisticConcurrencyException(IOperation operation, string message, Exception inner)
            : base(message, inner) {
            this.Operation = operation;
        }

        protected OptimisticConcurrencyException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}