using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.AST.Functions
{
    public enum FunctionIdentifier
    {
        Spawn,
        Respawn,
        Color,
        Size,
        DrawLine,
        DrawCircle,
        DrawRectangle,
        Fill,
        GetActualX,
        GetActualY,
        GetCanvasSize,
        GetCanvasSizeX,
        GetCanvasSizeY,
        GetColorCount,
        IsBrushColor,
        IsBrushSize,
        GetBrushSize,
        IsCanvasColor,
        IsColor
    }
}
