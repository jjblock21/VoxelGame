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
        void BuildMesh(Vector3i pos, IMeshInterface meshInterface);

        /// <summary>
        /// Build the mesh for one face of the block.
        /// </summary>
        /// <param name="totalVerts">Total amount of vertices in the vertex buffer.</param>
        /// <param name="vertices">Array of vertex data.</param>
        /// <param name="indices">Array of index data.</param>
        /// <returns><see langword="false"/> if the generated data should be ignored, vertices and indexes can be null here.</returns>
        void BuildFace(uint direction, Vector3i pos, IMeshInterface meshInterface);
    }
}
