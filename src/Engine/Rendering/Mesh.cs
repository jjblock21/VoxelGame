using OpenTK.Graphics.OpenGL4;
using VoxelGame.Framework;

namespace VoxelGame.Engine.Rendering
{
    /// <summary>(!) All functions need to be called inside a GL context.</summary>
    public class Mesh : IHasDriverResources
    {
        private BufferUsageHint _bufferUsage;
        private int _vertexBufHandle;
        private int _indexBufHandle;
        private int _bufferStride;

        /// <param name="bufferUsage">Buffer usage hint for OpenGL.</param>
        /// <param name="vertexStride">Vertex buffer stride by number of values.</param>
        public Mesh(int vertexStride, BufferUsageHint bufferUsage)
        {
            _bufferStride = vertexStride * sizeof(float);
            _bufferUsage = bufferUsage;
            NumVertices = 0;
            NumIndices = 0;
            _vertexBufHandle = GL.GenBuffer();
            _indexBufHandle = GL.GenBuffer();
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
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, _bufferUsage);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBufHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(float), indices, _bufferUsage);
            NumVertices = vertices.Length;
            NumIndices = indices.Length;
        }

        /// <summary>
        /// Binds the vertex buffer, index buffer and vertex array of the mesh.
        /// </summary>
        /// <param name="bindingSlot">Slot to bind the vertex buffer to.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void BindBuffers(int bindingSlot)
        {
            // Bind the vertex buffer to the vertex array
            GL.BindVertexBuffer(bindingSlot, _vertexBufHandle, IntPtr.Zero, _bufferStride);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBufHandle);
        }

        /// <summary>
        /// Deletes all buffers used by the mesh (Buffers need to be unbound manually beforehand)
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Free()
        {
            GL.DeleteBuffer(_vertexBufHandle);
            GL.DeleteBuffer(_indexBufHandle);
        }
    }
}
