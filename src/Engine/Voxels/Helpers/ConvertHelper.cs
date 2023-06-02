using OpenTK.Mathematics;
using System;
using VoxelGame.Framework.Helpers;

namespace VoxelGame.Engine.Voxels.Helpers
{
    public static class ConvertHelper
    {
        /// <summary>Returns the index of the chunk the given position is in.</summary>
        public static Vector3i PosToChunkIndex(Vector3 pos)
        {
            int x = (int)MathF.Floor(pos.X * 0.0625f);
            int y = (int)MathF.Floor(pos.Y * 0.0625f);
            int z = (int)MathF.Floor(pos.Z * 0.0625f);
            return new Vector3i(x, y, z);
        }

        /// <summary>
        /// Converts an absolute location in the world to chunk and block indexes.
        /// </summary>
        /// <returns>Chunk index and block index in that order.</returns>
        public static (Vector3i, Vector3i) PosToChunkBlockIndex(Vector3 pos)
        {
            // Calculate block index in chunk.
            int x = (int)MathH.Mod(pos.X, 16f);
            int y = (int)MathH.Mod(pos.Y, 16f);
            int z = (int)MathH.Mod(pos.Z, 16f);
            return (PosToChunkIndex(pos), new Vector3i(x, y, z));
        }

        /// <summary>
        /// Returns a <see cref="Vector3i"/> pointing in the direction specified.
        /// </summary>
        /// <param name="direction">Direction integer (See implementation in <see cref="Minecraft.Engine.Voxels.ChunkBuilder"/>)</param>
        /// <exception cref="Exception"/>
        public static Vector3i DirToVector(uint direction) => direction switch
        {
            0 => Vector3i.UnitZ,
            1 => Vector3i.UnitX,
            2 => -Vector3i.UnitZ,
            3 => -Vector3i.UnitX,
            4 => Vector3i.UnitY,
            5 => -Vector3i.UnitY,
            _ => Vector3i.Zero
        };
    }
}
