using OpenTK.Mathematics;
using VoxelGame.Engine.Voxels.Block;

namespace VoxelGame.Game.Blocks.Models
{
    /// <summary>
    /// Model for a block which will has the same texture on every side and will be culled against other blocks.
    /// </summary>
    public class DefaultBlockModel : IBlockModel
    {
        private readonly int _textureIndex;
        public DefaultBlockModel(int textureIndex)
        {
            _textureIndex = textureIndex;
        }

        public bool BuildFace(uint dir, Vector3i pos, ref uint totalVerts, out float[]? vertices, out uint[]? indices)
        {
            // Just call the ready made method in the base class.
            BlockModelHelper.CreateFace(dir, pos, _textureIndex, ref totalVerts, out vertices, out indices);
            return true;
        }

        public bool BuildMesh(Vector3i pos, ref uint totalVerts, out float[]? vertices, out uint[]? indices)
        {
            // Output nothing, this should not be used if the block is set up correctly.
            vertices = null;
            indices = null;
            return false;
        }
    }
}
