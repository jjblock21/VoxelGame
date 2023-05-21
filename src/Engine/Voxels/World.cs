using OpenTK.Mathematics;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using VoxelGame.Engine.Voxels.Chunks;
using VoxelGame.Engine.Voxels.Chunks.MeshGen;
using VoxelGame.Framework.Helpers;
using VoxelGame.Game;
using VoxelGame.Game.Blocks;
using static VoxelGame.Framework.Helpers.MethodImplConstants;

namespace VoxelGame.Engine.Voxels
{
    public class World
    {
        public readonly ConcurrentDictionary<Vector3i, Chunk> Chunks;

        public readonly ChunkBuilderProvider ChunkBuilder;
        public readonly ChunkGeneratorProvider ChunkGenerator;

        private WorldRenderer _worldRenderer;

        /// <param name="worldRenderer">Renderer rendering the world.</param>
        /// <param name="attribArrBindingIndex">Biding point of the Vertex Attribute Array specifying the data layout in the chunks vertex buffers.</param>
        public World(WorldRenderer worldRenderer)
        {
            Chunks = new ConcurrentDictionary<Vector3i, Chunk>();

            ChunkBuilder = new ChunkBuilderProvider();
            ChunkGenerator = new ChunkGeneratorProvider(this);

            _worldRenderer = worldRenderer;
        }

        public void DrawChunks()
        {
            _worldRenderer.Begin();
            // Note: I'm iterating over a dictionary here which is not the best idea, but I couldn't find a better collection satisfying all my needs.
            // Perhaps you could change this to only regenerate the enumerator every time the colection is modified?
            // Or make a custom unordered concurrent collection you can access through keys and which has fast iteration speeds if possible idk.
            foreach (Chunk chunk in Chunks.Values)
            {
                if (chunk.GenStage == Chunk.GenStageEnum.HasMesh)
                    _worldRenderer.RenderChunk(chunk);
            }
        }

        public void Free()
        {
            _worldRenderer.Free();
            foreach (Chunk chunk in Chunks.Values)
                chunk.Free();
        }

        public void Update()
        {
            // Manage chunk mesh rebuilds.
            ChunkBuilder.Update();
        }

        /// <summary>
        /// Loads and unloads chunks to move the loaded region to be centered around <paramref name="centerChunk"/>.
        /// </summary>
        public void MoveLoadedRegion(Vector3i centerChunk)
        {
            throw new NotImplementedException();
        }

        #region Methods to manipulate blocks
        /// <summary>
        /// Attempts to retrieve a chunk in the world.
        /// </summary>
        /// <param name="location">Index of the chunk.</param>
        /// <returns><see langword="null"/> if the chunk is not loaded.</returns>
        public Chunk? TryGetChunk(Vector3i location)
        {
            return Chunks.TryGetValue(location, out Chunk? c) ? c : null;
        }

        /// <summary>
        /// Attempts to retrieve a block from the world.
        /// </summary>
        /// <param name="location">The location of the block in global coordinates.</param>
        /// <returns><see langword="false"/> if the chunk containing the block is not loaded.</returns>
        public bool TryGetBlock(Vector3i location, out BlockType block)
        {
            (Vector3i ci, Vector3i bi) = GetChunkBlockIndex(location);
            if (Chunks.TryGetValue(ci, out Chunk? c) && c.GenStage != Chunk.GenStageEnum.NoData)
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
            (Vector3i ci, Vector3i bi) = GetChunkBlockIndex(location);
            if (Chunks.TryGetValue(ci, out Chunk? c) && c.GenStage != Chunk.GenStageEnum.NoData)
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
            ChunkBuilder.BuildChunk(chunk, dontDefer: true);

            void rebuild(uint dir, int x, int y, int z)
            {
                // Try to get the neighbour chunk in the corresponding direction.
                Vector3i ci = chunkIndex + GetDirAsVector(dir);
                if (Chunks.TryGetValue(ci, out Chunk? c) && c.GenStage != Chunk.GenStageEnum.NoData)
                {
                    // Dont mark the chunk as dirty if the block next to the affected block is an air block.
                    if (c.Blocks![x, y, z] == BlockType.Air) return;

                    // Rebuild the chunk if the block next to the affected block culls against it.
                    SharedBlockData data = Minecraft.Instance.BlockRegistry.GetData(c.Blocks[x, y, z]);
                    if ((data.Params & BlockParams.DontCull) == 0)
                        ChunkBuilder.BuildChunk(c, dontDefer: true);
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

        // Static methods for helping with world related stuff.
        #region Helpers
        /// <summary>
        /// Returns a <see cref="Vector3i"/> pointing in the direction specified.
        /// </summary>
        /// <param name="direction">Direction integer (See implementation in <see cref="Minecraft.Engine.Voxels.ChunkBuilder"/>)</param>
        /// <exception cref="Exception"/>
        public static Vector3i GetDirAsVector(uint direction)
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
        public static BlockFaces GetFace(uint direction)
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

        /// <returns>The index of the chunk the given position is in.</returns>
        public static Vector3i GetChunkIndex(Vector3 pos)
        {
            int x = (int)MathF.Floor(pos.X / 16f);
            int y = (int)MathF.Floor(pos.Y / 16f);
            int z = (int)MathF.Floor(pos.Z / 16f);
            return new Vector3i(x, y, z);
        }

        /// <summary>
        /// Converts an absolute location in the world to chunk and block indices.
        /// </summary>
        /// <returns>Chunk index and block index in that order.</returns>
        public static (Vector3i, Vector3i) GetChunkBlockIndex(Vector3 pos)
        {
            Vector3i chunkIndex = GetChunkIndex(pos);
            // Calculate block index in chunk.
            int bx = (int)MathH.Mod(pos.X, 16f);
            int by = (int)MathH.Mod(pos.Y, 16f);
            int bz = (int)MathH.Mod(pos.Z, 16f);
            return (chunkIndex, new Vector3i(bx, by, bz));
        }
        #endregion
    }
}
