using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;

namespace VoxelGame.Framework
{
    public class Resources
    {
        public const string ASSET_DIR = @"assets";

        /// <summary>
        /// Converts a relative path with / instead of \ to a fully qualified path.
        /// </summary>
        public static string ToFullyQualifiedPath(string path)
        {
            path = path.Replace('/', '\\');
            return Path.Join(Environment.CurrentDirectory, ASSET_DIR, path);
        }

        #region Summary
        /// <summary>
        /// Reads text from a file using a simplified path.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="FileNotFoundException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="System.Security.SecurityException"/>
        #endregion
        public static string ReadText(string path)
        {
            return File.ReadAllText(ToFullyQualifiedPath(path));
        }

        #region Summary
        /// <summary>
        /// Reads an image from a file using a simplified path.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="InvalidImageContentException"/>
        /// <exception cref="UnknownImageFormatException"/>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="ImageProcessingException"/>
        #endregion
        public static Image<Rgba32> ReadImage(string path)
        {
            return Image.Load<Rgba32>(ToFullyQualifiedPath(path));
        }
    }
}
