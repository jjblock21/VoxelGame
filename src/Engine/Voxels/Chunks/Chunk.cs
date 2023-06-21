using OpenTK.Mathematics;
using VoxelGame.Engine.Rendering;
using VoxelGame.Framework;
using VoxelGame.Framework.Jobs;
using VoxelGame.Framework.Threading;
using VoxelGame.Game.Blocks;
using CancelTokenSrc = System.Threading.CancellationTokenSource;

namespace VoxelGame.Engine.Voxels.Chunks
{
    public class Chunk : IHasDriverResources, System.IDisposable
    {
        // Important: The current threading code assumes reads and writes to these values to be atomic.
        public volatile BlockType[,,]? Blocks;
        public volatile Mesh? Mesh;

        public volatile GenStageEnum GenStage; // Note: I don't actually think this needs to be volatile.

        public readonly RecurringTask<Void> BuildJob;

        public readonly Vector3i Location; // Position is stored twice, here and in the worlds dictionary, for ease of use.
        public readonly Vector3i Offset; // Location to be passed to the shader.

        public Chunk(Vector3i location, ChunkManager chunkManager)
        {
            Location = location;
            Offset = location * 16;
            GenStage = GenStageEnum.NoData;

            BuildJob = new RecurringTask<Void>((token, _) => chunkManager.Builder.BuildTask(this, token));
        }

        /// <summary>
        /// Frees all resources managed by the object.
        /// (<see cref="GLHelper.UnbindMeshBuffers"/> needs to be called before)<br/>
        /// Needs to be called from the opengl context thread.
        /// </summary>
        public void Free()
        {
            Mesh?.Free();
        }

        public void Dispose()
        {
            GenStage = GenStageEnum.Disposed;
            BuildJob.Dispose();
            Blocks = null; // Just in case
        }

        public enum GenStageEnum
        {
            /// <summary>
            /// The chunk is still waiting to receive its data.
            /// </summary>
            NoData = 0,
            /// <summary>
            /// The chunk has its data and is waiting to have its mesh built.
            /// </summary>
            HasData = 1,
            /// <summary>
            /// The chunk has its data and mesh and is ready for rendering.
            /// </summary>
            HasMesh = 2,
            /// <summary>
            /// The chunks resources have been disposed of.<br/>
            /// (Has the value of <see cref="NoData"/> for ease of making it work with already existing code)
            /// </summary>
            Disposed = 0
        }
    }
}
