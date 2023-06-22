using OpenTK.Mathematics;
using System.Collections.Concurrent;
using VoxelGame.Engine.Voxels.Chunks.MeshGen;
using VoxelGame.Engine.Voxels.Chunks.ChunkGen;
using System.Runtime.CompilerServices;
using VoxelGame.Game.Blocks;
using VoxelGame.Game;
using VoxelGame.Engine.Voxels.Helpers;
using VoxelGame.Engine.Voxels.Blocks;

namespace VoxelGame.Engine.Voxels.Chunks
{
    public class ChunkManager
    {
        public readonly ConcurrentDictionary<Vector3i, Chunk> Chunks;

        public readonly ChunkBuilderManager Builder;
        public readonly ChunkGeneratorManager Generator;
        public readonly ChunkLifetimeManager LifetimeManager;

        //TODO: Implement greedy meshing for full blocks.
        //TODO: Eliminate arrays passed from block models to reduce GC allocations. (Done)
        //TODO: Calculate average chunk vertex and index list size while building chunks to reduce reallocations of the underlying buffer while adding items.

        //Note: The chunk system still has a lot of problems with race conditions if chunks are created and deleted very fast. Should be fine tho.

        public ChunkManager()
        {
            Chunks = new ConcurrentDictionary<Vector3i, Chunk>();

            Builder = new ChunkBuilderManager();
            Generator = new ChunkGeneratorManager(this);
            LifetimeManager = new ChunkLifetimeManager(this);
        }

        /* Note: There should also be a chunk deletion manager to rebuild surrounding chunks when a chunk is deleted,
         * but that shouldn't cause any big problems with SOLID_WORLD_BORDER disabled.
         */

        public void ClearChunks()
        {
            foreach (Chunk chunk in Chunks.Values) chunk.Free();
            Chunks.Clear();
        }

        public Chunk? GetChunk(Vector3i location)
        {
            return Chunks.TryGetValue(location, out Chunk? chunk) ? chunk : null;
        }

        /// <summary>
        /// Attempts to retrieve a block from the world.
        /// </summary>
        /// <param name="location">The location of the block in global coordinates.</param>
        /// <returns>
        /// <see langword="false"/> if the chunk containing the block is not loaded.
        /// </returns>
        public bool TryGetBlock(Vector3i location, out BlockType block)
        {
            (Vector3i chunkIndex, Vector3i blockIndex) = ConvertH.PosToChunkBlockIndex(location);
            if (Chunks.TryGetValue(chunkIndex, out Chunk? chunk) && chunk.GenStage != Chunk.GenStageEnum.NoData)
            {
                block = chunk!.Blocks![blockIndex.X, blockIndex.Y, blockIndex.Z];
                return true;
            }
            block = BlockType.Air;
            return false;
        }

        /// <summary>
        /// Attempts to place a block in the world.
        /// </summary>
        /// <returns>
        /// <see langword="false"/> if the chunk the block would be placed in is not loaded or if nothing was changed.
        /// </returns>
        public bool TrySetBlock(Vector3i location, BlockType type)
        {
            // Split absolute location into chunk & block indexes and get the affected chunk.
            (Vector3i chunkIndex, Vector3i blockIndex) = ConvertH.PosToChunkBlockIndex(location);
            if (Chunks.TryGetValue(chunkIndex, out Chunk? chunk) && chunk.GenStage != Chunk.GenStageEnum.NoData)
            {
                // If nothing will change exit.
                if (chunk.Blocks![blockIndex.X, blockIndex.Y, blockIndex.Z] == type) return false;
                chunk.Blocks[blockIndex.X, blockIndex.Y, blockIndex.Z] = type;
                RebuildModifiedChunks(chunk, chunkIndex, blockIndex.X, blockIndex.Y, blockIndex.Z);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Schedules a chunk affected by a block modification for being rebuilt.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void RebuildModifiedChunks(Chunk chunk, Vector3i location, int x, int y, int z)
        {
            Builder.BuildChunk(chunk, dontDefer: true);

            // If the block is on the border to other chunks, check if they are affected and rebuild them as well.

            #pragma warning disable format // For more readability
            if      (z >= 15) RebuildNeighbourBlock(location + ConvertH.DirToVector(0),  x,  y,  0);
            else if (z <= 0)  RebuildNeighbourBlock(location + ConvertH.DirToVector(2),  x,  y, 15);
            if      (x >= 15) RebuildNeighbourBlock(location + ConvertH.DirToVector(1),  0,  y,  z);
            else if (x <= 0)  RebuildNeighbourBlock(location + ConvertH.DirToVector(3), 15,  y,  z);
            if      (y >= 15) RebuildNeighbourBlock(location + ConvertH.DirToVector(4),  x,  0,  z);
            else if (y <= 0)  RebuildNeighbourBlock(location + ConvertH.DirToVector(5),  x, 15,  z);
            #pragma warning restore format
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void RebuildNeighbourBlock(Vector3i location, int x, int y, int z)
        {
            if (Chunks.TryGetValue(location, out Chunk? chunk) && chunk.GenStage != Chunk.GenStageEnum.NoData)
            {
                // Don't rebuild the chunk if the block next to the affected block is an air block.
                if (chunk.Blocks![x, y, z] == BlockType.Air) return;

                // Rebuild the chunk if the block next to the affected block culls against it.
                BlockEntry data = Minecraft.Instance.BlockRegistry[chunk.Blocks[x, y, z]];
                if ((data.Params & BlockParams.DontCull) == 0)
                {
                    Builder.BuildChunk(chunk, dontDefer: true);
                }
            }
        }
    }
}
