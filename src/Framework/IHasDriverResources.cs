namespace VoxelGame.Framework
{
    /// <summary>
    /// Provides <see cref="Free"/> method (needs to be called from the OpenGL context thread) to delete OpenGL resources.
    /// </summary>
    public interface IHasDriverResources
    {
        public void Free();
    }
}
