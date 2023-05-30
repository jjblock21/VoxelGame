using OpenTK.Mathematics;
using VoxelGame.Framework.Helpers;
using VoxelGame.Game.Blocks;

namespace VoxelGame.Engine.Voxels.Chunks.ChunkGen
{
    public class ChunkGenerator
    {
        private static int Seed = 69420;
        private static byte[] Perm1 = SimplexNoise.GenPermTable(Seed);
        private static byte[] Perm2 = SimplexNoise.GenPermTable(Seed + 1);

        private SimplexNoise _simplexNoise;

        public ChunkGenerator()
        {
            // Should be fine using the same byte array from multiple threads since its atomic
            // and isn't modified by the noise function.
            _simplexNoise = new SimplexNoise(Perm1);
            _simplexNoise.Scale = 0.035f;
        }

        public Chunk Generate(Chunk chunk)
        {
            BlockType[,,] blocks = new BlockType[16, 16, 16];

            _simplexNoise.SetPermTable(Perm1);
            VectorUtility.Vec3For(0, 0, 0, 16, 16, 16, (i) =>
            {
                Vector3i pos = i + chunk.Location * 16;
                float val = _simplexNoise.Sample3D(pos.X, pos.Y, pos.Z);
                blocks[i.X, i.Y, i.Z] = val < 0.4 ? BlockType.Stone : BlockType.Air;
            });

            _simplexNoise.SetPermTable(Perm2);
            VectorUtility.Vec3For(0, 0, 0, 16, 16, 16, (i) =>
            {
                if (blocks[i.X, i.Y, i.Z] != BlockType.Stone) return;

                Vector3i pos = i + chunk.Location * 16;
                float val = _simplexNoise.Sample3D(pos.X, pos.Y, pos.Z);
                if (val < 0.35) blocks[i.X, i.Y, i.Z] = BlockType.Earth;
            });

            // Place debug block at the origin of the chunk.
            blocks[0, 0, 0] = BlockType.Debug;

            // Not locking this, just need to make sure to not access it while the build stage is set to Pending.
            chunk.Blocks = blocks;

            return chunk;
        }
    }
}
