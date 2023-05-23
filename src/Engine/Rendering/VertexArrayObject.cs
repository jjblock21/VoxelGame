using OpenTK.Graphics.OpenGL4;
using VoxelGame.Framework;

namespace VoxelGame.Engine.Rendering
{
    public struct VertexArrayObject : IHasDriverResources
    {
        public readonly int Handle;
        private int _attribIndex;

        public VertexArrayObject()
        {
            Handle = GL.GenVertexArray();
            _attribIndex = 0;
        }

        #region Summary
        /// <summary>
        /// Add a new vertex attribute and assign it to a vertex buffer binding point. (For buffers containing floats)
        /// </summary>
        /// <param name="bindingIndex">Binding point for the vertex buffer storing the value.</param>
        /// <param name="size">Number of values per vertex.</param>
        /// <param name="offset">Offset into the data of a vertex.</param>
        #endregion
        public void AddVertexAttrib(int bindingIndex, int size, int offset, VertexAttribType type = VertexAttribType.Float)
        {
            GL.BindVertexArray(Handle);
            GL.EnableVertexAttribArray(_attribIndex);
            GL.VertexAttribFormat(_attribIndex, size, type, false, offset);
            GL.VertexAttribBinding(_attribIndex, bindingIndex);
            _attribIndex++;
        }

        #region Summary
        /// <summary>
        /// Add a new vertex attribute and assign it to a vertex buffer binding point. (For buffers containing ints)
        /// </summary>
        /// <param name="bindingIndex">Binding point for the vertex buffer storing the value.</param>
        /// <param name="size">Number of values per vertex.</param>
        /// <param name="offset">Offset into the data of a vertex.</param>
        #endregion
        public void AddVertexAttribI(int bindingIndex, int size, int offset, VertexAttribIntegerType type = VertexAttribIntegerType.Int)
        {
            GL.BindVertexArray(Handle);
            GL.EnableVertexAttribArray(_attribIndex);
            GL.VertexAttribIFormat(_attribIndex, size, type, offset);
            GL.VertexAttribBinding(_attribIndex, bindingIndex);
            _attribIndex++;
        }

        #region Summary
        /// <summary>
        /// Add a new vertex attribute and assign it to a vertex buffer binding point. (For buffers containing doubles)
        /// </summary>
        /// <param name="bindingIndex">Binding point for the vertex buffer storing the value.</param>
        /// <param name="size">Number of values per vertex.</param>
        /// <param name="offset">Offset into the data of a vertex.</param>
        #endregion
        public void AddVertexAttribL(int bindingIndex, int size, int offset)
        {
            GL.BindVertexArray(Handle);
            GL.EnableVertexAttribArray(_attribIndex);
            GL.VertexAttribLFormat(_attribIndex, size, VertexAttribDoubleType.Double, offset);
            GL.VertexAttribBinding(_attribIndex, bindingIndex);
            _attribIndex++;
        }

        public void Free() => GL.DeleteVertexArray(Handle);
    }
}
