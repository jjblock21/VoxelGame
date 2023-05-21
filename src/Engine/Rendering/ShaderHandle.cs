using OpenTK.Graphics.OpenGL4;
using VoxelGame.Game;

namespace VoxelGame.Engine.Rendering
{
    /// <summary>
    /// Abstraction for shader program handle.
    /// </summary>
    public struct ShaderHandle
    {
        public readonly int Handle;
        private ShaderHandle(int handle) { Handle = handle; }

        /// <summary>
        /// Implicitly converts the ShaderHandle object into an integer.
        /// </summary>
        public static implicit operator int(ShaderHandle shader) => shader.Handle;

        #region Shader Compilation
        private const string VERT_DEFINE = "@program vertex";
        private const string FRAG_DEFINE = "@program fragment";

        /// <summary>
        /// Compiles the vertex and fragment shaders and links them together into a shader program.<br/>
        /// (!) Needs to be called inside a GL context.
        /// </summary>
        /// <param name="source">Combined vertex and fragment shader code.</param>
        /// <param name="logName">Name displayed in the error message if the compilation fails.</param>
        /// <returns>A ShaderPtr object containing the handle of the generated shader program.</returns>
        /// <exception cref="InvalidDataException">Throws when parsing the combined shader source fails.</exception>
        public static ShaderHandle CompileShader(string source, string logName)
        {
            void Throw() { throw new InvalidDataException("Shader is invalid."); }

            if (string.IsNullOrEmpty(source)) Throw();
            source = source.Trim();
            // Check for version declaration at start of file. (required)
            if (!source.StartsWith("#version")) Throw();

            // Parse combined shader into scource code for vertex and fragment shaders.
            int vertIndex = source.IndexOf(VERT_DEFINE);
            int fragIndex = source.IndexOf(FRAG_DEFINE);
            if (vertIndex == -1 || fragIndex == -1 || fragIndex <= vertIndex) Throw();
            int vertSrcLen = fragIndex - vertIndex - FRAG_DEFINE.Length + 2;

            string version = source.Substring(0, vertIndex);
            string vertSrc = version + source.Substring(vertIndex + VERT_DEFINE.Length, vertSrcLen);
            string fragSrc = version + source.Substring(fragIndex + FRAG_DEFINE.Length);

            // Compile shaders.
            int vertHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertHandle, vertSrc);
            GL.CompileShader(vertHandle);
            int fragHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragHandle, fragSrc);
            GL.CompileShader(fragHandle);

            return LinkShaderProgram(vertHandle, fragHandle, logName);
        }

        /// <summary>
        /// Links two compiled shaders into a shader program.<br/>
        /// (!) Needs to be called inside a GL context.
        /// </summary>
        /// <param name="vertHandle">Handle of the vertex shader.</param>
        /// <param name="fragHandle">Handle of the fragment shader.</param>
        /// <param name="logName">Name displayed in the error message if the compilation fails.</param>
        /// <returns>A ShaderPtr object containing the handle of the generated shader program.</returns>
        public static ShaderHandle LinkShaderProgram(int vertHandle, int fragHandle, string logName)
        {
            // Check if both programs compiled successfully.
            CheckCompileErrors(vertHandle, logName + " (Vertex)");
            CheckCompileErrors(fragHandle, logName + " (Fragment)");

            // Create shader program.
            int shaderHandle = GL.CreateProgram();
            GL.AttachShader(shaderHandle, vertHandle);
            GL.AttachShader(shaderHandle, fragHandle);
            GL.LinkProgram(shaderHandle);

            // Cleanup
            GL.DetachShader(shaderHandle, vertHandle);
            GL.DetachShader(shaderHandle, fragHandle);
            GL.DeleteShader(vertHandle);
            GL.DeleteShader(fragHandle);

            return new ShaderHandle(shaderHandle);
        }

        /// <summary>
        /// Checks if a shader compiled successfully.
        /// </summary>
        /// <param name="shader">Handle to the shader part (vertex/fragment) to check.</param>
        public static void CheckCompileErrors(int shader, string name)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int result);
            if (result == 0)
            {
                string log = GL.GetShaderInfoLog(shader);
                McWindow.Logger.Error($"Compilation of shader '{name}' failed:\n{log}");
                Minecraft.Instance.Window.Close();
            }
        }

        #endregion
    }
}
