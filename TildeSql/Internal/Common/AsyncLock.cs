namespace TildeSql.Internal.Common {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class AsyncLock {
        private readonly SemaphoreSlim semaphore = new(1, 1);

        private readonly Task<IDisposable> releaser;

        public AsyncLock() {
            this.releaser = Task.FromResult((IDisposable)new Releaser(this));
        }

        public Task<IDisposable> LockAsync(CancellationToken cancellationToken = default) {
            var wait = this.semaphore.WaitAsync(cancellationToken);
            return wait.IsCompleted
                       ? this.releaser
                       : wait.ContinueWith(
                           (_, state) => (IDisposable)state,
                           this.releaser.Result,
                           CancellationToken.None,
                           TaskContinuationOptions.ExecuteSynchronously,
                           TaskScheduler.Default);
        }

        private sealed class Releaser : IDisposable {
            private readonly AsyncLock toRelease;

            internal Releaser(AsyncLock toRelease) {
                this.toRelease = toRelease;
            }

            public void Dispose() {
                this.toRelease.semaphore.Release();
            }
        }
    }
}