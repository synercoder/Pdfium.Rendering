namespace Pdfium.Rendering
{
    /// <summary>
    /// A render of a pdf page
    /// </summary>
    public class Render
    {
        internal Render(byte[] data, int width, int height)
        {
            Data = data;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Image data, 4 bytes per pixel, in format bgra
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// The width of the image represented by <see cref="Data"/>
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// The height of the image represented by <see cref="Data"/>
        /// </summary>
        public int Height { get; }
    }
}
