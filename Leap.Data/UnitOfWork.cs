namespace Leap.Data {
    using System.Collections.Generic;

    class UnitOfWork {
        private IList<IOperation> operations = new List<IOperation>();
        
        public void Add(IOperation operation) {
            this.operations.Add(operation);
        }
    }
}