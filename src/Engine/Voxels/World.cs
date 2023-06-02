using OpenTK.Mathematics;
using System;
using System.Runtime.CompilerServices;
using VoxelGame.Engine.Voxels.Chunks;
using VoxelGame.Engine.Voxels.Helpers;
using VoxelGame.Game;
using VoxelGame.Game.Blocks;
using static VoxelGame.Framework.Helpers.MethodImplConstants;

namespace VoxelGame.Engine.Voxels
{
    public class World
    {
        private ChunkManager _chunkMgr;

        public World(ChunkManager chunkManager)
        {
            _chunkMgr = chunkManager;
        }

        #region Block methods

        /// <summary>
        /// Attempts to retrieve a block from the world.
        /// </summary>
        /// <param name="location">The location of the block in global coordinates.</param>
        /// <returns><see langword="false"/> if the chunk containing the block is not loaded.</returns>
        public bool TryGetBlock(Vector3i location, out BlockType block)
        {
            (Vector3i chunkIndex, Vector3i blockIndex) = ConvertHelper.PosToChunkBlockIndex(location);
            if (_chunkMgr.Chunks.TryGetValue(chunkIndex, out Chunk? chunk) && chunk.GenStage != Chunk.GenStageEnum.NoData)
            {
                block = chunk!.Blocks![blockIndex.X, blockIndex.Y, blockIndex.Z];
                return true;
            }
            block = BlockType.Air;
            return false;
        }

        /// <summary>
        /// Attempts to place a block in the world.
        /// </summary>
        /// <param name="location">The location to place the block at.</param>
        /// <param name="type">The block to place.</param>
        /// <returns><see langword="false"/> if the chunk the block would be placed in is not loaded or if nothing was changed.</returns>
        public bool TrySetBlock(Vector3i location, BlockType type)
        {
            // Split absolute location into chunk & block indexes and get the affected chunk.
            (Vector3i chunkIndex, Vector3i blockIndex) = ConvertHelper.PosToChunkBlockIndex(location);
            if (_chunkMgr.Chunks.TryGetValue(chunkIndex, out Chunk? chunk) && chunk.GenStage != Chunk.GenStageEnum.NoData)
            {
                // If nothing will change exit.
                if (chunk.Blocks![blockIndex.X, blockIndex.Y, blockIndex.Z] == type) return false;
                chunk.Blocks[blockIndex.X, blockIndex.Y, blockIndex.Z] = type;
                _chunkMgr.RebuildModifiedChunks(chunk, chunkIndex, blockIndex.X, blockIndex.Y, blockIndex.Z);
                return true;
            }
            return false;
        }

        #endregion
    }
}
