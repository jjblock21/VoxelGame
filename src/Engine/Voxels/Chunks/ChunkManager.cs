using OpenTK.Mathematics;
using System.Collections.Concurrent;
using VoxelGame.Engine.Voxels.Chunks.MeshGen;
using VoxelGame.Framework.Helpers;
using System;
using VoxelGame.Engine.Voxels.Chunks.ChunkGen;
using System.Runtime.CompilerServices;
using VoxelGame.Game.Blocks;
using VoxelGame.Game;
using VoxelGame.Engine.Voxels.Block;
using VoxelGame.Engine.Voxels.Helpers;

namespace VoxelGame.Engine.Voxels.Chunks
{
    public class ChunkManager
    {
        public readonly ConcurrentDictionary<Vector3i, Chunk> Chunks;

        public readonly ChunkBuilderProvider Builder;
        public readonly ChunkGeneratorProvider Generator;

        public ChunkManager()
        {
            Chunks = new ConcurrentDictionary<Vector3i, Chunk>();

            Builder = new ChunkBuilderProvider();
            Generator = new ChunkGeneratorProvider(this);
        }

        public void ClearChunks()
        {
            foreach (Chunk chunk in Chunks.Values)
                chunk.Free();
            Chunks.Clear();
        }

        public void Update()
        {
            Builder.Update();
        }

        /// <returns><see langword="null"/> if the chunk is not loaded.</returns>
        public Chunk? GetChunk(Vector3i location)
        {
            return Chunks.TryGetValue(location, out Chunk? c) ? c : null;
        }

        /// <summary>
        /// Moves the chunk marking the center of the currently loaded region.
        /// </summary>
        public void MoveCenterChunk(Vector3 location)
        {
            /* Start two tasks:
             * 1: Loop over the square around the center chunk and generate chunks within a cylinder (r = render distance, h = half render distance)
             * which are not generated yet.
             * 2: Loop over all chunk currently in the world and delete them if they around outside the cylinder.
             * 
             * Before starting new tasks we also probably need to check for running ones and stop them, 
             * or exit if other tasks have been dispatched but haven't started yet.
             * If the center chunk changes to often, tasks may be stopped and not generate/delte all chunks,
             * so there needs to be some kind of counter to count how often this happens and halt the renderer to wait for 
             * completion of the tasks if it happens to often.
             */

        }

        private void GenNewChunksTask()
        {

        }

        private void DelOldChunksTask()
        {

        }

        /// <summary>
        /// Schedules a chunk affected by a block modification for being rebuilt.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void RebuildModifiedChunks(Chunk chunk, Vector3i location, int x, int y, int z)
        {
            Builder.BuildChunk(chunk, dontDefer: true);

            // If the block is on the border to other chunks, check if they are affected and rebuild them aswell.
            if (z >= 15) RebuildNeighbourBlock(location + ConvertHelper.DirToVector(0), x, y, 0);
            if (z <= 0) RebuildNeighbourBlock(location + ConvertHelper.DirToVector(2), x, y, 15);
            if (x >= 15) RebuildNeighbourBlock(location + ConvertHelper.DirToVector(1), 0, y, z);
            if (x <= 0) RebuildNeighbourBlock(location + ConvertHelper.DirToVector(3), 15, y, z);
            if (y >= 15) RebuildNeighbourBlock(location + ConvertHelper.DirToVector(4), x, 0, z);
            if (y <= 0) RebuildNeighbourBlock(location + ConvertHelper.DirToVector(5), x, 15, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void RebuildNeighbourBlock(Vector3i location, int x, int y, int z)
        {
            if (Chunks.TryGetValue(location, out Chunk? chunk) && chunk.GenStage != Chunk.GenStageEnum.NoData)
            {
                // Dont rebuild the chunk if the block next to the affected block is an air block.
                if (chunk.Blocks![x, y, z] == BlockType.Air) return;

                // Rebuild the chunk if the block next to the affected block culls against it.
                BlockEntry data = Minecraft.Instance.BlockRegistry[chunk.Blocks[x, y, z]];
                if ((data.Params & BlockParams.DontCull) == 0)
                    Builder.BuildChunk(chunk, dontDefer: true);
            }
        }
    }
}
