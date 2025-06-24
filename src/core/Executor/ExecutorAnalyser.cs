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
using Core.Utils.SystemClass;

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

                    __Executor.AddSystemFunction(FunctionIdentifier.SetCanvasColor.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        System.Drawing.Color color = (System.Drawing.Color)args[1]!;

                        error = await obj.ImageLoad(error, color);

                        args[0] = error;
                        return (error, Core.Utils.SystemClass.Void.Value);
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.Spawn.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        int x = (args[1] is IntegerOrBool ib1) ? ib1 : (int)args[1]!;
                        int y = (args[2] is IntegerOrBool ib2) ? ib2 : (int)args[2]!;

                        obj.MoveTo(out error, x, y);
                        await Task.CompletedTask;

                        args[0] = error;
                        return (error, Core.Utils.SystemClass.Void.Value);
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.Respawn.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        int x = (args[1] is IntegerOrBool ib1) ? ib1 : (int)args[1]!;
                        int y = (args[2] is IntegerOrBool ib2) ? ib2 : (int)args[2]!;

                        obj.MoveTo(out error, x, y);
                        await Task.CompletedTask;

                        args[0] = error;
                        return (error, Core.Utils.SystemClass.Void.Value);
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.Move.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        int x = (args[1] is IntegerOrBool ib1) ? ib1 : (int)args[1]!;
                        int y = (args[2] is IntegerOrBool ib2) ? ib2 : (int)args[2]!;

                        obj.Move(out error, x, y);
                        await Task.CompletedTask;

                        args[0] = error;
                        return (error, Core.Utils.SystemClass.Void.Value);
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.Color.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        System.Drawing.Color color = (System.Drawing.Color)args[1]!;

                        obj.SetBrushColor(out error, color);
                        await Task.CompletedTask;

                        args[0] = error;
                        return (error, Core.Utils.SystemClass.Void.Value);
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.Size.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        int size = (args[1] is IntegerOrBool ib1) ? ib1 : (int)args[1]!;

                        obj.SetSize(out error, size);
                        await Task.CompletedTask;

                        args[0] = error;
                        return (error, Core.Utils.SystemClass.Void.Value);
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.DrawLine.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        int xDir = (args[1] is IntegerOrBool ib1) ? ib1 : (int)args[1]!;
                        int yDir = (args[2] is IntegerOrBool ib2) ? ib2 : (int)args[2]!;
                        int distance = (args[3] is IntegerOrBool ib3) ? ib3 : (int)args[3]!;

                        error = await obj.DrawLine(error, xDir, yDir, distance);

                        args[0] = error;
                        return (error, Core.Utils.SystemClass.Void.Value);
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.DrawCircle.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        int xDir = (args[1] is IntegerOrBool ib1) ? ib1 : (int)args[1]!;
                        int yDir = (args[2] is IntegerOrBool ib2) ? ib2 : (int)args[2]!;
                        int distance = (args[3] is IntegerOrBool ib3) ? ib3 : (int)args[3]!;

                        error = await obj.DrawCircle(error, xDir, yDir, distance);

                        args[0] = error;
                        return (error, Core.Utils.SystemClass.Void.Value);
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.DrawFullCircle.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        int xDir = (args[1] is IntegerOrBool ib1) ? ib1 : (int)args[1]!;
                        int yDir = (args[2] is IntegerOrBool ib2) ? ib2 : (int)args[2]!;
                        int distance = (args[3] is IntegerOrBool ib3) ? ib3 : (int)args[3]!;

                        error = await obj.DrawFullCircle(error, xDir, yDir, distance);

                        args[0] = error;
                        return (error, Core.Utils.SystemClass.Void.Value);
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.DrawRectangle.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        int xDir = (args[1] is IntegerOrBool ib1) ? ib1 : (int)args[1]!;
                        int yDir = (args[2] is IntegerOrBool ib2) ? ib2 : (int)args[2]!;
                        int distance = (args[3] is IntegerOrBool ib3) ? ib3 : (int)args[3]!;
                        int width = (args[4] is IntegerOrBool ib4) ? ib4 : (int)args[4]!;
                        int height = (args[5] is IntegerOrBool ib5) ? ib5 : (int)args[5]!;

                        error = await obj.DrawRectangle(error, xDir, yDir, distance, width, height);

                        args[0] = error;
                        return (error, Core.Utils.SystemClass.Void.Value);
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.DrawFullRectangle.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        int xDir = (args[1] is IntegerOrBool ib1) ? ib1 : (int)args[1]!;
                        int yDir = (args[2] is IntegerOrBool ib2) ? ib2 : (int)args[2]!;
                        int distance = (args[3] is IntegerOrBool ib3) ? ib3 : (int)args[3]!;
                        int width = (args[4] is IntegerOrBool ib4) ? ib4 : (int)args[4]!;
                        int height = (args[5] is IntegerOrBool ib5) ? ib5 : (int)args[5]!;

                        error = await obj.DrawFullRectangle(error, xDir, yDir, distance, width, height);

                        args[0] = error;
                        return (error, Core.Utils.SystemClass.Void.Value);
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.Fill.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];

                        error = await obj.Fill(error);

                        args[0] = error;
                        return (error, Core.Utils.SystemClass.Void.Value);
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.GetActualX.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];

                        var result = obj.GetX(out error);
                        await Task.CompletedTask;

                        args[0] = error;
                        return (error, result);
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.GetActualY.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];

                        var result = obj.GetY(out error);
                        await Task.CompletedTask;

                        args[0] = error;
                        return (error, result);
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.GetCanvasSize.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];

                        var result = obj.GetCanvasSize(out error);
                        await Task.CompletedTask;

                        args[0] = error;
                        return (error, result);
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.GetCanvasSizeX.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];

                        var result = obj.GetCanvasWidth(out error);
                        await Task.CompletedTask;

                        args[0] = error;
                        return (error, result);
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.GetCanvasSizeY.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];

                        var result = obj.GetCanvasHeight(out error);
                        await Task.CompletedTask;

                        args[0] = error;
                        return (error, result);
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.GetColorCount.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        System.Drawing.Color color = (System.Drawing.Color)args[1]!;
                        int x1 = (args[2] is IntegerOrBool ib2) ? ib2 : (int)args[2]!;
                        int y1 = (args[3] is IntegerOrBool ib3) ? ib3 : (int)args[3]!;
                        int x2 = (args[4] is IntegerOrBool ib4) ? ib4 : (int)args[4]!;
                        int y2 = (args[5] is IntegerOrBool ib5) ? ib5 : (int)args[5]!;

                        var result = obj.GetColorCount(out error, color, x1, x2, y1, y2);
                        await Task.CompletedTask;

                        args[0] = error;
                        return (error, result);
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.IsBrushColor.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        System.Drawing.Color color = (System.Drawing.Color)args[1]!;

                        var result = obj.IsBrushColor(out error, color);
                        await Task.CompletedTask;

                        args[0] = error;
                        return (error, result);
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.IsBrushSize.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        int size = (args[1] is IntegerOrBool ib1) ? ib1 : (int)args[1]!;

                        var result = obj.IsBrushSize(out error, size);
                        await Task.CompletedTask;

                        args[0] = error;
                        return (error, result);
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.IsCanvasColor.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        System.Drawing.Color color = (System.Drawing.Color)args[1]!;
                        int vertical = (args[2] is IntegerOrBool ib2) ? ib2 : (int)args[2]!;
                        int horizontal = (args[3] is IntegerOrBool ib3) ? ib3 : (int)args[3]!;

                        var result = obj.IsCanvasColor(out error, color, vertical, horizontal);
                        await Task.CompletedTask;

                        args[0] = error;
                        return (error, result);
                    });
                    __Executor.AddSystemFunction(FunctionIdentifier.IsColor.ToString(), async (obj, args) =>
                    {
                        var error = (ExecutionError?)args[0];
                        System.Drawing.Color color = (System.Drawing.Color)args[1]!;
                        int vertical = (args[2] is IntegerOrBool ib2) ? ib2 : (int)args[2]!;
                        int horizontal = (args[3] is IntegerOrBool ib3) ? ib3 : (int)args[3]!;

                        var result = obj.IsCanvasColor(out error, color, vertical, horizontal);
                        await Task.CompletedTask;

                        args[0] = error;
                        return (error, result);
                    });
                }

                return __Executor;
            }
        }
    }
}
