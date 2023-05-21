using OpenTK.Mathematics;
using System.Runtime.CompilerServices;

namespace VoxelGame.Framework.Helpers
{
    public static class VectorUtility
    {
        public static void Vec3For(int startX, int startY, int startZ, int endX, int endY, int endZ, Action<Vector3i> action)
        {
            for (int x = startX; x < endX; x++)
                for (int y = startY; y < endY; y++)
                    for (int z = startZ; z < endZ; z++)
                        action(new Vector3i(x, y, z));
        }

        [MethodImpl(MethodImplConstants.INLINE)]
        public static Vector3i FloorVec3(Vector3 vec)
        {
            int x = (int)MathF.Floor(vec.X);
            int y = (int)MathF.Floor(vec.Y);
            int z = (int)MathF.Floor(vec.Z);
            return new Vector3i(x, y, z);
        }
    }
}
