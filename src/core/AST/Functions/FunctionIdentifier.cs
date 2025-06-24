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
        Move,
        Color,
        Size,
        SetCanvasColor,
        DrawLine,
        DrawCircle,
        DrawFullCircle,
        DrawRectangle,
        DrawFullRectangle,
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
