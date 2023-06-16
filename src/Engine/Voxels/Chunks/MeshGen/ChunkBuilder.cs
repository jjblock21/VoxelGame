﻿using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using VoxelGame.Engine.Voxels.Blocks;
using VoxelGame.Engine.Voxels.Helpers;
using VoxelGame.Game;
using VoxelGame.Game.Blocks;
using static VoxelGame.Framework.Helpers.MethodImplConstants;

namespace VoxelGame.Engine.Voxels.Chunks.MeshGen
{
    public class ChunkBuilder
    {
        /// <summary>
        /// Vertex buffer stride (not as bits/bytes but as count of entries)
        /// </summary>
        public const int BUFFER_STRIDE = 6;

        private Chunk? _target;
        private Chunk?[] _neighbours;
        private List<float> _vertices;
        private List<uint> _indices;

        private uint _totalVertices;

        public ChunkBuilder()
        {
            _neighbours = new Chunk[6];
            _vertices = new List<float>();
            _indices = new List<uint>();
            _totalVertices = 0;
        }

        public ChunkBuilderProvider.BuildResult Process(Chunk target, CancellationToken token)
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

                // Output result struct.
                var result = new ChunkBuilderProvider.BuildResult();
                token.ThrowIfCancellationRequested();
                result.VertexData = _vertices.ToArray();
                token.ThrowIfCancellationRequested();
                result.IndexData = _indices.ToArray();
                return result;
            }
            finally
            {
                // Clear the lists to free up memory even if an exception is thrown.
                _vertices.Clear();
                _indices.Clear();
            }
        }

        [MethodImpl(OPTIMIZE)]
        private void InternalBuildMesh(CancellationToken token)
        {
            // Loop over all blocks, check their neighbors in all directions and build a face if they border an air block.
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        token.ThrowIfCancellationRequested();

                        // _target.Blocks will only be null if this is called in the wrong stage.
                        // If this happens I fucked up big somewhere.
                        if (_target!.Blocks![x, y, z] == BlockType.Air) continue;

                        BlockEntry data = Minecraft.Instance.BlockRegistry[_target.Blocks[x, y, z]];
                        Vector3i index = new Vector3i(x, y, z);

                        // If the block should not be culled against other blocks, generate all faces and exit.
                        if ((data.Params & BlockParams.DontCull) != 0)
                        {
                            if (data.Model.BuildMesh(index, ref _totalVertices, out float[]? verts, out uint[]? ind))
                            {
                                _vertices.AddRange(verts!);
                                _indices.AddRange(ind!);
                            }
                            return;
                        }

                        // If it needs to be culled against other blocks, loop over all directions and
                        // check if the block next to that face is solid.
                        // If yes build that face.
                        for (uint dir = 0; dir < 6; dir++)
                        {
                            Vector3i d = ConvertH.DirToVector(dir);
                            if (ShouldRenderFace(x + d.X, y + d.Y, z + d.Z, data))
                            {
                                if (data.Model.BuildFace(dir, index, ref _totalVertices, out float[]? verts, out uint[]? ind))
                                {
                                    _vertices.AddRange(verts!);
                                    _indices.AddRange(ind!);
                                }
                            }
                        }
                    }
                }
            }
        }

        [MethodImpl(INLINE)]
        private bool ShouldRenderFace(int x, int y, int z, BlockEntry data)
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
            if (z > 15) return _neighbours[0] == null ? false : isTransparent(0,  x,  y,  0);
            if (x > 15) return _neighbours[1] == null ? false : isTransparent(1,  0,  y,  z);
            if (z < 0)  return _neighbours[2] == null ? false : isTransparent(2,  x,  y, 15);
            if (x < 0)  return _neighbours[3] == null ? false : isTransparent(3, 15,  y,  z);
            if (y > 15) return _neighbours[4] == null ? false : isTransparent(4,  x,  0,  z);
            if (y < 0)  return _neighbours[5] == null ? false : isTransparent(5,  x, 15,  z);
            #pragma warning restore format

            // If the block is inside the chunk, check if it is an air block.
            return _target!.Blocks![x, y, z] == BlockType.Air;
        }
    }
}
