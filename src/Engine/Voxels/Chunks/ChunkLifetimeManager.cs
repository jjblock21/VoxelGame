using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using VoxelGame.Framework.Helpers;
using VoxelGame.Framework.Jobs;
using VoxelGame.Framework.Threading;
using VoxelGame.Game;
using VoxelGame.Game.Level;

namespace VoxelGame.Engine.Voxels.Chunks
{
    // Bad name, but didn't know what to call it.
    public class ChunkLifetimeManager
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

        private RecurringTask<Vector3i> _createNewTask;
        private RecurringTask<Vector3i> _deleteOldTask;

        private ChunkManager _chunkManager;

        public ChunkLifetimeManager(ChunkManager chunkManager)
        {
            _chunkManager = chunkManager;

            _createNewTask = new RecurringTask<Vector3i>((token, vec) => CreateNewTask(vec));
            _deleteOldTask = new RecurringTask<Vector3i>((token, vec) => DeleteOldTask(vec));
        }

        public void MoveCenterChunk(Vector3i center)
        {
            _createNewTask.StartIfPreviousCompleted(center);
            _deleteOldTask.StartIfPreviousCompleted(center);
        }

        private void CreateNewTask(Vector3i center)
        {
            const int radius = World.RENDER_DIST;
            // Loop over all chunks in a cube with size of render distance.
            VectorUtility.Vec3For(-radius, -radius / 2, -radius, radius, radius / 2, radius, vec =>
            {
                // Check if the chunk is withing the cylinder where chunks are rendered and generate it if it is.
                if (CheckCylinder(vec)) _chunkManager.Generator.GenChunk(center + vec);
            });
        }

        private void DeleteOldTask(Vector3i center)
        {
            // Note: This is really slow, so I'm using tasks so the renderer can continue while this is working.

            // Select all chunks outside of the cylinder with r = render distance and center = chunk the player is in.
            IEnumerable<Chunk> toDelete = _chunkManager.Chunks.Values.Where(chunk => !CheckCylinder(chunk.Location - center));

            foreach (Chunk chunk in toDelete)
            {
                // Perhaps this might need to be moved to a proper method.
                chunk.Dispose(); // This is the dispose method that can be called from anywhere.

                // Remove the chunk from the dictionary asynchronously.
                if (!_chunkManager.Chunks.TryRemove(chunk.Location, out Chunk _))
                    McWindow.Logger.Warn("Removing chunk from collection failed, this is most likely bad");

                // Schedule a call on the main thread to chunk.Free() to delete OpenGL buffers.
                RenderThreadCallback.Schedule(RenderThreadCallback.Priority.DeleteMesh,
                    DELETE_CHUNK_COMPUTE_COST, chunk.Free); // Free() is the dispose method that must be called from the render thread.
            }
        }

        /// <summary>
        /// Checks if the position is inside a cylinder with radius = render distance centered around 0,0,0.
        /// </summary>
        private bool CheckCylinder(Vector3i vec)
        {
            if (vec.Y < (-World.RENDER_DIST / 2) || vec.Y > (World.RENDER_DIST / 2)) return false;
            return (vec.X * vec.X + vec.Z * vec.Z) < (World.RENDER_DIST * World.RENDER_DIST);
        }
    }
}
