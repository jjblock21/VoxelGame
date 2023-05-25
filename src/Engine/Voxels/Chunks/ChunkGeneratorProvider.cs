using OpenTK.Mathematics;
using VoxelGame.Game;

namespace VoxelGame.Engine.Voxels.Chunks
{
    public class ChunkGeneratorProvider
    {
        private ThreadLocal<ChunkGenerator> _generator;
        private ChunkManager _chunkManager;

        public ChunkGeneratorProvider(ChunkManager chunkManager)
        {
            _chunkManager = chunkManager;
            _generator = new ThreadLocal<ChunkGenerator>(() => new ChunkGenerator());
        }

        /// <summary>
        /// Generate a chunk asynchronously. (May be called from any thread)
        /// </summary>
        /// <param name="location"></param>
        public void GenChunk(Vector3i location)
        {
            //TODO: Dont create the chunk object and then dispose of it again if the chunk already exists.

            // Create the empty chunk class and add it to the world...
            Chunk chunk = new Chunk(location);
            // ..if the chunk doesn't already exist
            if (_chunkManager.Chunks.TryAdd(location, chunk))
                Process(chunk);

            // Print a debug message to the log as this should ideally be avoided.
            else McWindow.Logger.Debug($"Chunk ({location.X},{location.Y},{location.Z}) already exists, cancelling generator.");
        }

        private void Process(Chunk chunk)
        {
            Task.Factory.StartNew(() =>
            {
                _generator.Value!.Generate(chunk);

                // Stage is marked as volatile.
                chunk.GenStage = Chunk.GenStageEnum.HasData;
                BuildNeighbours(chunk);
            });
        }

        private void BuildNeighbours(Chunk chunk)
        {
            // TODO: Long time task: Optimize this code to only do neccessary checks.
            // Directions only go to 6 so 7 will just return (0,0,0) which will check the current chunk.
            for (uint dir = 0; dir < 7; dir++)
            {
                // For every surrounding chunk, check the build stage of its neighbours.
                Vector3i location = chunk!.Location + World.DirToVector(dir);
                if (!_chunkManager.Chunks.TryGetValue(location, out Chunk? toBuild)) return;

                /*
                 * This piece of code requires a longer explanation:
                 * There are two cases where the first condition is true:
                 * 1. A chunk has data but no mesh.
                 * 2. A chunk has data and a mesh.
                 * 
                 * If a chunk has no mesh yet, we check its neighbors, and if all are in their "final" state, we build the chunk's mesh.
                 * "Final" means that a chunk has been generated or is missing from the collection, 
                 * in which case it's probably still outside render distance and won't be generated yet.
                 * 
                 * If a chunk already has a mesh, we also need to rebuild it, since a new neighbor was generated,
                 * and its mesh needs to connect to the current chunks.
                 * Here we also check if all neighbors are in their final state, to avoid building the chunk twice, if,
                 * for example, two new neighbors were generated.
                 * This is technically not necessary for dir = 7 / location = (0,0,0) but shouldn't cause problems.
                 */
                if (toBuild != null && toBuild.GenStage != Chunk.GenStageEnum.NoData
                    && CheckNeighboursFinal(location))
                {
                    _chunkManager.Builder.BuildChunk(toBuild);
                }
            }
        }

        /// <summary>
        /// Checks if all surrounding chunks are "final".
        /// </summary>
        private bool CheckNeighboursFinal(Vector3i location)
        {
            for (uint dir = 0; dir < 6; dir++)
            {
                Vector3i l = location + World.DirToVector(dir);
                if (_chunkManager.Chunks.TryGetValue(l, out Chunk? chunk) && chunk.GenStage == Chunk.GenStageEnum.NoData)
                    return false;
            }
            return true;
        }
    }
}
