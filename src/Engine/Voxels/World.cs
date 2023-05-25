using OpenTK.Mathematics;
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
        private WorldRenderer _worldRenderer;

        /// <param name="worldRenderer">Renderer rendering the world.</param>
        public World(WorldRenderer worldRenderer)
        {
            _worldRenderer = worldRenderer;
            _chunkMgr = new ChunkManager();
        }

        public void Render()
        {
            _worldRenderer.Begin();
            // Note: I'm iterating over a dictionary here which is not the best idea, but I couldn't find a better collection satisfying all my needs.
            // Perhaps you could change this to only regenerate the enumerator every time the colection is modified?
            // Or make a custom unordered concurrent collection you can access through keys and which has fast iteration speeds if possible idk.
            foreach (Chunk chunk in _chunkMgr.Chunks.Values)
            {
                if (chunk.GenStage == Chunk.GenStageEnum.HasMesh)
                    _worldRenderer.RenderChunk(chunk);
            }
        }

        public void Update()
        {
            _chunkMgr.Update();
        }

        public void Free()
        {
            _chunkMgr.Free();
            _worldRenderer.Free();
        }

        // Probably temporary
        public void GenChunk(Vector3i location) => _chunkMgr.Generator.GenChunk(location);

        /// <param name="location">Index of the chunk.</param>
        /// <returns><see langword="null"/> if the chunk is not loaded.</returns>
        public Chunk? TryGetChunk(Vector3i location)
        {
            return _chunkMgr.Chunks.TryGetValue(location, out Chunk? c) ? c : null;
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
            (Vector3i ci, Vector3i bi) = ChunkManager.GetChunkBlockIndex(location);
            if (_chunkMgr.Chunks.TryGetValue(ci, out Chunk? c) && c.GenStage != Chunk.GenStageEnum.NoData)
            {
                // If nothing will change exit.
                if (c.Blocks![bi.X, bi.Y, bi.Z] == type) return false;
                c.Blocks[bi.X, bi.Y, bi.Z] = type;
                RebuildAffected(c, ci, bi.X, bi.Y, bi.Z);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Schedules affected chunks for being rebuilt after a block was changed.
        /// </summary>
        [MethodImpl(OPTIMIZE)]
        private void RebuildAffected(Chunk chunk, Vector3i chunkIndex, int x, int y, int z)
        {
            _chunkMgr.Builder.BuildChunk(chunk, dontDefer: true);

            void rebuild(uint dir, int x, int y, int z)
            {
                // Try to get the neighbour chunk in the corresponding direction.
                Vector3i ci = chunkIndex + DirToVector(dir);
                if (_chunkMgr.Chunks.TryGetValue(ci, out Chunk? c) && c.GenStage != Chunk.GenStageEnum.NoData)
                {
                    // Dont mark the chunk as dirty if the block next to the affected block is an air block.
                    if (c.Blocks![x, y, z] == BlockType.Air) return;

                    // Rebuild the chunk if the block next to the affected block culls against it.
                    SharedBlockData data = Minecraft.Instance.BlockRegistry.GetData(c.Blocks[x, y, z]);
                    if ((data.Params & BlockParams.DontCull) == 0)
                        _chunkMgr.Builder.BuildChunk(c, dontDefer: true);
                }
            }

            // If the block is on the border to other chunks, check if they are affected and mark them as dirty aswell.
            if (z >= 15) rebuild(0, x, y, 0);
            if (z <= 0) rebuild(2, x, y, 15);
            if (x >= 15) rebuild(1, 0, y, z);
            if (x <= 0) rebuild(3, 15, y, z);
            if (y >= 15) rebuild(4, x, 0, z);
            if (y <= 0) rebuild(5, x, 15, z);
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

        /// <summary>
        /// Returns a <see cref="BlockFaces"/> value for the direction specified.
        /// </summary>
        /// <param name="direction">Direction integer (See implementation in <see cref="Minecraft.Engine.Voxels.ChunkBuilder"/>)</param>
        /// <exception cref="Exception"></exception>
        public static BlockFaces DirToFace(uint direction)
        {
            return direction switch
            {
                0 => BlockFaces.Front,
                1 => BlockFaces.Right,
                2 => BlockFaces.Back,
                3 => BlockFaces.Left,
                4 => BlockFaces.Top,
                5 => BlockFaces.Bottom,
                _ => throw new Exception("Invalid direction: " + direction)
            };
        }
        #endregion
    }
}
