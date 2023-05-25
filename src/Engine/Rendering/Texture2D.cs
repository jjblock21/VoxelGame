using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using VoxelGame.Framework;

namespace VoxelGame.Engine.Rendering
{
    /// <summary>
    /// Abstraction for an OpenGL texture object.
    /// </summary>
    public struct Texture2D : IEquatable<Texture2D>, IHasDriverResources
    {
        public readonly int Handle;
        public readonly int Width;
        public readonly int Height;
        private bool _initialized;

        /// <summary>
        /// Creates a new OpenGL Texture.<br/>
        /// (!) Needs to be called inside a GL context.
        /// </summary>
        /// <param name="unit">Texture slot the texture ends up in.</param>
        public Texture2D(int width, int height)
        {
            Handle = GL.GenTexture();
            Width = width;
            Height = height;

            _initialized = false;
        }

        /// <summary>
        /// (!) Needs to be called inside a GL context.
        /// </summary>
        public void Bind()
        {
            if (!_initialized) throw new InvalidOperationException("Texture has no data.");
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        /// <summary>
        /// Moves image data into the textures data store.<br/>
        /// (!) Needs to be called inside a GL context.
        /// </summary>
        public void SetData(byte[] imageData)
        {
            if (_initialized) throw new InvalidOperationException("Texture already has data.");

            GL.BindTexture(TextureTarget.Texture2D, Handle);
            GLHelper.ApplyDefaultTexParams();
            GL.TexImage2D(TextureTarget.Texture2D, 0, _internalFormat, Width, Height, 0, _format, _type, imageData);
            _initialized = true;
        }

        /// <summary>
        /// Deletes underlying the OpenGL texture.
        /// </summary>
        public void Free()
        {
            if (_initialized) GL.DeleteTexture(Handle);
        }

        #region Static
        /// <summary>
        /// Creates a texure, copies the images data into the texture and disposes of the image.
        /// </summary>
        /// <param name="image">Image to create the texture from</param>
        public static Texture2D FromImage(Image<Rgba32> image)
        {
            byte[] buffer = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(buffer);
            image.Dispose();

            Texture2D texture = new Texture2D(image.Width, image.Height);
            texture.SetData(buffer);
            return texture;
        }

        private static PixelInternalFormat _internalFormat = PixelInternalFormat.Rgba;
        private static PixelFormat _format = PixelFormat.Rgba;
        private static PixelType _type = PixelType.UnsignedByte;

        /// <summary>
        /// Change settings used to create the texture,
        /// </summary>
        /// <param name="internalFormat">Internal representation.</param>
        /// <param name="format">Input image format.</param>
        public static void SetTextureFormat(PixelInternalFormat internalFormat, PixelFormat format, PixelType type)
        {
            _internalFormat = internalFormat;
            _format = format;
            _type = type;
        }
        #endregion

        public bool Equals(Texture2D other) => Handle == other.Handle;
        public override bool Equals(object? obj) => obj is Texture2D && Equals((Texture2D)obj);
        public override int GetHashCode() => Handle.GetHashCode();

        public static bool operator ==(Texture2D a, Texture2D b) => a.Equals(b);
        public static bool operator !=(Texture2D a, Texture2D b) => !a.Equals(b);
    }
}
