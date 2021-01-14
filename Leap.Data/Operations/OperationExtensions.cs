namespace Leap.Data.Operations {
    static class OperationExtensions {
        public static bool IsAddOperation(this IOperation operation) {
            return operation.GetType().GetGenericTypeDefinition() == typeof(AddOperation<>);
        }

        public static bool IsUpdateOperation(this IOperation operation) {
            return operation.GetType().GetGenericTypeDefinition() == typeof(UpdateOperation<>);
        }

        public static bool IsDeleteOperation(this IOperation operation) {
            return operation.GetType().GetGenericTypeDefinition() == typeof(DeleteOperation<>);
        }
    }
}