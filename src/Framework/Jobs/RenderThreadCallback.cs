using System;
using System.Collections.Concurrent;
using System.Linq;
using VoxelGame.Framework.Helpers;

namespace VoxelGame.Framework.Threading
{
    /// <summary>
    /// Implements functionality to schedule callbacks from tasks to execute on the render thread with a primitive implementation of priority.
    /// </summary>
    public static class RenderThreadCallback
    {
        // Array of concurrent queues to allow the user to specify a priority.
        private static ConcurrentQueue<(float, Action)>[] _queues;
        private static float _computeCost;

        static RenderThreadCallback()
        {
            // Initialize the array of queues.
            _queues = new ConcurrentQueue<(float, Action)>[PRIORITY_ENUM_VALUE_COUNT];
            for (int i = 0; i < _queues.Length; i++)
                _queues[i] = new ConcurrentQueue<(float, Action)>();

            _computeCost = 0;
        }

        /// <summary>
        /// Schedules a callback to a function on the rendering thread when <see cref="Execute"/> is called.
        /// </summary>
        /// <param name="computeCost">
        /// Number from 0 to 1 specifying the computational cost of the callback.<br/>
        /// This is used to limit the amount of callbacks processed in a single frame to avoid high frame times.<br/>
        /// When calling into callbacks this number will be added to a variable,
        /// and if the variable reaches 1 no more callbacks will be processed and a the next frame is rendered.<br/>
        /// </param>
        public static void Schedule(Priority priority, float computeCost, Action callback)
        {
            _queues[(int)priority].Enqueue((MathH.Clamp01(computeCost), callback));
        }

        public static void Execute()
        {
            _computeCost = 0;

            // Loop through the queues with different priorities in descending order.
            for (int i = 0; i < _queues.Length; i++)
            {
                // Process callbacks from the current queue until it is empty.
                while (_queues[i].TryDequeue(out var callback))
                {
                    (float computeCost, Action func) = callback;
                    func();
                    _computeCost += computeCost;

                    // Stop processing callbacks if the variable reaches 1.
                    if (_computeCost >= 1) return;
                }
                // Repeat with the next, lower priority.
            }
        }

        public const int PRIORITY_ENUM_VALUE_COUNT = 4;

        // (!) DONT ASSIGN VALUS TO THESE.
        /// <summary>
        /// Lists the priorities of callbacks to the main thread in descending order from highest to lowest priority.<br/>
        /// The <see cref="RenderThreadCallback"/> code is build to allow for addition of priorities easily.
        /// </summary>
        public enum Priority
        {
            /// <summary>
            /// (Highest) Used for chunk rebuilds on the render thread.
            /// </summary>
            SyncChunkBuild,
            /// <summary>
            /// Used for uploading meshes to the GPU.
            /// </summary>
            UploadMesh,
            /// <summary>
            /// Used for deleting meshes on the GPU.
            /// </summary>
            DeleteMesh,

            /// <summary>
            /// (Lowest) Used for everything that isn't too important.
            /// </summary>
            Common
        }
    }
}
