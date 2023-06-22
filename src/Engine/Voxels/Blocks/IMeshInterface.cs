namespace VoxelGame.Engine.Voxels.Blocks
{
    /// <summary>
    /// Provided to block model classes by the chunk builder to add vertexes and indexes without having to allocate and return arrays.
    /// </summary>
    public interface IMeshInterface
    {
        void AddVertex(float x, float y, float z, float textureX, float textureY, float brightness);
        void AddIndex(uint index);

        public uint TotalVerts { get; set; }
    }
}
