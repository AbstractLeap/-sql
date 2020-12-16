namespace Leap.Data {
    internal class AddOperation : IOperation {
        private readonly object entity;

        public AddOperation(object entity) {
            this.entity = entity;
        }
    }
}