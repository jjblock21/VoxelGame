using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using VoxelGame.Engine.Rendering.Buffers;
using VoxelGame.Game;
using static VoxelGame.Framework.Helpers.MethodImplConstants;
using System;
using VoxelGame.Framework;

namespace VoxelGame.Engine.Rendering
{
    // NOTE: Not the most optimal solution but the most straight forward one I could think of.
    public class SpriteBatch
    {
        private static readonly ushort[] quadIndices = new ushort[]
        {
            2, 1, 0, 3, 2, 0
        };

        public const int NUM_QUAD_VERTS = 4;

        private float[] _verts;
        private uint[] _vertColors;
        private VertexBuffer<float> _vbo;
        private VertexBuffer<uint> _colorVbo;

        private ElementBuffer<ushort> _ebo;
        private VertexArrayObject _vao;

        private int _shader;
        private Vector2d _screenToNdc;
        private int _texWidth;
        private int _texHeight;
        private int _numQuads;
        private int _batchSize;
        private bool _flushed;

        public SpriteBatch(int spriteShader, int batchSize, Vector2i viewport)
        {
            // Prevent ushort overflow errors in index buffer.
            if (batchSize > 32768) throw new ArgumentException("Batch size can't be larger than 32768.");

            _shader = spriteShader;
            _batchSize = batchSize;
            ChangeViewport(viewport);

            // Generate buffer arrays.
            _verts = new float[batchSize * 4 * NUM_QUAD_VERTS];
            _vertColors = new uint[batchSize * NUM_QUAD_VERTS];

            // Generate quad indices.
            ushort[] indices = new ushort[batchSize * quadIndices.Length];
            for (int i = 0; i < batchSize; i++)
            {
                int j = i * quadIndices.Length;
                for (int k = 0; k < quadIndices.Length; k++)
                    indices[j + k] = (ushort)(i * NUM_QUAD_VERTS + quadIndices[k]);
            }

            // Generate index buffer and copy data into the buffer.
            _ebo = new ElementBuffer<ushort>();
            _ebo.BufferData(indices, BufferUsageHint.StaticDraw);

            // Create vertax array object.
            _vao = new VertexArrayObject();
            _vao.AddVertexAttrib(0, 2, 0); // Vertex position
            _vao.AddVertexAttrib(0, 2, 2 * sizeof(float)); // Texture coords
            _vao.AddVertexAttribI(1, 1, 0, VertexAttribIntegerType.UnsignedInt); // Color (in a seperate buffer)

            // Create the vertex buffers with its size set to the max batch size.
            _vbo = new VertexBuffer<float>(4);
            _vbo.BufferData(_verts, BufferUsageHint.StreamDraw);

            _colorVbo = new VertexBuffer<uint>(1);
            _colorVbo.BufferData(_vertColors, BufferUsageHint.StreamDraw);

            _flushed = true;
        }

        /// <summary>
        /// Needs to be called when the size of the window the sprite batch is rendering to is changed.
        /// </summary>
        public void ChangeViewport(Vector2i viewport)
        {
            _screenToNdc = Vector2d.One / ((Vector2d)viewport * 0.5);
        }

        /// <summary>
        /// Changes the sprite sheet used for rendering.
        /// </summary>
        public void Begin(Texture2D spriteSheet)
        {
            if (!_flushed) throw new InvalidOperationException("Flush has to be called before calling Begin.");

            spriteSheet.Bind();
            _texWidth = spriteSheet.Width;
            _texHeight = spriteSheet.Height;
            _flushed = false;
        }

        #region Summary
        /// <summary>
        /// Creates a new quad to be rendered by the sprite batch once <see cref="Flush"/> is called.
        /// </summary>
        /// <param name="x">
        /// X coordinate in screen space to draw the quad at in pixels.<br/>
        /// (Relative to the lower left corner of the window)
        /// </param>
        /// <param name="y">Y coordinate in screen space to draw the quad at in pixels.<br/>
        /// (Relative to the lower left corner of the window)
        /// </param>
        /// <param name="width">Width of the quad to draw in pixels.</param>
        /// <param name="height">Height of the quad to draw in pixels.</param>
        /// <param name="srcRect">Rectangle indicating the region of the currently bound texture to render on the quad in pixels.</param>
        /// <param name="color">Color to overlay on top of the texture in RGBA from 0f to 1f.</param>
        #endregion
        [MethodImpl(OPTIMIZE)] // Just in case.
        public void Quad(int x, int y, int width, int height, Vector4i srcRect = default, PackedColor color = default)
        {
            if (srcRect == default) srcRect = Vector4i.Zero;

            // Convert the source rect from pixels into texture space.
            float tx = (float)srcRect.X / _texWidth;
            float ty = (float)srcRect.Y / _texHeight;
            float tw = (float)srcRect.Z / _texWidth;
            float th = (float)srcRect.W / _texHeight;

            Quad(x, y, width, height, new Vector4(tx, ty, tw, th), color);
        }

