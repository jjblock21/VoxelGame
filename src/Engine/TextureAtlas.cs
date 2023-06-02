using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;

namespace VoxelGame.Engine
{
    public class TextureAtlas
    {
        public readonly Image<Rgba32> Texture;
        public readonly int Width, Height;
        public readonly int Columns, Rows, Size;

        // Array containing the texture coords for each texture index. (n * 8 but as a jagged array)
        private float[][] _textureCoords;

        private int _currentIndex;
        private Point _currentSlot;
        private int _tileWidth, _tileHeight;

        public const float SMALL_NUMBER = 0.0001f;

        public TextureAtlas(int rows, int columns, int tileWidth, int tileHeight)
        {
            Columns = columns;
            Rows = rows;
            Width = tileWidth * columns;
            Height = tileHeight * rows;
            Texture = new Image<Rgba32>(Width, Height);

            Size = rows * columns;
            _textureCoords = new float[Size][];
            _currentIndex = 0;

            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
            _currentSlot = Point.Empty;
        }

        /// <summary>
        /// Adds a texture onto the atlas texture and disposes of it.<br/>
        /// (The texture will be resized to the tile size of the atlas)
        /// </summary>
        /// <exception cref="OverflowException"/>
        public void AddTexture(Image<Rgba32> texture)
        {
            if (_currentIndex >= Size || _currentSlot.Y >= Height)
                throw new Exception("Texture atlas is full!");

            // Resize image if it is the wrong size.
            if (texture.Width != _tileWidth || texture.Height != _tileHeight)
                texture.Mutate(x => x.Resize(_tileWidth, _tileHeight));

            // Draw texture onto the atlas texture.
            Texture.Mutate(c => c.DrawImage(texture, _currentSlot, 1));
            texture.Dispose();

            NextSlot();

            // Put texture coordinates for opengl into array.
            _textureCoords[_currentIndex] = CalcTexCoords(_currentIndex);
            _currentIndex++;
        }

        private void NextSlot()
        {
            _currentSlot.X += _tileWidth;
            if (_currentSlot.X >= Width)
            {
                _currentSlot.X = 0;
                _currentSlot.Y += _tileHeight;
            }
        }

        /// <summary>
        /// Get the texture coordinates for a texture from its index.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"/>
        public float[] this[int textureIndex]
        {
            get
            {
                if (textureIndex > _currentIndex)
                    throw new IndexOutOfRangeException();
                return _textureCoords[textureIndex];
            }
        }

        private float[] CalcTexCoords(int index)
        {
            // index % Columns: Calculate the x component of the textures index.
            // ... / Columns: Normalize to range from 0 to 1.
            float left = index % Columns / (float)Columns + SMALL_NUMBER;
            // Do the same as for left but on the y axis.
            float top = index / Columns / (float)Rows + SMALL_NUMBER;

            // (1f / Columns): Normalized width of a texture.
            float right = left + 1f / Columns - SMALL_NUMBER * 2;
            // (1f / Rows): Normalized height of a texture.
            float bottom = top + 1f / Rows - SMALL_NUMBER * 2;

            // Small number is added to avoid pulling in pixels from neighboring textures.
            // Return coordinates for all 4 vertices in a 1D array.
            return new float[8] { left, bottom, right, bottom, right, top, left, top };
        }
    }
}
