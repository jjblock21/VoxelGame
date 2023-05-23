using OpenTK.Graphics.OpenGL4;
using VoxelGame.Framework;

namespace VoxelGame.Engine.Rendering.Buffers
{
    public interface IRenderBuffer<T> : IHasDriverResources where T : struct
    {
        public void BufferData(T[] data, BufferUsageHint usageHint);
        public void Bind();
    }
}
