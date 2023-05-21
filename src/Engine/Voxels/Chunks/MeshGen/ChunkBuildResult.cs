using VoxelGame.Engine.Voxels.Chunks;

namespace VoxelGame.Engine.Voxels.Chunks.MeshGen
{
    /// <summary>
    /// Data returned by the async chunk builder.
    /// </summary>
    public struct ChunkBuildResult
    {
        public float[]? VertexData;
        public uint[]? IndexData;
        public Chunk Chunk;
    }
}
