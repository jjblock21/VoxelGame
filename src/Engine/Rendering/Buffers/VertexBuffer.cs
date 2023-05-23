using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace VoxelGame.Engine.Rendering.Buffers
{
    public struct VertexBuffer<T> : IRenderBuffer<T> where T : struct
    {
        /// <summary>
        /// Handle of the underlying OpenGL buffer object.
        /// </summary>
        public readonly int Handle;

        private int _bufferStride;
        private int _valueSize;

        ///// <param name="valueSize">Size in bytes of a single object of type <see cref="T"/>.</param>
        /// <param name="vertexSize">Number of consecutive values making up a single vertex.</param>
        public VertexBuffer(int vertexSize)
        {
            _valueSize = Marshal.SizeOf(default(T));
            _bufferStride = _valueSize * vertexSize;
            Handle = GL.GenBuffer();
        }

        /// <summary>
        /// Reallocates the buffers data store and moves data into it.
        /// </summary>
        /// <param name="usageHint">The expected usage pattern of the buffers data store.</param>
        public void BufferData(T[] data, BufferUsageHint usageHint)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * _valueSize, data, usageHint);
        }

        public void Bind() => GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);

        /// <summary>
        /// Binds the vertex buffer for rendering using <see cref="GL.BindVertexBuffer"/>.
        /// </summary>
        /// <param name="bindingPoint">Index of the vertex buffer binding point to bind the buffer to.</param>
        public void BindRender(int bindingPoint)
        {
            GL.BindVertexBuffer(bindingPoint, Handle, IntPtr.Zero, _bufferStride);
        }

        /// <summary>
        /// Deltes the underlying buffer object.
        /// </summary>
        public void Free() => GL.DeleteBuffer(Handle);
    }
}
