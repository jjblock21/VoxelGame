using OpenTK.Mathematics;
using System.Collections.Concurrent;
using VoxelGame.Engine.Voxels.Chunks.MeshGen;
using VoxelGame.Engine.Voxels.Chunks;
using VoxelGame.Framework;
using VoxelGame.Framework.Helpers;

namespace VoxelGame.Engine.Voxels
{
    public class ChunkManager : IHasDriverResources
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

        public void Free()
        {
            foreach (Chunk chunk in Chunks.Values)
                chunk.Free();
        }

        public void Update()
        {
            Builder.Update();
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
