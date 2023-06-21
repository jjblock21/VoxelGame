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
        private CancellationTokenSource? _taskCts;
        private Action<CancellationToken, TArg> _work;
        private volatile JobState _state;
        private object _stateLockObject;
        private object _taskCtsLockObject;

        public JobState State { get => _state; }

        public RecurringTask(Action<CancellationToken, TArg> work)
        {
            _work = work;
            _stateLockObject = new object();
            _taskCtsLockObject = new object();
            _state = JobState.Inactive;
        }

        public Task StartCancelPrevious(TArg arg)
        {
            PrepareCts();
            /* Lock the _state variable (indirectly through lock object)
             * To prevent another thread executing this code at the same time to check the variable before the current
             * thread has modified it, which would allow them both to run simultaneously.
             */
            lock (_stateLockObject)
            {
                if (_state != JobState.Inactive)
                {
                    lock (_taskCtsLockObject)
                        _taskCts!.Cancel();
                }
                _state = JobState.WaitingToRun;
            }

            return StartNew(arg);
        }

        public Task? StartIfPreviousCompleted(TArg arg)
        {
            PrepareCts();
            // Same as in StartCancelRunning() but just exit if the job is already running.
            lock (_stateLockObject)
            {
                if (_state != JobState.Inactive) return null;
                _state = JobState.WaitingToRun;
            }

            return StartNew(arg);
        }

        public void CancelRunning()
        {
            lock (_stateLockObject)
            {
                if (_state == JobState.Inactive) return;
                lock (_taskCtsLockObject)
                    _taskCts!.Cancel(); // This should not be null if a task was started, if it is null just panic since something has already gone wrong.

                _state = JobState.Inactive; // Set _state to Inactive in case the task was canceled before starting.
            }
        }

        private void PrepareCts()
        {
            // The cancellation token source might be null if CancelRunning() is called before this finishes, so we need to lock the cts.
            lock (_taskCtsLockObject)
            {
                // Try to reuse the CTS or create a new one.
                if (_taskCts == null || !_taskCts.TryReset())
                {
                    _taskCts?.Dispose();
                    _taskCts = new CancellationTokenSource();
                }
            }
        }

        private Task StartNew(TArg arg)
        {
            CancellationToken token = _taskCts!.Token;
            return Task.Factory.StartNew(() => DoWork(token, arg), token);
        }

        private void DoWork(CancellationToken token, TArg arg)
        {
            _state = JobState.Running;
            try { _work(token, arg); }
            finally { _state = JobState.Inactive; }
        }

        public void Dispose()
        {
            CancelRunning(); // Cancel all tasks and dispose of the cts.
            lock (_taskCtsLockObject)
                _taskCts?.Dispose();
        }
    }

    public enum JobState
    {
        WaitingToRun,
        Running,
        Inactive
    }
}
