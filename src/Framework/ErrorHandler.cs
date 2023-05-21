using OpenTK.Graphics.OpenGL4;
using VoxelGame.Game;

namespace VoxelGame.Framework
{
    public static class ErrorHandler
    {
        private static string? _section;

        /// <summary>
        /// Begin a new section and log errors from the previous one.
        /// </summary>
        public static void Section(string name)
        {
            if (_section != null) CheckGLErrors();
            _section = name;
        }

        /// <summary>
        /// Check for OpenGL errors and log a message if there were any.
        /// </summary>
        public static void CheckGLErrors()
        {
            ErrorCode code = GL.GetError();
            if (code != ErrorCode.NoError)
            {
                McWindow.Logger.Warn($"OpenGL error in section '{_section}': ErrorCode.{code}");
            }
        }
    }
}