        #region Summary
        /// <summary>
        /// Creates a new quad to be rendered by the sprite batch once <see cref="Flush"/> is called.
        /// </summary>
        /// <param name="x">
        /// X coordinate in screen space to draw the quad at in pixels.<br/>
        /// (Relative to the lower left corner of the window)
        /// </param>
        /// <param name="y">Y coordinate in screen space to draw the quad at in pixels.<br/>
        /// (Relative to the lower left corner of the window)
        /// </param>
        /// <param name="width">Width of the quad to draw in pixels.</param>
        /// <param name="height">Height of the quad to draw in pixels.</param>
        /// <param name="srcRect">Rectangle indicating the region of the currently bound texture to render on the quad in OpenGL texture coordinates.</param>
        /// <param name="color">Color to overlay on top of the texture in RGBA from 0f to 1f.</param>
        #endregion
        public void Quad(int x, int y, int width, int height, Vector4 srcRect, PackedColor color = default)
        {
            if (_numQuads >= _batchSize)
            {
                McWindow.Logger.Debug($"Max batch size of {_batchSize} reached, flushing batch.");
                Flush();

                // Set flushed to false since we continue on using the same texture.
                _flushed = false;
            }

            if (color == default) color = PackedColor.Transparent;

            // Maybe move this into the shader.
            float ndcX = (float)(x * _screenToNdc.X) - 1f;
            float ndcY = (float)(y * _screenToNdc.Y) - 1f;
            float ndcW = (float)(width * _screenToNdc.X);
            float ndcH = (float)(height * _screenToNdc.Y);

            BuildQuad(ndcX, ndcY, ndcW, ndcH, srcRect.X, srcRect.Y, srcRect.Z, srcRect.W, color);
            _numQuads++;
        }

        /// <summary>
        /// Uploads the created quads to the GPU and renders the batch.
        /// </summary>
        public void Flush()
        {
            if (_flushed) return;

            UploadBatch();
            GL.UseProgram(_shader);
            GL.BindVertexArray(_vao.Handle);

            _ebo.Bind();
            _vbo.BindRender(0);
            _colorVbo.BindRender(1);

            GL.DrawElements(PrimitiveType.Triangles, _numQuads * quadIndices.Length, DrawElementsType.UnsignedShort, 0);

            // Reset current index.
            _numQuads = 0;
            _flushed = true;
        }

        private void UploadBatch()
        {
            // Using sub data here to not to avoid uploading the empty space in the array aswell.
            int coordBufferSize = _numQuads * 4 * NUM_QUAD_VERTS;
            GLHelper.VertexBufferSubData(_vbo, _verts, coordBufferSize);

            ErrorHandler.Section("dhsakdjhkas");
            int colorBufferSize = _numQuads * NUM_QUAD_VERTS;
            GLHelper.VertexBufferSubData(_colorVbo, _vertColors, colorBufferSize);
            ErrorHandler.Section("Other");
        }

        [MethodImpl(OPTIMIZE)]
        private void BuildQuad(float x, float y, float w, float h, float tx, float ty, float tw, float th, PackedColor color)
        {
            int i = _numQuads * 4 * NUM_QUAD_VERTS;
            int j = _numQuads * NUM_QUAD_VERTS;

            // Vertex 1 (0, 1)
            PutVertex(i, x, y + h, tx, ty + th);
            _vertColors[j] = color.Argb;
            // Vertex 2 (0, 0)
            PutVertex(i + 4, x, y, tx, ty);
            _vertColors[j + 1] = color.Argb;
            // Vertex 3 (1, 0)
            PutVertex(i + 8, x + w, y, tx + tw, ty);
            _vertColors[j + 2] = color.Argb;
            // Vertex 4 (1, 1)
            PutVertex(i + 12, x + w, y + h, tx + tw, ty + th);
            _vertColors[j + 3] = color.Argb;
        }

        [MethodImpl(INLINE)]
        private void PutVertex(int j, float vx, float vy, float vtx, float vty)
        {
            _verts[j] = vx;
            _verts[j + 1] = vy;
            _verts[j + 2] = vtx;
            _verts[j + 3] = vty;
        }
    }
}
