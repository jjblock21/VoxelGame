using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace VoxelGame.Engine.Rendering.Buffers
{
    public struct ElementBuffer<T> : IRenderBuffer<T> where T : struct
    {
        /// <summary>
        /// Handle of the underlying OpenGL buffer object.
        /// </summary>
        public readonly int Handle;

        private int _valueSize;

        // <param name="valueSize">Size in bytes of a single object of type <see cref="T"/>.</param>
        public ElementBuffer()
        {
            _valueSize = Marshal.SizeOf(default(T));
            Handle = GL.GenBuffer();
        }

        /// <summary>
        /// Reallocates the buffers data store and moves data into it.
        /// </summary>
        /// <param name="usageHint">The expected usage pattern of the buffers data store.</param>
        public void BufferData(T[] data, BufferUsageHint usageHint)
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Handle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, data.Length * _valueSize, data, usageHint);
        }

        public void Bind() => GL.BindBuffer(BufferTarget.ElementArrayBuffer, Handle);

        /// <summary>
        /// Deletes the underlying buffer object.
        /// </summary>
        public void Free() => GL.DeleteBuffer(Handle);
    }
}
