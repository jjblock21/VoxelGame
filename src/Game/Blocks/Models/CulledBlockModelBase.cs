using OpenTK.Mathematics;
using VoxelGame.Engine.Voxels;
using VoxelGame.Engine.Voxels.Chunks.MeshGen;

namespace VoxelGame.Game.Blocks.Models
{
    public abstract class CulledBlockModelBase : IBlockModel
    {
        #region Face Data
        private const uint NUM_VERTS = 4;

        //TODO: Overhaul this to make textures not be rotated weird on the sides of a block.
        // Vertices for every corner of a block.
        public static readonly float[] cornerVerts = new float[24]
        {
            1, 1, 1,
            1, 0, 1,
            1, 0, 0,
            1, 1, 0,
            0, 0, 1,
            0, 1, 1,
            0, 1, 0,
            0, 0, 0
        };

        // Mappings determining which vertices to use for a face.
        public static readonly uint[] faceVertMappings = new uint[24]
        {
            5, 4, 1, 0, // (Up)      - Forward
            0, 1, 2, 3, // (Forward) - Right
            3, 2, 7, 6, // (Down)    - Back
            4, 5, 6, 7, // (Back)    - Left
            5, 0, 3, 6, // (Right)   - Up
            1, 4, 7, 2, // (Left)    - Down
        };

        // Indices to form a quad.
        public static readonly uint[] quadIndices = new uint[6]
        {
            3, 2, 0,
            2, 1, 0
        };
        #endregion

        protected void MakeSingleFace(uint direction, Vector3i pos, int textureIndex, ref uint totalVerts, out float[]? vertices, out uint[]? indices)
        {
            vertices = new float[NUM_VERTS * ChunkBuilder.BUFFER_STRIDE];
            indices = new uint[quadIndices.Length];

            float[] texCoords = Minecraft.Instance.TextureAtlas[textureIndex];

            // Retrive row containing vertex mappings for face from direction.
            uint mappingRow = direction * NUM_VERTS;
            for (int i = 0; i < NUM_VERTS; i++)
            {
                int vertIndex = i * ChunkBuilder.BUFFER_STRIDE;

                // For every vertex mapping, retrive the vertexes row in the vertex array...
                uint vertRow = faceVertMappings[mappingRow + i] * 3;

                // ...and add the coordinates of the vertex in that row to the list.
                vertices[vertIndex] = cornerVerts[vertRow] + pos.X;
                vertices[vertIndex + 1] = cornerVerts[vertRow + 1] + pos.Y;
                vertices[vertIndex + 2] = cornerVerts[vertRow + 2] + pos.Z;

                // Add texture coordinates for every vertex.
                int texCoordRow = i * 2;
                vertices[vertIndex + 3] = texCoords[texCoordRow];
                vertices[vertIndex + 4] = texCoords[texCoordRow + 1];

                // TODO: Prototype
                float brightness = direction switch
                {
                    0 => 0.75f,
                    1 => 0.85f,
                    4 => 1f,
                    _ => 0.7f
                };
                vertices[vertIndex + 5] = brightness;
            }

            // Add indices to form 2 trangles for each face.
            for (int i = 0; i < quadIndices.Length; i++)
                indices[i] = quadIndices[i] + totalVerts;

            // Increse the number of total vertices by the number of vertices per face.
            totalVerts += NUM_VERTS;
        }

        public abstract bool BuildFace(uint direction, Vector3i pos, ref uint totalVerts, out float[]? vertices, out uint[]? indices);
        public abstract bool BuildMesh(Vector3i pos, ref uint totalVerts, out float[]? vertices, out uint[]? indices);
    }
}
