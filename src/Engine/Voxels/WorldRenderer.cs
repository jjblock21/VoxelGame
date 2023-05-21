using OpenTK.Graphics.OpenGL4;
using VoxelGame.Engine.Rendering;
using VoxelGame.Engine.Voxels.Chunks;
using VoxelGame.Framework;

namespace VoxelGame.Engine.Voxels
{
    public class WorldRenderer : IHasDriverResources
    {
        private int _offsetVecHandle;
        private int _projViewMatHandle;

        private ShaderHandle _worldShader;
        private Texture2D _worldTexture;
        private VertexArrayObject _chunkMeshVao;

        private BaseCamera _camera;

        public WorldRenderer(VertexArrayObject chunkMeshVao, ShaderHandle worldShader, Texture2D worldTexture, BaseCamera camera)
        {
            _chunkMeshVao = chunkMeshVao;
            _worldShader = worldShader;
            _worldTexture = worldTexture;
            _camera = camera;

            _offsetVecHandle = GL.GetUniformLocation(worldShader, "uOffset");
            _projViewMatHandle = GL.GetUniformLocation(worldShader, "mProjView");
        }

        public void Begin()
        {
            GL.UseProgram(_worldShader);
            GLHelper.UniformMatrix4(_projViewMatHandle, _camera.ProjViewMat);

            _worldTexture.Bind();

            GL.BindVertexArray(_chunkMeshVao.Handle);
        }

        public void RenderChunk(Chunk chunk)
        {
            if (chunk.Mesh == null) return;

            GL.Uniform3(_offsetVecHandle, chunk.Offset);
            chunk.Mesh.BindBuffers(0);
            GL.DrawElements(PrimitiveType.Triangles, chunk.Mesh.NumIndices, DrawElementsType.UnsignedInt, 0);
        }

        public void Free()
        {
            _chunkMeshVao.Free();
            GL.DeleteTexture(_worldTexture.Handle);
            GL.DeleteShader(_worldShader);
        }
    }
}
