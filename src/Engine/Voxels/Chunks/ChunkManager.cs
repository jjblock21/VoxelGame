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

        public readonly ChunkBuilderProvider Builder;
        public readonly ChunkGeneratorProvider Generator;
        public readonly ChunkLifetimeManager LifetimeManager;

        public ChunkManager()
        {
            Chunks = new ConcurrentDictionary<Vector3i, Chunk>();

            Builder = new ChunkBuilderProvider();
            Generator = new ChunkGeneratorProvider(this);
            LifetimeManager = new ChunkLifetimeManager(this);
        }

        public void ClearChunks()
        {
            foreach (Chunk chunk in Chunks.Values)
                chunk.Free();
            Chunks.Clear();
        }

        /// <returns><see langword="null"/> if the chunk is not loaded.</returns>
        public Chunk? GetChunk(Vector3i location)
        {
            return Chunks.TryGetValue(location, out Chunk? c) ? c : null;
        }

        /// <summary>
        /// Schedules a chunk affected by a block modification for being rebuilt.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void RebuildModifiedChunks(Chunk chunk, Vector3i location, int x, int y, int z)
        {
            Builder.BuildChunk(chunk, dontDefer: true);

            // If the block is on the border to other chunks, check if they are affected and rebuild them as well.
            if (z >= 15) RebuildNeighbourBlock(location + ConvertH.DirToVector(0), x, y, 0);
            else if (z <= 0) RebuildNeighbourBlock(location + ConvertH.DirToVector(2), x, y, 15);
            if (x >= 15) RebuildNeighbourBlock(location + ConvertH.DirToVector(1), 0, y, z);
            else if (x <= 0) RebuildNeighbourBlock(location + ConvertH.DirToVector(3), 15, y, z);
            if (y >= 15) RebuildNeighbourBlock(location + ConvertH.DirToVector(4), x, 0, z);
            else if (y <= 0) RebuildNeighbourBlock(location + ConvertH.DirToVector(5), x, 15, z);
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
                    Builder.BuildChunk(chunk, dontDefer: true);
            }
        }
    }
}
