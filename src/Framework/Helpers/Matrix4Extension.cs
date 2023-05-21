using OpenTK.Mathematics;
using System.Runtime.CompilerServices;

using static VoxelGame.Framework.Helpers.MethodImplConstants;

namespace VoxelGame.Framework.Helpers
{
    public static class Matrix4Extension
    {
        /// <summary>
        /// Returns the unnormalized forward vector of the matrix.
        /// </summary>
        public static Vector3 GetForwardRaw(this Matrix4 matrix) => matrix.Row2.Xyz;

        /// <summary>
        /// Returns the unnormalized right vector of the matrix.
        /// </summary>
        public static Vector3 GetRightRaw(this Matrix4 matrix) => matrix.Row0.Xyz;

        /// <summary>
        /// Returns the unnormalized up vector of the matrix.
        /// </summary>
        public static Vector3 GetUpRaw(this Matrix4 matrix) => matrix.Row1.Xyz;

        /// <summary>
        /// Returns the approximately normalized right vector of the matrix. (faster than default normalization)
        /// </summary>
        public static Vector3 GetRightApprox(this Matrix4 matrix)
        {
            Vector3 vec = matrix.GetRightRaw();
            vec.NormalizeFast();
            return vec;
        }

        /// <summary>
        /// Returns the approximately normalized up vector of the matrix. (faster than default normalization)
        /// </summary>
        public static Vector3 GetUpApprox(this Matrix4 matrix)
        {
            Vector3 vec = matrix.GetUpRaw();
            vec.NormalizeFast();
            return vec;
        }

        /// <summary>
        /// Returns the approximately normalized forward vector of the matrix. (faster than default normalization)
        /// </summary>
        public static Vector3 GetForwardApprox(this Matrix4 matrix)
        {
            Vector3 vec = matrix.GetForwardRaw();
            vec.NormalizeFast();
            return vec;
        }
    }
}
