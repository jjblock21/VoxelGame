using OpenTK.Mathematics;
using System.Threading;
using System.Threading.Tasks;
using VoxelGame.Engine.Voxels.Helpers;

namespace VoxelGame.Engine.Voxels.Chunks.ChunkGen
{
    public class ChunkGeneratorProvider
    {
        // Collection of processors, one for each thread.
        private ThreadLocal<ChunkGenerator> _generator;

        private ChunkManager _chunkManager;

        public ChunkGeneratorProvider(ChunkManager chunkManager)
        {
            _chunkManager = chunkManager;
            _generator = new ThreadLocal<ChunkGenerator>(() => new ChunkGenerator());
        }

        public void GenChunk(Vector3i location)
        {
            // First check here if the chunk already exists, but if the chunk object is still being created this wont work.
            if (_chunkManager.Chunks.ContainsKey(location)) return;

            // I chose to not do this in a task.
            Chunk chunk = new Chunk(location);

            if (_chunkManager.Chunks.TryAdd(location, chunk))
                Task.Factory.StartNew(() => Process(chunk));
        }

        private void Process(Chunk chunk)
        {
            _generator.Value!.Generate(chunk);
            chunk.GenStage = Chunk.GenStageEnum.HasData;
            BuildNeighbours(chunk);
        }

        private void BuildNeighbours(Chunk chunk)
        {
            // Directions only go to 6 so 7 will just return (0,0,0) which will check the current chunk.
            for (uint dir = 0; dir < 7; dir++)
            {
                // For every surrounding chunk, check the build stage of its neighbors.
                Vector3i location = chunk!.Location + ConvertH.DirToVector(dir);
                // Try to retrieve the chunk, if that fails, skip it.
                if (!_chunkManager.Chunks.TryGetValue(location, out Chunk? toBuild)) continue;

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
                if (toBuild.GenStage != Chunk.GenStageEnum.NoData && CheckNeighboursFinal(location))
                    _chunkManager.Builder.BuildChunk(toBuild);
            }
        }

        // Checks if all surrounding chunks are "final".
        private bool CheckNeighboursFinal(Vector3i location)
        {
            for (uint dir = 0; dir < 6; dir++)
            {
                // If the chunk exists but doesn't have data return false.
                if (_chunkManager.Chunks.TryGetValue(location + ConvertH.DirToVector(dir), out Chunk? chunk) &&
                    chunk.GenStage == Chunk.GenStageEnum.NoData)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
