using OpenTK.Mathematics;
using VoxelGame.Engine.Voxels.Blocks;

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

        public void BuildFace(uint dir, Vector3i pos, IMeshInterface meshInterface)
        {
            // Just call the ready made method in the base class.
            BlockModelHelper.CreateFace(dir, pos, _textureIndex, meshInterface);
        }

        public void BuildMesh(Vector3i pos, IMeshInterface meshInterface)
        {
            // Output nothing, this should not be used if the block is set up correctly.
        }
    }
}
