namespace Leap.Data {
    using System;
    using System.Threading.Tasks;

    class LocalQueryExecutor {
        public bool CanExecute(IQuery query) {
            return false; // TODO implement
        }

        public async ValueTask<Maybe> ExecuteAsync(IQuery query) {
            throw new NotImplementedException();
        }
    }
}