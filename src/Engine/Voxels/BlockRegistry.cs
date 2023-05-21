using VoxelGame.Game.Blocks;

namespace VoxelGame.Engine.Voxels
{
    public class BlockRegistry
    {
        private SharedBlockData[] Behaviours;

        public BlockRegistry(int capacity)
        {
            Behaviours = new SharedBlockData[capacity];
        }

        public void Register(BlockType block, SharedBlockData behaviour)
        {
            int index = (int)block - 1;
            if (index >= Behaviours.Length || block <= 0) throw new IndexOutOfRangeException();
            Behaviours[index] = behaviour;
        }

        public SharedBlockData GetData(BlockType block)
        {
            int index = (int)block - 1;
            if (index >= Behaviours.Length || block < 0) throw new IndexOutOfRangeException();
            return Behaviours[index];
        }
    }
}
