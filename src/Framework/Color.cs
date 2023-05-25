using OpenTK.Mathematics;

namespace VoxelGame.Framework
{
    public struct Color
    {
        public readonly uint Argb;

        public Color(Color color, uint alpha)
        {
            Argb = (color.Argb & 0xFFFFFF00) | alpha << 24;
        }

        public Color(float r, float g, float b, float a = 0)
        {
            Argb = (uint)(a * 255) << 24 |
                   (uint)(r * 255) << 16 |
                   (uint)(g * 255) << 8 |
                   (uint)(b * 255);
        }

        public Color(uint r, uint g, uint b, uint a = 255)
        {
            Argb = a << 24 | r << 16 | g << 8 | b;
        }

        /// <summary>
        /// Unpacks the packed argb integer into integers with values between 0 and 255.
        /// </summary>
        public Vector4i Unpack()
        {
            int a = (int)((Argb >> 24) & 255);
            int r = (int)((Argb >> 16) & 255);
            int g = (int)((Argb >> 8) & 255);
            int b = (int)(Argb & 255);
            return new Vector4i(r, g, b, a);
        }

        /// <summary>
        /// Unpacks the packed argb integer into floats with values between 0 and 1.
        /// </summary>
        public Vector4 UnpackFloat()
        {
            const int toFloat = 1 / 255;

            float a = ((Argb >> 24) & 255) * toFloat;
            float r = ((Argb >> 16) & 255) * toFloat;
            float g = ((Argb >> 8) & 255) * toFloat;
            float b = (Argb & 255) * toFloat;
            return new Vector4(r, g, b, a);
        }

        #region Predefined

        static Color()
        {
            White = new Color(1f, 1f, 1f);
            Black = new Color(0f, 0f, 0f);
            Transparent = new Color(0f, 0f, 0f, 0f);
        }

        // Add more as needed.
        public static readonly Color White;
        public static readonly Color Black;
        public static readonly Color Transparent;

        #endregion
    }
}
