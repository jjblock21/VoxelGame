using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Threading;
using VoxelGame.Framework.Helpers;
using VoxelGame.Framework.Threading;
using VoxelGame.Game;
using VoxelGame.Game.Level;

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
        private const float DELETE_CHUNK_COMPUTE_COST = 0.2f;

        // Are cancellation tokens necessary here?
        private CancellationTokenSource _createNewCancelSrc;
        private CancellationTokenSource _deleteOldCancelSrc;
        private TaskStateWrapper _createNewState;
        private TaskStateWrapper _deleteOldState;

        private ChunkManager _chunkManager;

        public ChunkLifetimeManager(ChunkManager chunkManager)
        {
            _chunkManager = chunkManager;

            _createNewCancelSrc = new CancellationTokenSource();
            _deleteOldCancelSrc = new CancellationTokenSource();
            _createNewState = new TaskStateWrapper();
            _deleteOldState = new TaskStateWrapper();
        }

        public void MoveCenterChunk(Vector3i center)
        {
            TaskHelper.StartNewCancelRunning(token => CreateNewChunksTask(center, token),
                _createNewState, _createNewCancelSrc);

            TaskHelper.StartNewCancelRunning(token => DeleteOldChunksTask(center, token),
                _deleteOldState, _deleteOldCancelSrc);
        }

        private void CreateNewChunksTask(Vector3i center, CancellationToken token)
        {
            ForCubeWithSizeOfRenderDistance(vec =>
            {
                token.ThrowIfCancellationRequested();
                if (CheckCylinder(vec))
                    _chunkManager.Generator.GenChunk(center + vec);
            });
        }

        private void DeleteOldChunksTask(Vector3i center, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            // Note: This is really slow, so I'm using tasks so the renderer can continue while this is working.
            List<Chunk> toDelete = new List<Chunk>();

            // Loop trough all chunks in the world and find the ones outside of render distance.
            foreach (Chunk chunk in _chunkManager.Chunks.Values)
            {
                token.ThrowIfCancellationRequested();
                if (!CheckCylinder(chunk.Location - center))
                    toDelete.Add(chunk);
            }

            // Delete those chunks.
            foreach (Chunk chunk in toDelete)
            {
                token.ThrowIfCancellationRequested();

                // Remove the chunk from the dictionary asynchronously.
                if (!_chunkManager.Chunks.TryRemove(chunk.Location, out Chunk _))
                    McWindow.Logger.Warn("Removing chunk from collection failed, this is most likely bad");

                // Schedule a call on the main thread to chunk.Free() to delete OpenGL buffers.
                RenderThreadCallback.Schedule(RenderThreadCallback.Priority.DeleteMesh,
                    DELETE_CHUNK_COMPUTE_COST, chunk.Free);
            }
        }

        private void ForCubeWithSizeOfRenderDistance(Action<Vector3i> callback)
        {
            const int radius = World.RENDER_DIST;
            VectorUtility.Vec3For(-radius, -radius / 2, -radius, radius, radius / 2, radius, callback);
        }

        /// <summary>
        /// Checks if the position is inside a cylinder with radius = render distance centered around 0,0,0.
        /// </summary>
        private bool CheckCylinder(Vector3i vec)
        {
            if (vec.Y < (-World.RENDER_DIST / 2) || vec.Y > (World.RENDER_DIST / 2)) return false;
            return (vec.X * vec.X + vec.Z * vec.Z) < (World.RENDER_DIST * World.RENDER_DIST);
        }

        public void Dispose()
        {
            _createNewCancelSrc.Dispose();
            _deleteOldCancelSrc.Dispose();
        }
    }
}
