using OpenTK.Mathematics;

namespace VoxelGame.Engine.Rendering.Text
{
    /// <summary>
    /// Simple class for indexing charracters from a texture atlas.
    /// </summary>
    public class GlyphAtlas
    {
        public readonly Texture2D Texture;

        /// <summary>
        /// The aspect ratio of the glyphs width to its height.
        /// </summary>
        public readonly float GlyphAspect;

        private float _textureGlyphWidth;
        private float _textureGlyphHeight;
        private Vector2[] _glyphCoords;

        /// <param name="texture">The atlas texture. (needs to contain 95 charracters, UTF-16 code 32 to 126)</param>
        /// <param name="glyphWidth">Width of a glyph on the texture.</param>
        /// <param name="glyphHeight">Height of a glyph on the texture.</param>
        /// <exception cref="ArgumentException"/>
        public GlyphAtlas(Texture2D texture, int glyphWidth, int glyphHeight)
        {
            // Validate inputs.
            if (texture.Width % glyphWidth != 0 || texture.Height % glyphHeight != 0)
                throw new ArgumentException("Texture dimenstions must be divisible by glyph dimensions.");
            if (texture.Width * texture.Height / (glyphWidth * glyphHeight) < 126 - 32)
                throw new ArgumentException("Texture must contain UTF-16 glyph 32 - glyph 126");

            Texture = texture;
            GlyphAspect = (float)glyphHeight / glyphWidth;

            // Calculate the glyph size in texture space.
            _textureGlyphWidth = (float)glyphWidth / texture.Width;
            _textureGlyphHeight = (float)glyphHeight / texture.Height;

            // Calculate the coordinates for each glyph.
            _glyphCoords = new Vector2[126 - 32 + 1];
            int glyphsPerRow = texture.Width / glyphWidth;
            for (int i = 0; i < 126 - 32 + 1; i++)
            {
                // Calculate the x and y coordinates from the index based on the numer of gylphs per row,
                // then divide by glyphs per row to obtein the position in texture space.
                float x = (float)(i % glyphsPerRow) / glyphsPerRow;
                // We need to flip the Y asix since the letters go from the top left to the bottom right in the texture.
                float flippedY = glyphsPerRow - i / glyphsPerRow - 1;
                _glyphCoords[i] = new Vector2(x, flippedY / glyphsPerRow);
            }
        }

        /// <summary>
        /// Converts a UTF-16 charracter into an index for the texture atlas.
        /// </summary>
        public static int CharToIndex(char c)
        {
            // ASCII is conatined in UTF-16 (which is what c# uses) so we can convert between them easily.
            // ASCII Range 32-126
            // -1 is to convert the charracter into a zero based index
            if (c < 33 || c > 126) return 127 - 32 - 1; // Index of the error charracter.
            return c - 32 - 1; // -32 to exclude the 31 invisible charracters at the beginning and space.
        }

        /// <summary>
        /// Returns the source rectangle for the given charracter.
        /// </summary>
        public Vector4 this[char c]
        {
            get
            {
                Vector2 position = _glyphCoords[CharToIndex(c)];
                return new Vector4(position.X, position.Y, _textureGlyphWidth, _textureGlyphHeight);
            }
        }
    }
}
