using OpenTK.Mathematics;

namespace VoxelGame.Game.Blocks.Models
{
    /// <summary>
    /// Model for a block which will has the same texture on every side and will be culled against other blocks.
    /// </summary>
    public class DefaultBlockModel : CulledBlockModelBase
    {
        private readonly int _textureIndex;
        public DefaultBlockModel(int textureIndex)
        {
            _textureIndex = textureIndex;
        }

        // TODO: Make sure there is not thread locking during mesh generation here.
        public override bool BuildFace(uint dir, Vector3i pos, ref uint totalVerts, out float[]? vertices, out uint[]? indices)
        {
            // Just call the ready made method in the base class.
            MakeSingleFace(dir, pos, _textureIndex, ref totalVerts, out vertices, out indices);
            return true;
        }

        public override bool BuildMesh(Vector3i pos, ref uint totalVerts, out float[]? vertices, out uint[]? indices)
        {
            // Output nothing, this should not be used if the block is set up correctly.
            vertices = null;
            indices = null;
            return false;
        }
    }
}
