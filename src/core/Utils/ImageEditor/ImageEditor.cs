using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Core.Utils.Error;

namespace Core.Utils.ImageEditor
{

    public class ImageEditor
    {
        private Image<Rgba32> image;
        private string path;

        public int Width => image.Width;
        public int Height => image.Height;

        public ImageEditor(out ExecutionError? error, string? inputPath = null)
        {
            error = null;
            path = inputPath ?? "img.png";

            if (!File.Exists(path))
            {
                image = new Image<Rgba32>(64, 64, Color.White);
                image.Save(path); 
            }
            try
            {
                image = Image.Load<Rgba32>(path);
            }
            catch (UnauthorizedAccessException)
            {
                error = new ExecutionError(ErrorCode.UnauthorizedAccessException, "Access denied");
                image = new Image<Rgba32>(64, 64, Color.White);
            }
            catch (InvalidImageContentException)
            {
                error = new ExecutionError(ErrorCode.InvalidImageContentException, "Non valid image or corrupted file");
                image = new Image<Rgba32>(64, 64, Color.White);
            }
            catch (IOException ex)
            {
                error = new ExecutionError(ErrorCode.IOException, $"I/O Error: {ex.Message}");
                image = new Image<Rgba32>(64, 64, Color.White);
            }
            catch (Exception ex)
            {
                error = new ExecutionError(ErrorCode.UnexpectedError, $"Unexpected Error: {ex.Message}");
                image = new Image<Rgba32>(64, 64, Color.White);
            }
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

        public void Save(out ExecutionError? error, string path = "")
        {
            error = null;
            string savePath = string.IsNullOrWhiteSpace(path) ? this.path : path;

            try
            {
                var dir = Path.GetDirectoryName(savePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                image.Save(savePath);
                this.path = savePath;
            }
            catch (Exception ex)
            {
                error = new ExecutionError(ErrorCode.UnexpectedError, $"[Error] Image not saved: {ex.Message}");
            }
        }

        public void Dispose()
        {
            image.Dispose();
        }

        public void ImageLoad(out ExecutionError? error, string? inputPath = null)
        {
            error = null;
            path = inputPath ?? "img.png";

            if (!File.Exists(path))
            {
                image = new Image<Rgba32>(64, 64, Color.White);
                image.Save(path);
            }
            try
            {
                image = Image.Load<Rgba32>(path);
            }
            catch (UnauthorizedAccessException)
            {
                error = new ExecutionError(ErrorCode.UnauthorizedAccessException, "Access denied");
                image = new Image<Rgba32>(64, 64, Color.White);
            }
            catch (InvalidImageContentException)
            {
                error = new ExecutionError(ErrorCode.InvalidImageContentException, "Non valid image or corrupted file");
                image = new Image<Rgba32>(64, 64, Color.White);
            }
            catch (IOException ex)
            {
                error = new ExecutionError(ErrorCode.IOException, $"I/O Error: {ex.Message}");
                image = new Image<Rgba32>(64, 64, Color.White);
            }
            catch (Exception ex)
            {
                error = new ExecutionError(ErrorCode.UnexpectedError, $"Unexpected Error: {ex.Message}");
                image = new Image<Rgba32>(64, 64, Color.White);
            }
        }

        public void ImageLoad(out ExecutionError? error, Color? color = null)
        {
            error = null;
            if (color is null) color = Color.White;

            try
            {
                image = new Image<Rgba32>(Width, Height, (Color)color);
            }
            catch (Exception ex)
            {
                error = new ExecutionError(ErrorCode.UnexpectedError, $"Unexpected Error: {ex.Message}");
            }
        }

        public void ImageLoad(out ExecutionError? error, int height = 0, int width = 0)
        {
            if (width <= 0) width = Width;
            if (height <= 0) height = Height;
            error = null;
            path = "img.png";

            if (!File.Exists(path))
            {
                image = new Image<Rgba32>(width, height, Color.White);
                image.Save(path);
            }
            try
            {
                image = Image.Load<Rgba32>(path);
            }
            catch (Exception ex)
            {
                error = new ExecutionError(ErrorCode.UnexpectedError, $"Unexpected Error: {ex.Message}");
                image = new Image<Rgba32>(64, 64, Color.White);
            }
        }

        public bool IsDefaultPath()
        {
            return path.Equals("img.png");
        }
    }
}
