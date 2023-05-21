using OpenTK.Graphics.OpenGL4;
using VoxelGame.Framework;

namespace VoxelGame.Engine.Rendering
{
    public struct VertexArrayObject : IHasDriverResources
    {
        public readonly int Handle;

        private int _index;
        private VertexAttribType _type;

        public VertexArrayObject(VertexAttribType type)
        {
            Handle = GL.GenVertexArray();
            _type = type;
            _index = 0;
        }

        /// <summary>
        /// Add a new vertex attribute and assign it to a vertex buffer binding point.
        /// </summary>
        /// <param name="bindingIndex">Binding point for the vertex buffer storing the value.</param>
        /// <param name="size">Number of values per vertex.</param>
        /// <param name="offset">Offset into the data of a vertex.</param>
        public void AddVertexAttrib(int bindingIndex, int size, int offset)
        {
            GL.BindVertexArray(Handle);
            GL.EnableVertexAttribArray(_index);
            GL.VertexAttribFormat(_index, size, _type, false, offset);
            GL.VertexAttribBinding(_index, bindingIndex);
            _index++;
        }

        public void Free() => GL.DeleteVertexArray(Handle);
    }
}
