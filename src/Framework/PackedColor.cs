using OpenTK.Mathematics;
using System;
using System.Reflection.Metadata;
using VoxelGame.Engine.Rendering;

namespace VoxelGame.Framework
{
    public struct PackedColor : IEquatable<PackedColor>
    {
        public readonly uint Argb;

        public PackedColor(PackedColor color, uint alpha)
        {
            Argb = (color.Argb & 0xFFFFFF00) | alpha << 24;
        }

        public PackedColor(float r, float g, float b, float a = 0)
        {
            Argb = (uint)(a * 255) << 24 |
                   (uint)(r * 255) << 16 |
                   (uint)(g * 255) << 8 |
                   (uint)(b * 255);
        }

        public PackedColor(uint r, uint g, uint b, uint a = 255)
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

        static PackedColor()
        {
            White = new PackedColor(1f, 1f, 1f);
            Black = new PackedColor(0f, 0f, 0f);
            Transparent = new PackedColor(0f, 0f, 0f, 0f);
        }

        // Add more as needed.
        public static readonly PackedColor White;
        public static readonly PackedColor Black;
        public static readonly PackedColor Transparent;

        #endregion

        #endregion
    }
}
