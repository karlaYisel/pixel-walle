using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils;
using Core.Utils.Error;
using Core.Utils.ImageEditor;

namespace Core.Executor
{
    public delegate Task CanvasChanged();
    public interface IExecutor
    {
        Task CanvasHasChanged();
        void AddCanvasChangedListener(CanvasChanged listener);
        void SetProgram(Program program);

        void AddSystemFunction(string identifier, Func<PixelWallE.IPixelWallE, object?[], object>? exp);

        void SetDelay(int delay);

        void SetColorType(ColorType type);

        void SetBrushType(BrushType type);

        void SetWallE(PixelWallE.IPixelWallE WallE);

        Task<ExecutionError?> ExecuteCode(ExecutionError? error, CancellationToken cancellationToken);
    }
}
