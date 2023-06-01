using OpenTK.Mathematics;
using System.Collections.Concurrent;
using VoxelGame.Engine.Voxels.Chunks.MeshGen;
using VoxelGame.Engine.Voxels.Chunks;
using VoxelGame.Framework.Helpers;
using System;
using VoxelGame.Engine.Voxels.Chunks.ChunkGen;
using System.Runtime.CompilerServices;
using VoxelGame.Game.Blocks;
using VoxelGame.Game;

namespace VoxelGame.Engine.Voxels
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
        /// Schedules a chunk affected by a block modification for being rebuilt.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void RebuildModifiedChunks(Chunk chunk, Vector3i location, int x, int y, int z)
        {
            Builder.BuildChunk(chunk, dontDefer: true);

            // If the block is on the border to other chunks, check if they are affected and rebuild them aswell.
            if (z >= 15) RebuildNeighbourBlock(location + World.DirToVector(0), x, y, 0);
            if (z <= 0) RebuildNeighbourBlock(location + World.DirToVector(2), x, y, 15);
            if (x >= 15) RebuildNeighbourBlock(location + World.DirToVector(1), 0, y, z);
            if (x <= 0) RebuildNeighbourBlock(location + World.DirToVector(3), 15, y, z);
            if (y >= 15) RebuildNeighbourBlock(location + World.DirToVector(4), x, 0, z);
            if (y <= 0) RebuildNeighbourBlock(location + World.DirToVector(5), x, 15, z);
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

        #region Static

        /// <returns>The index of the chunk the given position is in.</returns>
        public static Vector3i GetChunkIndex(Vector3 pos)
        {
            int x = (int)MathF.Floor(pos.X * 0.0625f);
            int y = (int)MathF.Floor(pos.Y * 0.0625f);
            int z = (int)MathF.Floor(pos.Z * 0.0625f);
            return new Vector3i(x, y, z);
        }

        /// <summary>
        /// Converts an absolute location in the world to chunk and block indices.
        /// </summary>
        /// <returns>Chunk index and block index in that order.</returns>
        public static (Vector3i, Vector3i) GetChunkBlockIndex(Vector3 pos)
        {
            // Calculate block index in chunk.
            int bx = (int)MathH.Mod(pos.X, 16f);
            int by = (int)MathH.Mod(pos.Y, 16f);
            int bz = (int)MathH.Mod(pos.Z, 16f);
            Vector3i chunkIndex = GetChunkIndex(pos);
            return (chunkIndex, new Vector3i(bx, by, bz));
        }

        #endregion
    }
}
