using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using VoxelGame.Engine.Rendering;
using VoxelGame.Framework;
using VoxelGame.Game.Blocks;
using CancelTokenSrc = System.Threading.CancellationTokenSource;

namespace VoxelGame.Engine.Voxels.Chunks
{
    public class Chunk : IHasDriverResources
    {
        // Important: The current threading code assumes reads and writes to these values to be atomic.
        public volatile BlockType[,,]? Blocks;
        public volatile Mesh? Mesh;

        public volatile GenStageEnum GenStage; // Note: I dont actually think this needs to be volatile.
        public volatile AsyncBuildStage AsyncStage;

        public readonly Vector3i Location; // Position is stored twice, here and in the worlds dictionary, for ease of use.
        public readonly Vector3i Offset; // Location to be passed to the shader.

        public readonly CancelTokenSrc BuilderCancelSrc;

        public Chunk(Vector3i location)
        {
            Location = location;
            Offset = location * 16;
            GenStage = GenStageEnum.NoData;
            AsyncStage = AsyncBuildStage.None;
            BuilderCancelSrc = new CancelTokenSrc();
        }

        /// <summary>
        /// Frees all resources managed by the object.
        /// (<see cref="GLHelper.UnbindMeshBuffers"/> needs to be called before)<br/>
        /// Needs to be called from the opengl context thread.
        /// </summary>
        public void Free()
        {
            GenStage = GenStageEnum.Disposed;
            Mesh?.Free();
            BuilderCancelSrc.Dispose();
        }

        public enum GenStageEnum
        {
            /// <summary>
            /// The chunk is still waiting to recieve its data.
            /// </summary>
            NoData = 0,
            /// <summary>
            /// The chunk has its data and is waiting for the instuction to build its mesh.
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

        // TODO: Mark a still pending builder task, so that no new ones are dispatched if it hasn't started yet.
        public enum AsyncBuildStage
        {
            None,
            /// <summary>
            /// A task was disapatched to rebuild the chunk, but hasn't started yet.
            /// </summary>
            Dispatched,
            /// <summary>
            /// A task to rebuild the chunk is running.
            /// </summary>
            Running
        }
    }
}
