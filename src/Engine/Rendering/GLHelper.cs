using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace VoxelGame.Engine.Rendering
{
    public static class GLHelper
    {
        public const int DEFAULT_TEX_WRAP_MODE = 0x2901; // TextureWrapMode.Repeat
        public const int DEFAULT_TEX_FILTER = 0x2600; // Texture(Mag/Min)Filter.Nearest

        /// <summary>
        /// Applies settigns for texture wrap and filtering to a texture.
        /// </summary>
        public static void ApplyDefaultTexParams()
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, DEFAULT_TEX_WRAP_MODE);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, DEFAULT_TEX_WRAP_MODE);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, DEFAULT_TEX_FILTER);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, DEFAULT_TEX_FILTER);
        }

        /// <summary>
        /// Binds <see langword="null"/> to all buffers used for mesh rendering.
        /// </summary>
        public static void UnbindMeshBuffers()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Same as <see cref="GL.UniformMatrix4(int, bool, ref Matrix4)"/> but doesn't take the matrix as a ref parameter.
        /// </summary>
        /// <param name="fieldHandle"></param>
        /// <param name="matrix"></param>
        public static void UniformMatrix4(int fieldHandle, Matrix4 matrix)
        {
            Matrix4 mat = matrix;
            GL.UniformMatrix4(fieldHandle, false, ref mat);
        }
    }
}
