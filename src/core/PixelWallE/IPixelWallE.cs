using System.Drawing;
using Core.Utils.ImageEditor;
using Core.Utils.Error;

namespace Core.PixelWallE
{
    public interface IPixelWallE
    {
        void SetDelay(int delay);

        void Save(out ExecutionError? error, string path = "");

        void ImageLoad(out ExecutionError? error, string? inputPath = null);

        void ImageLoad(out ExecutionError? error, int x, int y);

        bool IsDefaultPath(out ExecutionError? error);

        void MoveTo(out ExecutionError? error, int x, int y);

        bool IsInCanvas(out ExecutionError? error, int x, int y);

        void Move(out ExecutionError? error, int x, int y);

        int GetCanvasSize(out ExecutionError? error);

        int GetCanvasWidth(out ExecutionError? error);

        int GetCanvasHeight(out ExecutionError? error);

        Color GetCanvasColor(out ExecutionError? error, int? x = null, int? y = null);

        bool IsCanvasColor(out ExecutionError? error, Color color, int vertical, int horizontal);

        int GetColorCount(out ExecutionError? error, Color color, int x1, int x2, int y1, int y2);

        int GetX(out ExecutionError? error);

        int GetY(out ExecutionError? error);

        void SetBrushColor(out ExecutionError? error, Color color);

        Color GetBrushColor(out ExecutionError? error);

        bool IsBrushColor(out ExecutionError? error, Color color);

        void SetSize(out ExecutionError? error, int size);

        int GetSize(out ExecutionError? error);

        bool IsBrushSize(out ExecutionError? error, int size);

        BrushType GetBrushType(out ExecutionError? error);

        void SetBrushType(out ExecutionError? error, BrushType type);

        ColorType GetColorType(out ExecutionError? error);

        void SetColorType(out ExecutionError? error, ColorType type);

        void DrawPixel(out ExecutionError? error, int? x = null, int? y = null);

        void DrawPoint(out ExecutionError? error, int? x = null, int? y = null, List<(int x, int y)>? pixels = null);

        void DrawLine(out ExecutionError? error, int xDir, int yDir, int distance, int? x = null, int? y = null, bool changeWallEPos = true);

        void DrawCircle(out ExecutionError? error, int xDir, int yDir, int distance, int? x = null, int? y = null, bool changeWallEPos = true);

        void DrawRectangle(out ExecutionError? error, int xDir, int yDir, int distance, int width, int height, int? x = null, int? y = null, bool changeWallEPos = true);

        void Fill(out ExecutionError? error, int? x = null, int? y = null, bool changeWallEPos = true);
    }
}
