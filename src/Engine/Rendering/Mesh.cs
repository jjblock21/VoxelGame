using OpenTK.Graphics.OpenGL4;
using VoxelGame.Engine.Rendering.Buffers;
using VoxelGame.Framework;

namespace VoxelGame.Engine.Rendering
{
    /// <summary>(!) All functions need to be called inside a GL context.</summary>
    public class Mesh : IHasDriverResources
    {
        private VertexBuffer<float> _vertexBuffer;
        private ElementBuffer<uint> _indexBuffer;
        private BufferUsageHint _usageHint;

        /// <param name="vertexSize">Number of consecutive values making up a single vertex.</param>
        /// <param name="usageHint">Buffer usage hint for OpenGL.</param>
        public Mesh(int vertexSize, BufferUsageHint usageHint)
        {
            _usageHint = usageHint;
            _vertexBuffer = new VertexBuffer<float>(vertexSize);
            _indexBuffer = new ElementBuffer<uint>();
            NumVertices = 0;
            NumIndices = 0;
        }

        public int NumIndices { get; private set; }
        public int NumVertices { get; private set; }

        /// <summary>
        /// Puts data into the vertex and index buffers.<br/>
        /// (Buffers are reallocated, Buffers stay bound after)
        /// </summary>
        /// <param name="vertices">Array of vertex parameters.</param>
        /// <param name="indices">Array of indices.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void SetData(float[] vertices, uint[] indices)
        {
            _vertexBuffer.BufferData(vertices, _usageHint);
            _indexBuffer.BufferData(indices, _usageHint);
            NumVertices = vertices.Length;
            NumIndices = indices.Length;
        }

        /// <summary>
        /// Binds the vertex buffer, index buffer and vertex array of the mesh.
        /// </summary>
        /// <param name="bindingIndex">Slot to bind the vertex buffer to.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void BindBuffers(int bindingIndex)
        {
            // Bind the vertex buffer to the vertex array
            _vertexBuffer.BindRender(bindingIndex);
            _indexBuffer.Bind();
        }

        /// <summary>
        /// Deletes all buffers used by the mesh (Buffers need to be unbound manually beforehand)
        /// </summary>
        public void Free()
        {
            _vertexBuffer.Free();
            _indexBuffer.Free();
        }
    }
}
