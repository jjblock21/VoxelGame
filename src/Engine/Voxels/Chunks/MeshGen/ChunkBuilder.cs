using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using VoxelGame.Engine.Voxels.Blocks;
using VoxelGame.Engine.Voxels.Helpers;
using VoxelGame.Framework.Helpers;
using VoxelGame.Game;
using VoxelGame.Game.Blocks;
using static VoxelGame.Framework.Helpers.MethodImplConstants;

namespace VoxelGame.Engine.Voxels.Chunks.MeshGen
{
    public class ChunkBuilder : IMeshInterface
    {
        /// <summary>
        /// Vertex buffer stride (not as bits/bytes but as count of entries)
        /// </summary>
        public const int BUFFER_STRIDE = 6;

        //TODO: Rewrite this garbage.

        private Chunk? _target;
        private Chunk?[] _neighbours;
        private List<float> _vertexes;
        private List<uint> _indexes;

        private uint _totalVertices;

        public ChunkBuilder()
        {
            _neighbours = new Chunk[6];
            _vertexes = new List<float>();
            _indexes = new List<uint>();
            _totalVertices = 0;
        }

        public uint TotalVerts
        {
            get => _totalVertices;
            set => _totalVertices = value;
        }

        public void AddIndex(uint index)
        {
            _indexes.Add(index);
        }

        public void AddVertex(float x, float y, float z, float textureX, float textureY, float brightness)
        {
            _vertexes.Add(x);
            _vertexes.Add(y);
            _vertexes.Add(z);
            _vertexes.Add(textureX);
            _vertexes.Add(textureY);
            _vertexes.Add(brightness);
        }

        [MethodImpl(OPTIMIZE)]
        public ChunkBuilderManager.BuildResult Process(Chunk target, CancellationToken token)
        {
            _target = target;
            _totalVertices = 0;
            try
            {
                // Loop over all 6 directions and find neighbor chunks.
                for (uint dir = 0; dir < 6; dir++)
                {
                    token.ThrowIfCancellationRequested();

                    Vector3i location = target.Location + ConvertH.DirToVector(dir);
                    Chunk? chunk = Minecraft.Instance.Session.ChunkManager.GetChunk(location);

                    // If a chunk exists but doesn't have data yet don't add it to the array.
                    if (chunk != null && chunk.GenStage == Chunk.GenStageEnum.NoData)
                    {
                        // Always make sure to set this to null if the neighbor is invalid, otherwise it will create weird results.
                        _neighbours[dir] = null;
                    }
                    else _neighbours[dir] = chunk;
                }

                InternalBuildMesh(token);

                var result = new ChunkBuilderManager.BuildResult();
                token.ThrowIfCancellationRequested();
                result.VertexData = _vertexes.ToArray();
                token.ThrowIfCancellationRequested();
                result.IndexData = _indexes.ToArray();
                return result;
            }
            finally
            {
                // Clear the lists to free up memory even if an exception is thrown.
                _vertexes.Clear();
                _indexes.Clear();
            }
        }

        [MethodImpl(OPTIMIZE)]
        private void InternalBuildMesh(CancellationToken token)
        {
            // Loop over all blocks, check their neighbors in all directions and build a face if they border an air block.
            VectorUtility.Vec3For(0, 0, 0, 16, 16, 16, i =>
            {
                token.ThrowIfCancellationRequested();

                // _target.Blocks will only be null if this is called in the wrong stage.
                // If this happens I fucked up big somewhere.
                if (_target!.Blocks![i.X, i.Y, i.Z] == BlockType.Air) return; // Return is basically continue when using Vec3For

                BlockEntry data = Minecraft.Instance.BlockRegistry[_target.Blocks[i.X, i.Y, i.Z]];
                Vector3i index = new Vector3i(i.X, i.Y, i.Z);

                // If the block should not be culled against other blocks, generate all faces and exit.
                if ((data.Params & BlockParams.DontCull) != 0)
                {
                    data.Model.BuildMesh(index, this);
                    return;
                }

                // If it needs to be culled against other blocks, loop over all directions and
                // check if the block next to that face is solid.
                // If yes build that face.
                for (uint dir = 0; dir < 6; dir++)
                {
                    if (ShouldRenderFace(i + ConvertH.DirToVector(dir), data))
                        data.Model.BuildFace(dir, index, this);
                }
            });
        }

        [MethodImpl(OPTIMIZE)]
        private bool ShouldRenderFace(Vector3i pos, BlockEntry data)
        {
            [MethodImpl(INLINE)]
            bool isTransparent(int i, int bx, int by, int bz)
            {
                BlockType type = _neighbours[i]!.Blocks![bx, by, bz]; //Sometimes the neighbors are created but not generated yet.
                return type == BlockType.Air || (data.Params & BlockParams.DontCull) != 0;
            }

            // Check if position is outside of chunk and check neighbor.
            // We expect to only have to check bordering blocks to the chunk, so the positions to check are hard coded.

            #pragma warning disable format // For some nice formatting
            if (pos.Z > 15) return _neighbours[0] == null ? false : isTransparent(0,  pos.X, pos.Y,     0);
            if (pos.X > 15) return _neighbours[1] == null ? false : isTransparent(1,      0, pos.Y, pos.Z);
            if (pos.Z < 0)  return _neighbours[2] == null ? false : isTransparent(2,  pos.X, pos.Y,    15);
            if (pos.X < 0)  return _neighbours[3] == null ? false : isTransparent(3,     15, pos.Y, pos.Z);
            if (pos.Y > 15) return _neighbours[4] == null ? false : isTransparent(4,  pos.X,     0, pos.Z);
            if (pos.Y < 0)  return _neighbours[5] == null ? false : isTransparent(5,  pos.X,    15, pos.Z);
            #pragma warning restore format

            // If the block is inside the chunk, check if it is an air block.
            return _target!.Blocks![pos.X, pos.Y, pos.Z] == BlockType.Air;
        }
    }
}
