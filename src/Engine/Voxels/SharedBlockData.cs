using Minecraft.Game.Blocks;

namespace VoxelGame.Engine.Voxels
{
    public class SharedBlockData
    {
        public readonly BlockParams Params;
        public readonly IBlockModel Model;

        public SharedBlockData(BlockParams blockParams, IBlockModel model)
        {
            Params = blockParams;
            Model = model;
        }
    }
}
