using OpenTK.Graphics.OpenGL4;
using VoxelGame.Game;

namespace VoxelGame.Framework
{
    public static class ErrorHandler
    {
        public static string? CurrentSection { get; private set; }

        /// <summary>
        /// Begin a new section and log errors from the previous one.
        /// </summary>
        public static void Section(string name)
        {
            if (CurrentSection != null) CheckGLErrors();
            CurrentSection = name;
        }

        /// <summary>
        /// Check for OpenGL errors and log a message if there were any.
        /// </summary>
        public static void CheckGLErrors()
        {
            ErrorCode code = GL.GetError();
            if (code != ErrorCode.NoError)
            {
                McWindow.Logger.Warn($"OpenGL error in section '{CurrentSection}': ErrorCode.{code}");
            }
        }
    }
}