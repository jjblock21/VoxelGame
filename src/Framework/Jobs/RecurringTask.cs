using System;
using System.Threading;
using System.Threading.Tasks;

namespace VoxelGame.Framework.Jobs
{
    /// <summary>
    /// Limits tasks processing the same work to a maximum of one running instance at any time.
    /// </summary>
    public class RecurringTask<TArg> : IDisposable
    {
        private CancellationTokenSource _taskCts;
        private Action<CancellationToken, TArg> _work;
        private volatile TaskState _state;
        private object _stateLockObject;

        public TaskState State { get => _state; }

        public RecurringTask(Action<CancellationToken, TArg> work)
        {
            _work = work;
            _taskCts = new CancellationTokenSource();
            _stateLockObject = new object();
            _state = TaskState.Inactive;
        }

        public Task StartCancelPrevious(TArg arg)
        {
            /* Lock the _state variable (indirectly through lock object)
             * To prevent another thread executing this code at the same time to check the variable before the current
             * thread has modified it, which would allow them both to run simultaneously.
             */
            lock (_stateLockObject)
            {
                if (_state != TaskState.Inactive)
                {
                    lock (_taskCts) _taskCts.Cancel();
                }
                _state = TaskState.WaitingToRun;
            }

            return StartNew(arg);
        }

        public Task? StartIfPreviousCompleted(TArg arg)
        {
            // Same as in StartCancelRunning() but just exit if the job is already running.
            lock (_stateLockObject)
            {
                if (_state != TaskState.Inactive) return null;
                _state = TaskState.WaitingToRun;
            }

            return StartNew(arg);
        }

        public void CancelRunning()
        {
            lock (_stateLockObject)
            {
                if (_state == TaskState.Inactive) return;
                lock (_taskCts) _taskCts.Cancel();

                _state = TaskState.Inactive; // Set _state to Inactive in case the task was canceled before starting.
            }
        }

        private Task StartNew(TArg arg)
        {
            PrepareCts();

            CancellationToken token = _taskCts.Token;
            return Task.Factory.StartNew(() => DoWork(token, arg), token);
        }

        private void PrepareCts()
        {
            // The cancellation token source might be null if CancelRunning() is called before this finishes, so we need to lock the cts.
            lock (_taskCts)
            {
                // Try to reuse the CTS or create a new one.
                if (!_taskCts.TryReset())
                {
                    _taskCts.Dispose();
                    _taskCts = new CancellationTokenSource();
                }
            }
        }

        private void DoWork(CancellationToken token, TArg arg)
        {
            _state = TaskState.Running;
            try { _work(token, arg); }
            finally { _state = TaskState.Inactive; }
        }

        public void Dispose()
        {
            CancelRunning(); // Cancel all tasks and dispose of the cts.
            lock (_taskCts) _taskCts?.Dispose();
        }

        public enum TaskState
        {
            WaitingToRun,
            Running,
            Inactive
        }
    }
}
