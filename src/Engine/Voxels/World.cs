using OpenTK.Mathematics;
using System;
using System.Runtime.CompilerServices;
using VoxelGame.Engine.Voxels.Chunks;
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
            (Vector3i ci, Vector3i bi) = ChunkManager.GetChunkBlockIndex(location);
            if (_chunkMgr.Chunks.TryGetValue(ci, out Chunk? c) && c.GenStage != Chunk.GenStageEnum.NoData)
            {
                block = c!.Blocks![bi.X, bi.Y, bi.Z];
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
        public bool TryPlaceBlock(Vector3i location, BlockType type)
        {
            // Split absolute location into chunk & block indices and get the affected chunk.
            (Vector3i chunkIndex, Vector3i blockIndex) = ChunkManager.GetChunkBlockIndex(location);
            if (_chunkMgr.Chunks.TryGetValue(chunkIndex, out Chunk? c) && c.GenStage != Chunk.GenStageEnum.NoData)
            {
                // If nothing will change exit.
                if (c.Blocks![blockIndex.X, blockIndex.Y, blockIndex.Z] == type) return false;
                c.Blocks[blockIndex.X, blockIndex.Y, blockIndex.Z] = type;
                _chunkMgr.RebuildModifiedChunks(c, chunkIndex, blockIndex.X, blockIndex.Y, blockIndex.Z);
                return true;
            }
            return false;
        }

        #endregion

        #region Helpers
        /// <summary>
        /// Returns a <see cref="Vector3i"/> pointing in the direction specified.
        /// </summary>
        /// <param name="direction">Direction integer (See implementation in <see cref="Minecraft.Engine.Voxels.ChunkBuilder"/>)</param>
        /// <exception cref="Exception"/>
        public static Vector3i DirToVector(uint direction)
        {
            return direction switch
            {
                0 => Vector3i.UnitZ,
                1 => Vector3i.UnitX,
                2 => -Vector3i.UnitZ,
                3 => -Vector3i.UnitX,
                4 => Vector3i.UnitY,
                5 => -Vector3i.UnitY,
                _ => Vector3i.Zero
            };
        }
        #endregion
    }
}
