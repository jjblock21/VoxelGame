using OpenTK.Mathematics;
using VoxelGame.Framework;

namespace VoxelGame.Engine.Rendering.Text
{
    public class TextRenderer
    {
        private GlyphAtlas _glyphAtlas;
        private SpriteBatch _spriteBatch;
        private Settings _settings;

        public TextRenderer(GlyphAtlas glyphAtlas, SpriteBatch spriteBatch, Settings settings)
        {
            _glyphAtlas = glyphAtlas;
            _spriteBatch = spriteBatch;
            _settings = settings;
        }

        // Temporary simple text rendering solution.
        public void DrawText(int x, int y, string text, int size, PackedColor color = default)
        {
            int offset = 0;
            int height = (int)(_glyphAtlas.GlyphAspect * size);
            foreach (char c in text)
            {
                if (c == 32) offset += _settings.SpaceWidth;
                else
                {
                    _spriteBatch.Quad(x + offset, y, size, height, _glyphAtlas[c], color);
                    offset += size + _settings.Padding;
                }
            }
        }

        public void Begin() => _spriteBatch.Begin(_glyphAtlas.Texture);

        public void Flush() => _spriteBatch.Flush();

        #region Settings
        public struct Settings
        {
            public static Settings Default = new Settings(2, 6);

            public readonly int Padding;
            public readonly int SpaceWidth;

            /// <param name="padding">Padding between letters.</param>
            /// <param name="spaceWidth">Width of the space charracter.</param>
            public Settings(int padding, int spaceWidth)
            {
                Padding = padding;
                SpaceWidth = spaceWidth;
            }
        }
        #endregion
    }
}
