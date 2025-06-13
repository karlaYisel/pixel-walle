using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.AST.Functions;
using Core.Parser;
using Core.PixelWallE;
using Core.Utils.Error;

namespace Core.Executor
{
    public class ExecutorAnalyser : IExecutorAnalyser
    {
        private static Executor? __Executor;
        public IExecutor Executor
        {
            get
            {
                if (__Executor == null)
                {
                    __Executor = new Executor();

                    __Executor.AddSystemFunction(FunctionIdentifier.Spawn.ToString(), (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        int x = (int)args[1]!;
                        int y = (int)args[2]!;

                        obj.MoveTo(out error, x, y);

                        args[0] = error;
                        return Core.Utils.SystemClass.Void.Value;
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.Respawn.ToString(), (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        int x = (int)args[1]!;
                        int y = (int)args[2]!;

                        obj.MoveTo(out error, x, y);

                        args[0] = error;
                        return Core.Utils.SystemClass.Void.Value;
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.Color.ToString(), (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        System.Drawing.Color color = (System.Drawing.Color)args[1]!;

                        obj.SetBrushColor(out error, color);

                        args[0] = error;
                        return Core.Utils.SystemClass.Void.Value;
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.Size.ToString(), (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        int size = (int)args[1]!;

                        obj.SetSize(out error, size);

                        args[0] = error;
                        return Core.Utils.SystemClass.Void.Value;
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.DrawLine.ToString(), (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        int xDir = (int)args[1]!;
                        int yDir = (int)args[2]!;
                        int distance = (int)args[3]!;

                        obj.DrawLine(out error, xDir, yDir, distance);

                        args[0] = error;
                        return Core.Utils.SystemClass.Void.Value;
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.DrawCircle.ToString(), (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        int xDir = (int)args[1]!;
                        int yDir = (int)args[2]!;
                        int distance = (int)args[3]!;

                        obj.DrawCircle(out error, xDir, yDir, distance);

                        args[0] = error;
                        return Core.Utils.SystemClass.Void.Value;
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.DrawRectangle.ToString(), (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        int xDir = (int)args[1]!;
                        int yDir = (int)args[2]!;
                        int distance = (int)args[3]!;
                        int width = (int)args[4]!;
                        int height = (int)args[5]!;

                        obj.DrawRectangle(out error, xDir, yDir, distance, width, height);

                        args[0] = error;
                        return Core.Utils.SystemClass.Void.Value;
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.Fill.ToString(), (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];

                        obj.Fill(out error);

                        args[0] = error;
                        return Core.Utils.SystemClass.Void.Value;
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.GetActualX.ToString(), (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];

                        var result = obj.GetX(out error);

                        args[0] = error;
                        return result;
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.GetActualY.ToString(), (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];

                        var result = obj.GetY(out error);

                        args[0] = error;
                        return result;
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.GetCanvasSize.ToString(), (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];

                        var result = obj.GetCanvasSize(out error);

                        args[0] = error;
                        return result;
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.GetCanvasSizeX.ToString(), (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];

                        var result = obj.GetCanvasWidth(out error);

                        args[0] = error;
                        return result;
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.GetCanvasSizeY.ToString(), (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];

                        var result = obj.GetCanvasHeight(out error);

                        args[0] = error;
                        return result;
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.GetColorCount.ToString(), (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        System.Drawing.Color color = (System.Drawing.Color)args[1]!;
                        int x1 = (int)args[2]!;
                        int y1 = (int)args[3]!;
                        int x2 = (int)args[4]!;
                        int y2 = (int)args[5]!;

                        var result = obj.GetColorCount(out error, color, x1, x2, y1, y2);

                        args[0] = error;
                        return result;
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.IsBrushColor.ToString(), (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        System.Drawing.Color color = (System.Drawing.Color)args[1]!;

                        var result = obj.IsBrushColor(out error, color);

                        args[0] = error;
                        return result;
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.IsBrushSize.ToString(), (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        int size = (int)args[1]!;

                        var result = obj.IsBrushSize(out error, size);

                        args[0] = error;
                        return result;
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.IsCanvasColor.ToString(), (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        System.Drawing.Color color = (System.Drawing.Color)args[1]!;
                        int vertical = (int)args[2]!;
                        int horizontal = (int)args[3]!;

                        var result = obj.IsCanvasColor(out error, color, vertical, horizontal);

                        args[0] = error;
                        return result;
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.IsColor.ToString(), (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        System.Drawing.Color color = (System.Drawing.Color)args[1]!;
                        int vertical = (int)args[2]!;
                        int horizontal = (int)args[3]!;

                        var result = obj.IsCanvasColor(out error, color, vertical, horizontal);

                        args[0] = error;
                        return result;
                    });
                }

                return __Executor;
            }
        }
    }
}
