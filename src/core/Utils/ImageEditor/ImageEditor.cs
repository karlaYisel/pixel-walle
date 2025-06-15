using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Core.Utils.Error;

namespace Core.Utils.ImageEditor
{

    public class ImageEditor
    {
        private Image<Rgba32> image;

        public int Width => image.Width;
        public int Height => image.Height;

        public ImageEditor(out ExecutionError? error)
        {
            error = null;
            image = new Image<Rgba32>(64, 64, Color.White);
        }

        public byte[] GetImage()
        {
            using var ms = new MemoryStream();
            image.SaveAsPng(ms);

            byte[] colors = ms.ToArray();

            ms.Close();
            return colors;
        }

        public void SetImage(byte[] img)
        {
            image = Image.Load<Rgba32>(img);
        }

        public Rgba32 GetPixel(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return Color.Transparent;
            return image[x, y];
        }

        public void SetPixel(int x, int y, Rgba32 color)
        {
            if (!(x < 0 || y < 0 || x >= Width || y >= Height)) image[x, y] = color;
        }

        public void Dispose()
        {
            image.Dispose();
        }

        public void ImageLoad(out ExecutionError? error, Color? color = null)
        {
            error = null;
            if (color is null) color = Color.White;
            image = new Image<Rgba32>(Width, Height, (Color)color);
        }

        public void ImageLoad(out ExecutionError? error, int height = 0, int width = 0)
        {
            if (width <= 0) width = Width;
            if (height <= 0) height = Height;
            error = null;
            image = new Image<Rgba32>(width, height, Color.White);
        }
    }
}
