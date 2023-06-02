using OpenTK.Graphics.OpenGL4;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VoxelGame.Engine.Rendering;
using VoxelGame.Framework;

namespace VoxelGame.Engine.Voxels.Chunks.MeshGen
{
    // I know this is a bit of a mess and im not even sure its doing exactly what I want,
    // but it works most of the time.
    public class ChunkBuilderProvider
    {
        // Limit for how many chunks can be uploaded to the gpu in a single frame.
        public const int MESH_UPLOAD_LIMIT = 8;
        // Limit for how many chuks can be build on the main thread in a single frame.
        public const int SYNC_CHUNK_BUILD_LIMIT = 4;

        // Collection of processors, one for each thread.
        private ThreadLocal<ChunkBuilder> _processor;

        // Finsihed chunks processed on the threadpool.
        private ConcurrentQueue<ChunkBuildResult> _processedChunks;

        // Chunks waiting to be processed on the main thread.
        private Queue<Chunk> _watingSyncChunks;

        public ChunkBuilderProvider()
        {
            _processor = new ThreadLocal<ChunkBuilder>(() => new ChunkBuilder());
            _processedChunks = new ConcurrentQueue<ChunkBuildResult>();

            _watingSyncChunks = new Queue<Chunk>();
        }

        /// <summary>
        /// Submit a chunk for having its mesh built on the threadpool.
        /// </summary>
        /// <param name="dontDefer">
        /// If set to <see langword="true"/>, no Task will be started and the chunk 
        /// will be added to a Queue to be processed on the main thread instead.<br/>
        /// If set to <see langword="true"/>, the method can only be called from the main thread!
        /// </param>
        public void BuildChunk(Chunk chunk, bool dontDefer = false)
        {
            if (chunk.GenStage == Chunk.GenStageEnum.NoData) return;

            // If the chunk should be processed on the main thread,
            if (dontDefer && !_watingSyncChunks.Contains(chunk))
            {
                // Add it to a queue if it isn't added yet from procesing later.
                _watingSyncChunks.Enqueue(chunk);
                return;
            }

            // Start the task and pass in a cancellation token from the chunks source.
            // If another task has already been dispatched to work on the chunk, dont start another.
            if (chunk.AsyncStage == TaskState.Dispatched) return;
            chunk.AsyncStage = TaskState.Dispatched;

            CancellationToken token = chunk.BuilderCancelSrc.Token;
            Task.Factory.StartNew(() =>
            {
                // If an async builder task is already running, cancel it and continue with the new data.
                if (chunk.AsyncStage == TaskState.Running)
                {
                    // The already running task will be cancelled, the current task wont 
                    chunk.BuilderCancelSrc.Cancel();
                }

                chunk.AsyncStage = TaskState.Running;
                ChunkBuildResult result = BuildChunkMain(chunk, token);
                chunk.AsyncStage = TaskState.Inert;

                _processedChunks.Enqueue(result);
            }, token);
        }

        /// <summary>
        /// Process waiting chunks to be processed synchrounously or
        /// update the mesh of a completed chunk.<br/>
        /// ! Needs to be called from the OpenGL context thread !
        /// </summary>
        public void Update()
        {
            // If chunks are waiting to be processed synchrounously, delay uploading the meshes of async chunks.
            if (_watingSyncChunks.Count > 0)
            {
                for (int num = 0; num < SYNC_CHUNK_BUILD_LIMIT; num++)
                {
                    if (!_watingSyncChunks.TryDequeue(out Chunk? chunk)) break;

                    // If a task is running or has been dispatched for building cancel it.
                    if (chunk.AsyncStage != TaskState.Inert)
                        chunk.BuilderCancelSrc.Cancel();

                    // This doesn't cause any problems with another task beign dispatched while this is still running,
                    // becasue this is running synchronously.
                    chunk.AsyncStage = TaskState.Inert;
                    UploadMesh(BuildChunkMain(chunk, CancellationToken.None));
                }
                return;
            }

            for (int num1 = 0; num1 < MESH_UPLOAD_LIMIT; num1++)
            {
                if (!_processedChunks.TryDequeue(out ChunkBuildResult result)) break;

                // Update the mesh of a completed async chunk on the opengl context thread.
                UploadMesh(result);
            }
        }

        private void UploadMesh(ChunkBuildResult result)
        {
            if (result.Chunk.Mesh == null)
            {
                // Create the mesh with a combined vertex array.
                result.Chunk.Mesh = new Mesh(ChunkBuilder.BUFFER_STRIDE, BufferUsageHint.DynamicDraw);
            }

            // Update the vertex and index buffers.
            result.Chunk.Mesh!.SetData(result.VertexData!, result.IndexData!);
            result.VertexData = null;
            result.IndexData = null;
        }

        private ChunkBuildResult BuildChunkMain(Chunk chunk, CancellationToken token)
        {
            ChunkBuildResult result = _processor.Value!.Process(chunk, token);
            chunk.GenStage = Chunk.GenStageEnum.HasMesh;
            return result;
        }
    }
}
