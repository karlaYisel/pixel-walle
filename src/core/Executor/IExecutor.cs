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
    public interface IExecutor
    {
        void SetProgram(Program program);

        void AddSystemFunction(string identifier, Func<PixelWallE.IPixelWallE, object?[], object>? exp);

        void SetDelay(int delay);

        void SetColorType(ColorType type);

        void SetBrushType(BrushType type);

        void SetWallE(PixelWallE.IPixelWallE WallE);

        void ExecuteCode(out ExecutionError? error, CancellationToken cancellationToken);
    }
}
