using OpenTK.Mathematics;

namespace VoxelGame.Framework.Helpers
{
    public static class Matrix4Extension
    {
        /// <summary>Returns the unnormalized forward vector of the matrix.</summary>
        public static Vector3 GetForwardRaw(this Matrix4 matrix) => matrix.Row2.Xyz;

        /// <summary>Returns the unnormalized right vector of the matrix.</summary>
        public static Vector3 GetRightRaw(this Matrix4 matrix) => matrix.Row0.Xyz;

        /// <summary> Returns the unnormalized up vector of the matrix.</summary>
        public static Vector3 GetUpRaw(this Matrix4 matrix) => matrix.Row1.Xyz;
    }
}
