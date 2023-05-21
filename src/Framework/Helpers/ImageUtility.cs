namespace VoxelGame.Framework.Helpers
{
    // TODO: Remove this class.
    public static class ImageUtility
    {
        /// <summary>
        /// Creates a new byte buffer and copies the image data to it.
        /// </summary>
        public static byte[] CopyData(Image<Rgba32> image)
        {
            byte[] buffer = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(buffer);
            return buffer;
        }

        /// <summary>
        /// Draws the src image onto dest and disposes of src.
        /// </summary>
        /// <param name="position">The position the image is drawn at.</param>
        public static void Combine(Image<Rgba32> dest, Image<Rgba32> src, Point position, float opacity = 1f)
        {
            dest.Mutate(c => c.DrawImage(src, position, opacity));
            src.Dispose();
        }
    }
}
