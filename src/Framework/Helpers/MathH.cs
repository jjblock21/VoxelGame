using OpenTK.Mathematics;
using System.Runtime.CompilerServices;

using static VoxelGame.Framework.Helpers.MethodImplConstants;

namespace VoxelGame.Framework.Helpers
{
    /// <summary>Math Helpher</summary>
    public static class MathH
    {
        public static float Mod(float x, float m)
        {
            float r = x % m;
            return r < 0f ? r + m : r;
        }

        [MethodImpl(INLINE)]
        public static float ToRad(float degrees)
        {
            return MathHelper.DegreesToRadians(degrees);
        }

        [MethodImpl(INLINE)]
        public static float ToDeg(float radians)
        {
            return MathHelper.RadiansToDegrees(radians);
        }
    }
}
