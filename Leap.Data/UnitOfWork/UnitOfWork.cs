namespace Leap.Data.UnitOfWork {
    using System.Collections.Generic;

    using Leap.Data.Operations;

    class UnitOfWork {
        private readonly IList<IOperation> operations = new List<IOperation>();

        public void Add(IOperation operation) {
            this.operations.Add(operation);
        }
    }
}