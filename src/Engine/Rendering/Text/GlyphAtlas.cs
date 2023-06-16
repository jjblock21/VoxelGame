using OpenTK.Mathematics;
using System;

namespace VoxelGame.Engine.Rendering.Text
{
    /// <summary>
    /// Simple class for indexing characters from a texture atlas.
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

        /// <param name="texture">The atlas texture. (needs to contain 95 characters, UTF-16 code 32 to 126)</param>
        /// <param name="glyphWidth">Width of a glyph on the texture.</param>
        /// <param name="glyphHeight">Height of a glyph on the texture.</param>
        /// <exception cref="ArgumentException"/>
        public GlyphAtlas(Texture2D texture, int glyphWidth, int glyphHeight)
        {
            // Validate inputs.
            if (texture.Width % glyphWidth != 0 || texture.Height % glyphHeight != 0)
                throw new ArgumentException("Texture dimensions must be divisible by glyph dimensions.");
            if (texture.Width * texture.Height / (glyphWidth * glyphHeight) < 126 - 32)
                throw new ArgumentException("Texture must contain UTF-16 glyph 32 - 126");

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
                // Calculate the x and y coordinates from the index based on the number of glyphs per row,
                // then divide by glyphs per row to obtain the position in texture space.
                float x = (float)(i % glyphsPerRow) / glyphsPerRow;
                float y = (float)(i / glyphsPerRow) / glyphsPerRow;
                _glyphCoords[i] = new Vector2(x, y);
            }
        }

        /// <summary>
        /// Converts a UTF-16 character into an index for the texture atlas.
        /// </summary>
        public static int CharToIndex(char c)
        {
            // ASCII is contained in UTF-16 (which is what c# uses) so we can convert between them easily.
            // ASCII Range 32-126
            // -1 is to convert the character into a zero based index
            if (c < 33 || c > 126) return 127 - 32 - 1; // Index of the error character.
            return c - 32 - 1; // -32 to exclude the 31 invisible characters at the beginning and space.
        }

        /// <summary>
        /// Returns the source rectangle for the given character.
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
