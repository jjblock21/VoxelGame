using OpenTK.Graphics.OpenGL4;
using System.Threading;
using VoxelGame.Engine.Rendering;
using VoxelGame.Framework.Threading;

namespace VoxelGame.Engine.Voxels.Chunks.MeshGen
{
    public class ChunkBuilderManager
    {
        // Limit for how many chunks can be build on the main thread in a single frame.
        public const float SYNC_BUILD_COMPUTE_COST = 0.25f;
        public const float UPLOAD_COMPUTE_COST = 0.125f;

        // Collection of processors, one for each thread.
        private ThreadLocal<ChunkBuilder> _processor;

        // Collection of chunks that need to be built on the main thread.

        public ChunkBuilderManager()
        {
            _processor = new ThreadLocal<ChunkBuilder>(() => new ChunkBuilder());
        }

        /// <param name="dontDefer">
        /// If set to <see langword="true"/>, the chunk will be handed to the main thread to be processed synchronously,
        /// the renderer will wait for the chunk to finish building its mesh.<br/>
        /// If left on <see langword="false"/>, a task will be started to process the chunk asynchronously.
        /// </param>
        public void BuildChunk(Chunk chunk, bool dontDefer = false)
        {
            if (chunk.GenStage == Chunk.GenStageEnum.NoData) return;

            if (dontDefer)
            {
                // TODO: Make sure the same chunk doesn't get rebuilt multiple times.

                // Don't start a task, just hand it off to the main thread for processing there.
                RenderThreadCallback.Schedule(RenderThreadCallback.Priority.SyncChunkBuild,
                    SYNC_BUILD_COMPUTE_COST, () => BuildSync(chunk));
            }
            else
            {
                // You can also start the job directly but this provides slightly better syntax in some cases.
                chunk.BuildJob.StartCancelPrevious(default);
            }
        }

        public void BuildTask(Chunk chunk, CancellationToken token)
        {
            BuildResult result = _processor.Value!.Process(chunk, token);

            // Schedule a callback on the render thread, where the chunks data is uploaded.
            RenderThreadCallback.Schedule(RenderThreadCallback.Priority.UploadMesh,
                UPLOAD_COMPUTE_COST, () => UploadMesh(chunk, result));
        }

        private void BuildSync(Chunk chunk)
        {
            // Cancel running asynchronous tasks.
            chunk.BuildJob.CancelRunning();

            BuildResult result = _processor.Value!.Process(chunk, CancellationToken.None);
            UploadMesh(chunk, result);
        }

        private void UploadMesh(Chunk chunk, BuildResult result)
        {
            // Create the mesh if the chunk doesn't have one yet.
            chunk.Mesh ??= new Mesh(ChunkBuilder.BUFFER_STRIDE, BufferUsageHint.DynamicDraw);

            // Update the vertex and index buffers.
            chunk.Mesh!.SetData(result.VertexData!, result.IndexData!);
            result.VertexData = null;
            result.IndexData = null;

            chunk.GenStage = Chunk.GenStageEnum.HasMesh;
        }

        public struct BuildResult
        {
            public float[]? VertexData;
            public uint[]? IndexData;
        }
    }
}
