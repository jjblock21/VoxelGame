﻿using OpenTK.Mathematics;
using VoxelGame.Engine.Voxels.Blocks;
using VoxelGame.Engine.Voxels.Chunks.MeshGen;

namespace VoxelGame.Game.Blocks.Models
{
    public static class BlockModelHelper
    {
        #region Face Data
        private const uint NUM_QUAD_VERTS = 4;

        //TODO: Faces on the debug block are still not facing the direction they should.

        // Vertices for every corner of a block.
        public static readonly float[] cornerVerts = new float[24]
        {
            1, 1, 1, // 0
            1, 0, 1, // 1
            1, 0, 0, // 2
            1, 1, 0, // 3
            0, 0, 1, // 4
            0, 1, 1, // 5
            0, 1, 0, // 6
            0, 0, 0  // 7
        };

        // Indexes to form a quad (clockwise)
        public static readonly uint[] quadIndices = new uint[6]
        {
            0, 2, 1, 0, 3, 2
        };

        // Mappings determining which vertices to use for a face.
        public static readonly uint[] faceVertMappings = new uint[24]
        {
            4, 1, 0, 5, // Forward
            1, 2, 3, 0, // Right
            2, 7, 6, 3, // Back
            7, 4, 5, 6, // Left
            5, 0, 3, 6, // Up
            1, 4, 7, 2, // Down
        };

        #endregion

        #region Create Face
        public static void CreateFace(uint direction, Vector3i pos, int textureIndex, IMeshInterface meshInterface)
        {
            float[] texCoords = Minecraft.Instance.TextureAtlas[textureIndex];

            // Retrieve row containing vertex mappings for face from direction.
            uint vertMappingRow = direction * NUM_QUAD_VERTS;
            for (int i = 0; i < NUM_QUAD_VERTS; i++)
            {
                // For every vertex mapping, retrieve the vertexes row in the vertex array...
                uint vertRow = faceVertMappings[vertMappingRow + i] * 3;

                // ...and add the coordinates of the vertex in that row to the list.
                float x = cornerVerts[vertRow] + pos.X;
                float y = cornerVerts[vertRow + 1] + pos.Y;
                float z = cornerVerts[vertRow + 2] + pos.Z;

                // Add texture coordinates for every vertex.
                int texCoordRow = i * 2;
                float textureX = texCoords[texCoordRow];
                float textureY = texCoords[texCoordRow + 1];

                // TODO: Prototype
                float brightness = direction switch
                {
                    0 => 0.75f,
                    1 => 0.85f,
                    4 => 1f,
                    _ => 0.7f
                };

                meshInterface.AddVertex(x, y, z, textureX, textureY, brightness);
            }

            // Return indexes to form a quad.
            for (uint i = 0; i < quadIndices.Length; i++)
                meshInterface.AddIndex(quadIndices[i] + meshInterface.TotalVerts);

            // Increase the number of total vertices by the number of vertices per face.
            meshInterface.TotalVerts += NUM_QUAD_VERTS;
        }
        #endregion
    }
}
