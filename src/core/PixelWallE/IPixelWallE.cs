using System.Drawing;
using Core.Utils.ImageEditor;
using Core.Utils.Error;

namespace Core.PixelWallE
{
    public delegate Task CanvasChanged();
    public interface IPixelWallE
    {
        Task CanvasHasChanged();
        void AddCanvasChangedListener(CanvasChanged listener);

        byte[] GetImage();

        Task SetImage(byte[] img);

        Task<ExecutionError?> ImageLoad(ExecutionError? error, int x, int y);

        Task<ExecutionError?> ImageLoad(ExecutionError? error, System.Drawing.Color color);

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

        void SetAnimationType(out ExecutionError? error, AnimationType type);

        AnimationType GetAnimationType(out ExecutionError? error);

        void SetColorType(out ExecutionError? error, ColorType type);

        Task<ExecutionError?> DrawPixel(ExecutionError? error, int? x = null, int? y = null);

        Task<ExecutionError?> DrawPoint(ExecutionError? error, int? x = null, int? y = null, HashSet<(int x, int y)>? pixels = null);

        Task<ExecutionError?> DrawLine(ExecutionError? error, int xDir, int yDir, int distance, int? x = null, int? y = null, bool changeWallEPos = true);

        Task<ExecutionError?> DrawCircle(ExecutionError? error, int xDir, int yDir, int distance, int? x = null, int? y = null, bool changeWallEPos = true);

        Task<ExecutionError?> DrawFullCircle(ExecutionError? error, int xDir, int yDir, int distance, int? x = null, int? y = null, bool changeWallEPos = true);

        Task<ExecutionError?> DrawRectangle(ExecutionError? error, int xDir, int yDir, int distance, int width, int height, int? x = null, int? y = null, bool changeWallEPos = true);

        Task<ExecutionError?> DrawFullRectangle(ExecutionError? error, int xDir, int yDir, int distance, int width, int height, int? x = null, int? y = null, bool changeWallEPos = true);

        Task<ExecutionError?> Fill(ExecutionError? error, int? x = null, int? y = null, bool changeWallEPos = true);
    }
}
