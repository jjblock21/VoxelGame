namespace VoxelGame.Framework.Threading
{
    public enum TaskState
    {
        /// <summary>
        /// The task is running or has been dispatched.
        /// </summary>
        Inert,
        /// <summary>
        /// The task was dispatched but hasn't started yet.
        /// </summary>
        Dispatched,
        /// <summary>
        /// The task is running.
        /// </summary>
        Running
    }

    /// <summary>
    /// Wraps <see cref="TaskState"/> in a class so it can be passed to lambda expressions by reference.
    /// </summary>
    public class TaskStateWrapper
    {
        public volatile TaskState Value;

        public TaskStateWrapper()
        {
            Value = TaskState.Inert;
        }
    }
}
