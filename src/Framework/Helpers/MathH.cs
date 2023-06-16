using OpenTK.Mathematics;
using System.Runtime.CompilerServices;

using static VoxelGame.Framework.Helpers.MethodImplConstants;

namespace VoxelGame.Framework.Helpers
{
    /// <summary>Math Helper</summary>
    public static class MathH
    {
        public static float Mod(float x, float m)
        {
            float r = x % m;
            return r < 0f ? r + m : r;
        }

        public static float Clamp01(float value) => MathHelper.Clamp(value, 0, 1);

        [MethodImpl(INLINE)]
        public static float ToRad(float degrees) => MathHelper.DegreesToRadians(degrees);

        [MethodImpl(INLINE)]
        public static float ToDeg(float radians) => MathHelper.RadiansToDegrees(radians);
    }
}
