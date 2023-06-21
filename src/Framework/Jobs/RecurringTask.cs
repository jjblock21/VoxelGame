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

        public JobState State { get => _state; }

        public RecurringTask(Action<CancellationToken, TArg> work)
        {
            _work = work;
            _stateLockObject = new object();
            _state = JobState.Inactive;
        }

        public Task StartCancelPrevious(TArg arg)
        {
            /* Lock the _state variable (indirectly through lock object)
             * To prevent another thread executing this code at the same time to check the variable before the current
             * thread has modified it, which would allow them both to run simultaneously.
             */
            lock (_stateLockObject)
            {
                if (_state != JobState.Inactive) _taskCts!.Cancel(); // Shouldn't be null if the condition passes.
                _state = JobState.WaitingToRun;
            }

            return StartNew(arg);
        }

        public Task? StartIfPreviousCompleted(TArg arg)
        {
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

                _taskCts!.Cancel(); // This should not be null if a task was started, if it is null just panic since something has already gone wrong.
                _state = JobState.Inactive; // Set _state to Inactive in case the task was canceled before starting.
            }
        }

        private Task StartNew(TArg arg)
        {
            // Try to reuse the cts or create a new one.
            if (_taskCts == null || !_taskCts.TryReset())
            {
                _taskCts?.Dispose();
                _taskCts = new CancellationTokenSource();
            }

            CancellationToken token = _taskCts.Token;
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
