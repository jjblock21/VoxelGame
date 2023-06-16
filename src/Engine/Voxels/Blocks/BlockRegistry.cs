using System;
using VoxelGame.Game.Blocks;

namespace VoxelGame.Engine.Voxels.Blocks
{
    public class BlockRegistry
    {
        private BlockEntry[] Behaviours;

        public BlockRegistry(int capacity)
        {
            Behaviours = new BlockEntry[capacity];
        }

        public BlockEntry this[BlockType block]
        {
            get => Behaviours[GetIndex(block)];
            set => Behaviours[GetIndex(block)] = value;
        }

        private int GetIndex(BlockType block)
        {
            // Air block is excluded since its ignored everywhere anyways.
            int index = (int)block - 1;
            // Validate index.
            if (index >= Behaviours.Length || block < 0)
                throw new IndexOutOfRangeException("Block registry too small.");
            return index;
        }
    }

    public class BlockEntry
    {
        public readonly BlockParams Params;
        public readonly IBlockModel Model;

        public BlockEntry(BlockParams blockParams, IBlockModel model)
        {
            Params = blockParams;
            Model = model;
        }
    }
}
