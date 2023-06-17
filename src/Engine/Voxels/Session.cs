﻿using VoxelGame.Engine.Voxels.Chunks;
using VoxelGame.Framework;
using VoxelGame.Framework.Threading;

namespace VoxelGame.Engine.Voxels
{
    public class Session : IHasDriverResources
    {
        public readonly ChunkManager ChunkManager;
        public readonly WorldRenderer WorldRenderer;

        public bool RenderWorld = false;

        public Session(WorldRenderer worldRenderer)
        {
            ChunkManager = new ChunkManager();
            WorldRenderer = worldRenderer;
        }

        public void Render()
        {
            if (!RenderWorld) return;

            WorldRenderer.Begin();

            // Note: I'm iterating over a dictionary here which is not the best idea, but I couldn't find a better collection satisfying all my needs.
            // Perhaps you could change this to only regenerate the enumerator every time the collection is modified?
            // Or make a custom unordered concurrent collection you can access through keys and which has fast iteration speeds if possible idk.
            foreach (Chunk chunk in ChunkManager.Chunks.Values)
            {
                if (chunk.GenStage == Chunk.GenStageEnum.HasMesh)
                    WorldRenderer.RenderChunk(chunk);
            }
        }

        public void Update()
        {
            RenderThreadCallback.Execute();
        }

        public void Free()
        {
            WorldRenderer.Free();
            ChunkManager.ClearChunks();
        }
    }
}
