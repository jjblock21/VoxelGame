using System;

namespace VoxelGame.Engine.Voxels.Blocks
{
    /// <summary>
    /// Type: Flags enum
    /// </summary>
    [Flags]
    public enum BlockFaces : byte
    {
        Front = 1,
        Right = 2,
        Back = 4,
        Left = 8,
        Top = 16,
        Bottom = 32,
        All = 63
    }

    /// <summary>
    /// Type: Flags enum
    /// </summary>
    [Flags]
    public enum BlockParams
    {
        Default = 0,
        DontCull = 1,
        Transparent = 3, /*This is 3 to also set the DontCull flag when set.*/
    }
}
