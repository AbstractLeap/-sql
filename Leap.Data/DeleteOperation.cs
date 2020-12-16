namespace Leap.Data {
    internal class DeleteOperation : IOperation {
        private readonly object entity;

        public DeleteOperation(object entity) {
            this.entity = entity;
        }
    }
}