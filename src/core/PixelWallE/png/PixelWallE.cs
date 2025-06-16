using Core.Utils.ImageEditor;
using System.Drawing;
using Core.Utils.Error;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Core.PixelWallE.png
{
    public class PixelWallE : IPixelWallE
    {
        private int X;
        private int Y;
        private ImageEditor image;
        private Color brushColor;
        private int brushSize;
        private BrushType brushType;
        private ColorType colorType;
        private int delay;

        public PixelWallE(out ExecutionError? error, string? path = null)
        {
            X = Y = 0;
            image = new ImageEditor(out error);
            brushColor = Color.FromArgb(0);
            brushSize = 1;
            brushType = BrushType.Square;
            colorType = ColorType.Solid;
        }

        public void SetDelay(int delay)
        {
            this.delay = delay;
        }

        public byte[] GetImage()
        {
            return image.GetImage();
        }

        public void SetImage(byte[] img)
        {
            image.SetImage(img);
        }

        public void ImageLoad(out ExecutionError? error, int x, int y)
        {
            image.ImageLoad(out error, x, y);
        }

        public void ImageLoad(out ExecutionError? error)
        {
            image.ImageLoad(out error, 0, 0);
        }

        public void MoveTo(out ExecutionError? error, int x, int y)
        {
            if (!IsInCanvas(out error, x, y)) return;
            X = x;
            Y = y;
        }

        public bool IsInCanvas(out ExecutionError? error, int x, int y)
        {
            error = null;
            if (x < 0 || x >= image.Width || y < 0 || y >= image.Height)
            {
                error = new ExecutionError(ErrorCode.OutOfBounds, $"Coordinates ({x}, {y}) are out of bounds.");
                return false;
            }
            return true;
        }

        public void Move(out ExecutionError? error, int x, int y)
        {
            MoveTo(out error, X + x, Y + y);
        }

        public int GetCanvasSize(out ExecutionError? error)
        {
            error = null;
            return Math.Max(image.Width, image.Height);
        }

        public int GetCanvasWidth(out ExecutionError? error)
        {
            error = null;
            return image.Width;
        }

        public int GetCanvasHeight(out ExecutionError? error)
        {
            error = null;
            return image.Height;
        }

        public Color GetCanvasColor(out ExecutionError? error, int? x = null, int? y = null)
        {
            error = null;
            if (x is null) x = X;
            if (y is null) y = Y;
            IsInCanvas(out error, (int)x, (int)y);
            SixLabors.ImageSharp.PixelFormats.Rgba32 color = image.GetPixel((int)x, (int)y);
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public bool IsCanvasColor(out ExecutionError? error, Color color, int vertical, int horizontal)
        {
            error = null;
            if (!IsInCanvas(out error, X + horizontal, Y + vertical)) return false;
            SixLabors.ImageSharp.PixelFormats.Rgba32 pixelColor = image.GetPixel(X + horizontal, Y + vertical);
            return Color.FromArgb(color.A, color.R, color.G, color.B).ToArgb() == color.ToArgb();
        }

        public int GetColorCount(out ExecutionError? error, Color color, int x1, int x2, int y1, int y2)
        {
            error = null;
            if (!IsInCanvas(out error, (int)x1, (int)y1) || !IsInCanvas(out error, (int)x2, (int)y2)) return 0;
            int count = 0;
            for (int i = Math.Min(x1, x2); i <= Math.Max(x1, x2); i++)
            {
                for (int j = Math.Min(y1, y2); j <= Math.Max(y1, y2); j++)
                {
                    if (IsCanvasColor(color, j, i)) count++;
                }
            }
            return count;
        }

        private bool IsCanvasColor(Color color, int x, int y)
        {
            if (!IsInCanvas(out ExecutionError? error, x, y)) return false;
            SixLabors.ImageSharp.PixelFormats.Rgba32 pixelColor = image.GetPixel(x, y);
            return Color.FromArgb(color.A, color.R, color.G, color.B).ToArgb() == color.ToArgb();
        }

        public int GetX(out ExecutionError? error)
        {
            error = null;
            return X;
        }

        public int GetY(out ExecutionError? error)
        {
            error = null;
            return Y;
        }

        public void SetBrushColor(out ExecutionError? error, Color color)
        {
            error = null;
            brushColor = color;
        }

        public Color GetBrushColor(out ExecutionError? error)
        {
            error = null;
            return brushColor;
        }

        public bool IsBrushColor(out ExecutionError? error, Color color)
        {
            error = null;
            return brushColor.ToArgb() == color.ToArgb();
        }

        public void SetSize(out ExecutionError? error, int size)
        {
            error = null;
            brushSize = (size / 2) * 2 + 1;
        }

        public int GetSize(out ExecutionError? error)
        {
            error = null;
            return brushSize;
        }

        public bool IsBrushSize(out ExecutionError? error, int size)
        {
            error = null;
            return brushSize == size;
        }

        public BrushType GetBrushType(out ExecutionError? error)
        {
            error = null;
            return brushType;
        }

        public void SetBrushType(out ExecutionError? error, BrushType type)
        {
            error = null;
            brushType = type;
        }

        public ColorType GetColorType(out ExecutionError? error)
        {
            error = null;
            return colorType;
        }

        public void SetColorType(out ExecutionError? error, ColorType type)
        {
            error = null;
            colorType = type;
        }

        public void DrawPixel(out ExecutionError? error, int? x = null, int? y = null)
        {
            error = null;
            if (x is null) x = X;
            if (y is null) y = Y;
            if (!IsInCanvas(out error, (int)x, (int)y)) return;
            Color color = Color.FromArgb(0);
            switch(colorType)
            {
                case ColorType.Solid:
                    color = GetSolidColor((int)x, (int)y);
                    break;
                case ColorType.WaterColor:
                    color = GetWaterColor((int)x, (int)y);
                    break;
                default:
                    error = new ExecutionError(ErrorCode.ColorTypeNotDefined, $"Unknow color type.");
                    return;
            }
            image.SetPixel((int)x, (int)y, new SixLabors.ImageSharp.PixelFormats.Rgba32(color.R, color.G, color.B, color.A));
        }

        private Color GetSolidColor(int x, int y)
        {
            Color color = GetCanvasColor(out ExecutionError? error, x, y);
            if (error is not null) return Color.FromArgb(0);
            return Color.FromArgb(255, (brushColor.A*brushColor.R + (255 - brushColor.A)*color.R)/255, (brushColor.A * brushColor.G + (255 - brushColor.A) * color.G) / 255, (brushColor.A * brushColor.B + (255 - brushColor.A) * color.B) / 255);
        }

        private Color GetWaterColor(int x, int y)
        {
            Color color = GetCanvasColor(out ExecutionError? error, x, y);
            if (error is not null) return Color.FromArgb(0);
            return Color.FromArgb((brushColor.A + color.A)/2, (brushColor.A * brushColor.R + color.A * color.R) / (brushColor.A + color.A), (brushColor.A * brushColor.G + color.A * color.G) / (brushColor.A + color.A), (brushColor.A * brushColor.B + color.A * color.B) / (brushColor.A + color.A));
        }

        public void DrawPoint(out ExecutionError? error, int? x = null, int? y = null, List<(int x, int y)>? pixels = null)
        {
            error = null;
            if (x is null) x = X;
            if (y is null) y = Y;
            if (pixels is null) pixels = new List<(int x, int y)>();

            List<(int x, int y)> point;
            switch (brushType)
            {
                case BrushType.Square:
                    point = GetSquare((int)x, (int)y);
                    break;
                case BrushType.Circle:GetCircle((int)x, (int)y);
                    point = GetCircle((int)x, (int)y);
                    break;
                default:
                    error = new ExecutionError(ErrorCode.ColorTypeNotDefined, $"Unknow brush type.");
                    return;
            }
            foreach (var pixel in point)
            {
                if(!pixels.Contains(pixel))
                {
                    pixels.Add(pixel);
                    DrawPixel(out error, pixel.x, pixel.y);
                }
            }
        }

        private List<(int, int)> GetSquare(int x, int y)
        { 
            List<(int x, int y)> pixels = new List<(int x, int y)>();

            for (int i = x - brushSize/2; i <= x + brushSize/2; i++)
            {
                for (int j = y - brushSize / 2; j <= y + brushSize / 2; j++)
                {
                    if (IsInCanvas(out ExecutionError? error, i, j)) pixels.Add((i, j));
                }
            }

            return pixels;
        }

        private List<(int, int)> GetCircle(int x, int y)
        {
            List<(int x, int y)> pixels = new List<(int x, int y)>();

            int[] X = { 0, 0, 1, -1 };
            int[] Y = { 1, -1, 0, 0 };
            List<(int x, int y)> innerPixels = new List<(int x, int y)>();
            Queue<(int x, int y)> positions = new Queue<(int x, int y)>();
            positions.Enqueue((x, y));
            innerPixels.Add((x, y));
            pixels.Add((x, y));

            (int x, int y) pos;
            while (positions.Count > 0)
            {
                pos = positions.Dequeue();

                if(((pos.x - x)^2 + (pos.y - y)^2) < ((brushSize/2)^2))
                { 
                    for(int i = 0; i < X.Length; i++)
                    {
                        if(!innerPixels.Contains((pos.x + X[i], pos.y + Y[i])))
                        {
                            innerPixels.Add((pos.x + X[i], pos.y + Y[i]));
                            positions.Enqueue((pos.x + X[i], pos.y + Y[i]));
                            if (IsInCanvas(out ExecutionError? error, pos.x + X[i], pos.y + Y[i])) pixels.Add((pos.x + X[i], pos.y + Y[i]));
                        }
                    }
                }
            }

            return pixels;
        }

        public void DrawLine(out ExecutionError? error, int xDir, int yDir, int distance, int? x = null, int? y = null, bool changeWallEPos = true)
        {
            error = null;
            if (x is null) x = X;
            if (y is null) y = Y;
            List<(int x, int y)> positions;
            GetDir(ref xDir, ref yDir, out positions);

            (int x, int y) actualPosition = ((int)x, (int)y);

            List<(int x, int y)> pixels = new List<(int x, int y)> ();
            for (int i = 0; i < distance; i++)
            {
                DrawPoint(out error, actualPosition.x + positions[i % positions.Count].x, actualPosition.y + positions[i % positions.Count].y, pixels);
                if ((i + 1) % positions.Count == 0) actualPosition = (actualPosition.x + xDir, actualPosition.y + yDir);
                Thread.Sleep(delay);
            }
            error = null;
            if (distance % positions.Count != 0) actualPosition = (actualPosition.x + positions[distance % positions.Count].x, actualPosition.y + positions[distance % positions.Count].y);
            if (changeWallEPos) MoveTo(out error, actualPosition.x, actualPosition.y);
        }

        public void DrawCircle(out ExecutionError? error, int xDir, int yDir, int distance, int? x = null, int? y = null, bool changeWallEPos = true)
        {
            error = null;
            if (x is null) x = this.X;
            if (y is null) y = this.Y;
            List<(int x, int y)> circle = new List<(int x, int y)>();

            int[] X = { 0, 0, 1, -1 };
            int[] Y = { 1, -1, 0, 0 };
            List<(int x, int y)> innerPixels = new List<(int x, int y)>();
            Queue<(int x, int y)> positions = new Queue<(int x, int y)>();

            List<(int x, int y)> distances;
            GetDir(ref xDir, ref yDir, out distances);
            x = x + xDir *(distance/distances.Count) + distances[distance%distances.Count].x;
            y = y + yDir * (distance / distances.Count) + distances[distance % distances.Count].y;

            positions.Enqueue(((int)x, (int)y));
            innerPixels.Add(((int)x, (int)y));

            (int x, int y) pos;
            while (positions.Count > 0)
            {
                pos = positions.Dequeue();

                if (((pos.x - x) * (pos.x - x) + (pos.y - y) * (pos.y - y)) < (distance * distance))
                {
                    for (int i = 0; i < X.Length; i++)
                    {
                        if (!innerPixels.Contains((pos.x + X[i], pos.y + Y[i])))
                        {
                            innerPixels.Add((pos.x + X[i], pos.y + Y[i]));
                            positions.Enqueue((pos.x + X[i], pos.y + Y[i]));
                        }
                    }
                }
                else circle.Add((pos.x, pos.y));
            }

            List<(int x, int y)> pixels = new List<(int x, int y)>();
            foreach (var point in circle)
            {
                DrawPoint(out error, point.x, point.y, pixels);
                Thread.Sleep(delay);
            }

            error = null;
            if (changeWallEPos) MoveTo(out error, (int)x, (int)y);
        }

        public void DrawRectangle(out ExecutionError? error, int xDir, int yDir, int distance, int width, int height, int? x = null, int? y = null, bool changeWallEPos = true)
        {
            error = null;
            if (x is null) x = X;
            if (y is null) y = Y;
            List<(int x, int y)> rectangle = new List<(int x, int y)>();

            List<(int x, int y)> distances;
            GetDir(ref xDir, ref yDir, out distances);
            x = x + xDir * (distance / distances.Count) + distances[distance % distances.Count].x;
            y = y + yDir * (distance / distances.Count) + distances[distance % distances.Count].y;

            width = width / 2 * 2 + 3;
            height = height / 2 * 2 + 3;

            for (int i = (int)y - height / 2; i <= (int)y + height / 2; i++)
            {
                for (int j = (int)x - width / 2; j <= (int)x + width / 2; j++)
                {
                    if (i == y - height /2 || i == y + height /2 || j == x - width /2 || j == x + width /2) rectangle.Add((j, i));
                }
            }

            List<(int x, int y)> pixels = new List<(int x, int y)>();
            foreach (var point in rectangle)
            {
                DrawPoint(out error, point.x, point.y, pixels);
                Thread.Sleep(delay);
            }

            error = null;
            if (changeWallEPos) MoveTo(out error, (int)x, (int)y);
        }

        public void Fill(out ExecutionError? error, int? x = null, int? y = null, bool changeWallEPos = true)
        {
            error = null;
            if (x is null) x = this.X;
            if (y is null) y = this.Y;

            int targetColor = GetCanvasColor(out error, (int)x, (int)y).ToArgb();
            if (error is not null) return;

            int[] X = { 0, 0, 1, -1 };
            int[] Y = { 1, -1, 0, 0 };

            HashSet<(int x, int y)> visited = new HashSet<(int x, int y)>();
            List<(int x, int y)> fillPixels = new List<(int x, int y)>();
            Queue<(int x, int y)> positions = new Queue<(int x, int y)>();

            positions.Enqueue(((int)x, (int)y));

            while (positions.Count > 0)
            {
                var pos = positions.Dequeue();

                if (visited.Contains(pos) || !IsInCanvas(out error, pos.x, pos.y))
                    continue;

                visited.Add(pos);

                if (GetCanvasColor(out error, pos.x, pos.y).ToArgb() == targetColor && error is null)
                {
                    fillPixels.Add(pos);

                    for (int i = 0; i < X.Length; i++)
                    {
                        var newPos = (pos.x + X[i], pos.y + Y[i]);
                        if (!visited.Contains(newPos))
                        {
                            positions.Enqueue(newPos);
                        }
                    }
                }
            }

            foreach (var point in fillPixels)
            {
                DrawPixel(out error, point.x, point.y);
                if (error is not null) break;
                Thread.Sleep(delay);
            }

            if (changeWallEPos && error is null)
                MoveTo(out error, (int)x, (int)y);
        }

        private void GetDir(ref int x, ref int y, out List<(int x, int y)> positions)
        {
            positions = new List<(int x, int y)>();
            int d = (int)BigInteger.GreatestCommonDivisor((int)Math.CopySign(x, 1), (int)Math.CopySign(y, 1));
            if (d == 0)
            {
                positions.Add((x, y));
                return;
            }
            x = x / d;
            y = y / d;

            (int x, int y) pos;
            d = Math.Max((int)Math.CopySign(x, 1), (int)Math.CopySign(y, 1));
            for (int i = 0; i < d; i++)
            {
                pos = (i*x/d, i*y/d);
                positions.Add(pos);
            }
        }
    }
}
