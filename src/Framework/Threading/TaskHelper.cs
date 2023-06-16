using System;
using System.Threading.Tasks;

using CancelToken = System.Threading.CancellationToken;
using CancelTokenSrc = System.Threading.CancellationTokenSource;

namespace VoxelGame.Framework.Threading
{
    public static class TaskHelper
    {
        /// <summary>
        /// Creates and starts a new task which calls <paramref name="action"/>
        /// and cancels already running tasks executing <paramref name="action"/>.
        /// </summary>
        public static void StartNewCancelRunning(Action<CancelToken> action, TaskStateWrapper state, CancelTokenSrc cancelSource)
        {
            // Check if we should cancel the already running task and do so.
            if (state.Value == TaskState.Running)
                cancelSource.Cancel();

            // If a task will start soon, exit.
            else if (state.Value == TaskState.Dispatched) return;

            state.Value = TaskState.Dispatched;
            CancelToken token = cancelSource.Token;

            Task.Factory.StartNew(() =>
            {
                state.Value = TaskState.Running;
                try
                {
                    action(token);
                }
                catch { } // Ignore any exceptions.
                finally
                {
                    state.Value = TaskState.Inert;
                }

            }, token);
        }

        /// <summary>
        /// Creates and starts a new task to run <paramref name="action"/>
        /// if no other tasks executing <paramref name="action"/> are running.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="state"></param>
        public static void StartNewIfNoneRunning(Action action, TaskStateWrapper state)
        {
            // If a task will start soon or is running, exit.
            if (state.Value != TaskState.Inert) return;
            state.Value = TaskState.Dispatched;

            Task.Factory.StartNew(() =>
            {
                state.Value = TaskState.Running;
                try
                {
                    action();
                }
                catch { } // Ignore any exceptions.
                finally
                {
                    state.Value = TaskState.Inert;
                }

            });
        }
    }
}
