namespace Leap.Data.Exceptions {
    using System;
    using System.Runtime.Serialization;

    using Leap.Data.Internal;

    [Serializable]
    public class OptimisticConcurrencyException : Exception {
        public DatabaseRow DatabaseRow { get; }

        public OptimisticConcurrencyException(DatabaseRow databaseRow) {
            this.DatabaseRow = databaseRow;
        }

        public OptimisticConcurrencyException(DatabaseRow databaseRow, string message)
            : base(message) {
            this.DatabaseRow = databaseRow;
        }

        public OptimisticConcurrencyException(DatabaseRow databaseRow, string message, Exception inner)
            : base(message, inner) {
            this.DatabaseRow = databaseRow;
        }

        protected OptimisticConcurrencyException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}