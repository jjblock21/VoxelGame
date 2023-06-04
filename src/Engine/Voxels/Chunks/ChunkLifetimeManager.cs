using OpenTK.Mathematics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VoxelGame.Framework;
using VoxelGame.Framework.Helpers;
using VoxelGame.Game;

namespace VoxelGame.Engine.Voxels.Chunks
{
    // Bad name, but didn't know what to call it.
    public class ChunkLifetimeManager : IDisposable
    {
        // My though process:
        /* Start two tasks:
         * 1: Loop over the square around the center chunk and generate chunks within a cylinder (r = render distance, h = half render distance)
         * which are not generated yet.
         * 2: Loop over all chunk currently in the world and delete them if they around outside the cylinder.
         * 
         * Before starting new tasks we also probably need to check for running ones and stop them, 
         * or exit if other tasks have been dispatched but haven't started yet.
         * If the center chunk changes to often, tasks may be stopped and not generate/delete all chunks,
         * so there needs to be some kind of counter to count how often this happens and halt the renderer to wait for 
         * completion of the tasks if it happens to often. (! Still needs to be implemented)
         */
        //TODO: Comment this accordingly

        // Max amount of chunks that will be deleted synchronously each frame.
        private const int CHUNK_DELETEION_LIMIT = 4;

        // Are cancellation tokens necessary here?
        private CancellationTokenSource _createNewCancelSrc;
        private CancellationTokenSource _deleteOldCancelSrc;
        private volatile TaskState _createNewState = TaskState.Inert;
        private volatile TaskState _deleteOldState = TaskState.Inert;
        private ConcurrentQueue<Chunk> _chunksToDelete;

        private ChunkManager _chunkManager;

        public ChunkLifetimeManager(ChunkManager chunkManager)
        {
            _chunkManager = chunkManager;
            _createNewCancelSrc = new CancellationTokenSource();
            _deleteOldCancelSrc = new CancellationTokenSource();
            _chunksToDelete = new ConcurrentQueue<Chunk>();
        }

        public void MoveCenterChunk(Vector3i center)
        {
            CreateNewChunks(center);
            DeleteOldChunks(center);
        }

        // Note: These two methods are basically the same, maybe in the future I can make an abstraction for it.
        #region Start Methods
        private void CreateNewChunks(Vector3i center)
        {
            // If another task hasn't started yet, just exit.
            if (_createNewState == TaskState.Dispatched) return;
            // If another task is running cancel it.
            if (_createNewState == TaskState.Running)
                _createNewCancelSrc.Cancel();

            // Start the task and mark it as dispatched.
            _createNewState = TaskState.Dispatched;
            CancellationToken token = _createNewCancelSrc.Token;
            Task.Factory.StartNew(() =>
            {
                CreateNewChunksTask(center, token);
            }, token);
        }

        private void DeleteOldChunks(Vector3i center)
        {
            // If another task hasn't started yet, just exit.
            if (_deleteOldState == TaskState.Dispatched) return;
            // If another task is running cancel it.
            if (_deleteOldState == TaskState.Running)
                _deleteOldCancelSrc.Cancel();

            // Start the task and mark it as dispatched.
            _deleteOldState = TaskState.Dispatched;
            CancellationToken token = _deleteOldCancelSrc.Token;
            Task.Factory.StartNew(() =>
            {
                DeleteOldChunksTask(center, token);
            }, token);
        }
        #endregion

        private void CreateNewChunksTask(Vector3i center, CancellationToken token)
        {
            _createNewState = TaskState.Running;
            try
            {
                ForCubeWithSizeOfRenderDistance(vec =>
                {
                    token.ThrowIfCancellationRequested();
                    if (CheckCylinder(vec))
                        _chunkManager.Generator.GenChunk(center + vec);
                });
            }
            finally
            {
                _createNewState = TaskState.Inert;
            }
        }

        private void DeleteOldChunksTask(Vector3i center, CancellationToken token)
        {
            _deleteOldState = TaskState.Running;
            try
            {
                token.ThrowIfCancellationRequested();
                List<Chunk> toDelete = new List<Chunk>();

                // Note: This is really slow, so I'm using tasks so the renderer can continue while this is working.
                foreach (Chunk chunk in _chunkManager.Chunks.Values)
                {
                    token.ThrowIfCancellationRequested();
                    if (!CheckCylinder(chunk.Location - center))
                        toDelete.Add(chunk);
                }

                // We can already remove them from the world.
                foreach (Chunk chunk in toDelete)
                {
                    // I decided not to allow cancellation after here anymore.
                    if (!_chunkManager.Chunks.TryRemove(chunk.Location, out Chunk _))
                        McWindow.Logger.Warn("Removing chunk from collection failed, this is most likely bad");

                    _chunksToDelete.Enqueue(chunk);
                }
            }
            finally
            {
                _deleteOldState = TaskState.Inert;
            }
        }

        private void ForCubeWithSizeOfRenderDistance(Action<Vector3i> callback)
        {
            VectorUtility.Vec3For(-Session.RENDER_DIST, -Session.RENDER_DIST / 2, -Session.RENDER_DIST,
                Session.RENDER_DIST, Session.RENDER_DIST / 2, Session.RENDER_DIST, callback);
        }

        /// <summary>
        /// Checks if the position is inside a cylinder with radius = render distance centered around 0,0,0.
        /// </summary>
        private bool CheckCylinder(Vector3i vec)
        {
            if (vec.Y < (-Session.RENDER_DIST / 2) || vec.Y > (Session.RENDER_DIST / 2)) return false;
            return (vec.X * vec.X + vec.Z * vec.Z) < (Session.RENDER_DIST * Session.RENDER_DIST);
        }

        public void Update()
        {
            for (int i = 0; i < CHUNK_DELETEION_LIMIT; i++)
            {
                if (!_chunksToDelete.TryDequeue(out Chunk? chunk)) return;
                chunk.Free();
            }
        }

        public void Dispose()
        {
            _createNewCancelSrc.Dispose();
            _deleteOldCancelSrc.Dispose();
        }
    }
}
