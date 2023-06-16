using OpenTK.Mathematics;

namespace VoxelGame.Engine.Voxels.Blocks
{
    public interface IBlockModel
    {
        /// <summary>
        /// Build the mesh for the entire block.
        /// </summary>
        /// <param name="totalVerts">Total amount of vertices in the vertex buffer.</param>
        /// <param name="vertices">Array of vertex data.</param>
        /// <param name="indices">Array of index data.</param>
        /// <returns><see langword="false"/> if the generated data should be ignored, vertices and indexes can be null here.</returns>
        bool BuildMesh(Vector3i pos, ref uint totalVerts, out float[]? vertices, out uint[]? indices);

        /// <summary>
        /// Build the mesh for one face of the block.
        /// </summary>
        /// <param name="totalVerts">Total amount of vertices in the vertex buffer.</param>
        /// <param name="vertices">Array of vertex data.</param>
        /// <param name="indices">Array of index data.</param>
        /// <returns><see langword="false"/> if the generated data should be ignored, vertices and indexes can be null here.</returns>
        bool BuildFace(uint direction, Vector3i pos, ref uint totalVerts, out float[]? vertices, out uint[]? indices);
    }
}
