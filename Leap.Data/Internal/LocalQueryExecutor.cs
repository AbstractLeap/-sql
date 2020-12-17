namespace Leap.Data.Internal {
    using System;
    using System.Threading.Tasks;

    using Leap.Data.Queries;
    using Leap.Data.Utilities;

    class LocalQueryExecutor {
        public bool CanExecute(IQuery query) {
            return false; // TODO implement
        }

        public async ValueTask<Maybe> ExecuteAsync(IQuery query) {
            throw new NotImplementedException();
        }
    }
}