using OpenTK.Mathematics;
using System;
using VoxelGame.Engine.Rendering;

namespace VoxelGame.Framework
{
    public struct Argb : IEquatable<Argb>
    {
        /// <summary>
        /// Packed representation of the color with each channel occupying 8 bits.
        /// </summary>
        public readonly uint Packed;

        public Argb(Argb color, uint alpha)
        {
            Packed = (color.Packed & 0xFFFFFF00) | alpha << 24;
        }

        public Argb(float r, float g, float b, float a = 0)
        {
            Packed = (uint)(a * 255f) << 24 | (uint)(r * 255f) << 16 |
                   (uint)(g * 255f) << 8 | (uint)(b * 255f);
        }

        public Argb(byte r, byte g, byte b, byte a = 255)
        {
            Packed = (uint)a << 24 | (uint)r << 16 | (uint)g << 8 | b;
        }

        /// <summary>
        /// Unpacks the packed argb integer into integers with values between 0 and 255.
        /// </summary>
        public Vector4i Unpack()
        {
            int a = (int)((Packed >> 24) & 255);
            int r = (int)((Packed >> 16) & 255);
            int g = (int)((Packed >> 8) & 255);
            int b = (int)(Packed & 255);
            return new Vector4i(r, g, b, a);
        }

        /// <summary>
        /// Unpacks the packed argb integer into floats with values between 0 and 1.
        /// </summary>
        public Vector4 UnpackFloat()
        {
            const int toFloat = 1 / 255;

            float a = ((Packed >> 24) & 255) * toFloat;
            float r = ((Packed >> 16) & 255) * toFloat;
            float g = ((Packed >> 8) & 255) * toFloat;
            float b = (Packed & 255) * toFloat;
            return new Vector4(r, g, b, a);
        }

        #region Predefined

        static Argb()
        {
            White = new Argb(255, 255, 255);
            Black = new Argb(0, 0, 0);
            Transparent = new Argb(0, 0, 0, 0);
        }

        // Add more as needed.
        public static readonly Argb White;
        public static readonly Argb Black;
        public static readonly Argb Transparent;

        #endregion

        #region Operators

        public bool Equals(Argb other) => Packed == other.Packed;
        public override bool Equals(object? obj) => obj is Texture2D && Equals((Texture2D)obj);
        public override int GetHashCode() => Packed.GetHashCode();

        public static bool operator ==(Argb a, Argb b) => a.Equals(b);
        public static bool operator !=(Argb a, Argb b) => !a.Equals(b);

        #endregion
    }
}
