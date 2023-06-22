using OpenTK.Mathematics;
using VoxelGame.Engine.Voxels.Chunks;
using VoxelGame.Engine.Voxels.Helpers;

namespace VoxelGame.Game.Level
{
    public class World
    {
        private ChunkManager _chunkManager;
        private SpectatorCamera _playerCamera;

        private Vector3i _centerChunk;

        public const int RENDER_DIST = 6;

        public World(ChunkManager chunkManager, SpectatorCamera playerCamera)
        {
            _chunkManager = chunkManager;
            _playerCamera = playerCamera;
        }

        public void GenerateAsync()
        {
            _chunkManager.ClearChunks();

            // Load all chunks in render distance around the player.
            Vector3i chunkIn = ConvertH.PosToChunkIndex(_playerCamera.Translation);
            _centerChunk = chunkIn;
            _chunkManager.LifetimeManager.MoveCenterChunk(chunkIn);
        }

        public void Update()
        {
            Vector3i chunkIn = ConvertH.PosToChunkIndex(_playerCamera.Translation);
            if (chunkIn != _centerChunk)
            {
                _centerChunk = chunkIn;

                // Load chunks now within render distance, unload chunks outside render distance.
                _chunkManager.LifetimeManager.MoveCenterChunk(chunkIn);
            }
        }
    }
}
