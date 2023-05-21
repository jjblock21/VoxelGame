using OpenTK.Mathematics;
using VoxelGame.Game;

namespace VoxelGame.Engine.Voxels.Chunks
{
    public class ChunkGeneratorProvider
    {
        private ThreadLocal<ChunkGenerator> _processor;
        private World _world;

        public ChunkGeneratorProvider(World world)
        {
            _world = world;
            _processor = new ThreadLocal<ChunkGenerator>(() => new ChunkGenerator());
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
            if (_world.Chunks.TryAdd(location, chunk))
                Process(chunk);

            // Print a debug message to the log as this should ideally be avoided.
            else McWindow.Logger.Debug($"Chunk ({location.X},{location.Y},{location.Z}) already exists, cancelling generator.");
        }

        private void Process(Chunk chunk)
        {
            Task.Factory.StartNew(() =>
            {
                _processor.Value!.Generate(chunk);

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
                Vector3i location = chunk!.Location + World.GetDirAsVector(dir);
                Chunk? toBuild = _world.TryGetChunk(location);

                if (toBuild != null && (
                    //TODO: Perhaps change this os only chunks who have all their neighbours created already and have a mesh are rebuilt to save uneccessary rebuilds (might also prevent the badly fixed bug where some neighbouring chunks of chunks being rebuilt are created but not initialized)

                    // Build the chunk if it doesn't have a mesh yet and all its neighbours are in their final state.
                    toBuild.GenStage == Chunk.GenStageEnum.HasData && CheckNeighboursFinal(location) ||
                    // Rebuild the chunk if it has a mesh that needs to connect to the current chunks mesh later on.
                    toBuild.GenStage == Chunk.GenStageEnum.HasMesh // This is not neccessary for dir = 7 / location = (0,0,0) but shouldn't cause problems.
                    ))
                {
                    _world.ChunkBuilder.BuildChunk(toBuild);
                }
            }
        }

        /// <summary>
        /// Checks if all surroudning chunks have data or aren't loaded yet.
        /// </summary>
        private bool CheckNeighboursFinal(Vector3i location)
        {
            for (uint dir = 0; dir < 6; dir++)
            {
                //TODO: Make sure the chunk object is not being locked for rendering.
                Chunk? chunk = _world.TryGetChunk(location + World.GetDirAsVector(dir));
                if (chunk != null && chunk.GenStage == Chunk.GenStageEnum.NoData)
                    return false;
            }
            return true;
        }
    }
}
