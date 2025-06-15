using Core.AST;
using Core.AST.Expressions;
using Core.AST.Expressions.Atom;
using Core.AST.Functions;
using Core.Utils;
using Core.Utils.Error;
using Core.Utils.SystemClass;
using Core.Utils.TokenSystem;
using Core.Utils.ImageEditor;
using System;

namespace Core.Executor
{
    public class Executor : IExecutor
    {
        private Program? _program;
        private PixelWallE.IPixelWallE? _wall; 
        private Dictionary<string, Func<PixelWallE.IPixelWallE, object?[], object>> SystemFunctions;
        private CanvasChanged? _canvasChanged;

        public Executor()
        {
            SystemFunctions = new Dictionary<string, Func<PixelWallE.IPixelWallE, object?[], object>>();
        }

        public void SetProgram(Program program)
        {
            _program = program;
        }

        public void AddSystemFunction(string identifier, Func<PixelWallE.IPixelWallE, object?[], object>? exp)
        {
            if (exp is not null) SystemFunctions.Add(identifier, exp);
        }

        public void SetDelay(int delay)
        {
            if (_wall is not null) _wall.SetDelay(delay);
        }

        public void SetColorType(ColorType type)
        {
            ExecutionError? er;
            if (_wall is not null) _wall.SetColorType(out er, type);
        }

        public void SetBrushType(BrushType type)
        {
            ExecutionError? er;
            if (_wall is not null) _wall.SetBrushType(out er, type);
        }

        public void SetWallE(PixelWallE.IPixelWallE WallE)
        {
            _wall = WallE;
        }

        public async Task<ExecutionError?> ExecuteCode(ExecutionError? error, CancellationToken cancellationToken)
        {
            error = null;
            if (_program is null)
            {
                error = new ExecutionError(ErrorCode.ProgramNotFound, "Program not found");
                return error;
            }
            if (_wall is null)
            {
                error = new ExecutionError(ErrorCode.WallENotAssigned, "WallE image editor not found");
                return error;
            }

            _wall.SetBrushColor(out error, System.Drawing.Color.Transparent);
            if (error is not null) return error;
            _wall.SetSize(out error, 1);
            if (error is not null) return error;

            ProgramAST code = _program.Code;
            code.MoveTo(0);

            while (code.CanLookAhead())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    error = new ExecutionError(ErrorCode.ExecutionCancelled, "Execution was cancelled");
                    return error;
                }

                ASTNodeBase node = code.LookAhead();

                if (node is Assign asg)
                {
                    error = await ExecuteAssign(error, asg, cancellationToken);
                }

                else if (node is Function func)
                {
                    object? r = null;
                    error = (await ExecuteFunction(error, func, r, cancellationToken)).Item1; ;
                }

                else if (node is GoTo goTo)
                {
                    var go = await ExecuteGoTo(error, goTo, code, cancellationToken);
                    error = go.Item2;
                    if (go.Item1)
                    {
                        await CanvasHasChanged();
                        continue;
                    }
                }

                else if (node is Return ret)
                {
                    object? result = null;
                    var r = await ExecuteReturn(error, ret, result, cancellationToken);
                    result = r.Item2;
                    error = r.Item1;
                    _program.Context.ExitScope();
                    if (result is not null)
                    {
                        error = new ExecutionError(ErrorCode.InvalidReturnType, "Return type not void");
                    }
                    await CanvasHasChanged();
                    return error;
                }

                if (error is not null)
                {
                    _program.Context.ExitScope();
                    await CanvasHasChanged();
                    return error;
                }

                await CanvasHasChanged();
                code.MoveNext(1);
            }
            return error;
        }

        private async Task<(object, ExecutionError?)> ExecuteScript(ExecutionError? error, Script script, Expression[] Argumnets, CancellationToken cancellationToken) 
        {
            error = null;
            if (_program is null)
            {
                error = new ExecutionError(ErrorCode.ProgramNotFound, "Program not found");
                return (Core.Utils.SystemClass.Void.Value, error);
            }
            if (_wall is null)
            {
                error = new ExecutionError(ErrorCode.WallENotAssigned, "WallE image editor not found");
                return (Core.Utils.SystemClass.Void.Value, error);
            }

            script = script.ShallowCopy();
            script.MoveTo(0);
            if (script.ArgumentNames.Length != Argumnets.Length)
            {
                error = new ExecutionError(ErrorCode.InvalidFunctionArgumentsCount, "Function Argumnet Count");
                return (Core.Utils.SystemClass.Void.Value, error);
            }
            _program.Context.EnterScope(script.GetVariablesTypes());
            for (int i = 0; i < script.ArgumentNames.Length; i++)
            {
                var r = await EvaluateExpression(Argumnets[i], error, cancellationToken);
                error = r.Item2;
                if (!_program.Context.CurrentScope.SetVariable(script.ArgumentNames[i], r.Item1))
                {
                    error = new ExecutionError(ErrorCode.InvalidFunctionArguments, "Function Argumnet Type");
                    _program.Context.ExitScope();
                    return (Core.Utils.SystemClass.Void.Value, error);
                }
                if (error is not null)
                {
                    return (Core.Utils.SystemClass.Void.Value, error);
                }
            }

            while (script.CanLookAhead())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    error = new ExecutionError(ErrorCode.ExecutionCancelled, "Execution was cancelled");
                    return (Core.Utils.SystemClass.Void.Value, error);
                }

                ASTNodeBase node = script.LookAhead();

                if (node is Assign asg)
                {
                    error = await ExecuteAssign(error, asg, cancellationToken);
                }

                else if (node is Function func)
                {
                    object? r = null;
                    error = (await ExecuteFunction(error, func, r, cancellationToken)).Item1;
                }

                else if (node is GoTo goTo)
                {
                    var go = await ExecuteGoTo(error, goTo, script, cancellationToken);
                    error = go.Item2;
                    if (go.Item1)
                    {
                        await CanvasHasChanged();
                        continue;
                    }
                }

                else if (node is Return ret)
                {
                    object? result = null;
                    var r = await ExecuteReturn(error, ret, result, cancellationToken);
                    result = r.Item2;
                    error = r.Item1;
                    _program.Context.ExitScope();
                    if (result is null || result is Core.Utils.SystemClass.Void)
                    {
                        return (Core.Utils.SystemClass.Void.Value, error);
                    }
                    if (script.ReturnType == typeof(int)) return ((int)(IntegerOrBool)result, error);
                    if (script.ReturnType == typeof(bool)) return ((bool)(IntegerOrBool)result, error);
                    if (script.ReturnType == typeof(System.Drawing.Color)) return ((System.Drawing.Color)result, error);
                    if (script.ReturnType == typeof(string))
                    {
                        await CanvasHasChanged();
                        return ((result is System.Drawing.Color col)? col.Name: (string)result, error);
                    }
                    error = new ExecutionError(ErrorCode.InvalidReturnType, "Return type not defined");
                    await CanvasHasChanged();
                    return (Core.Utils.SystemClass.Void.Value, error);
                }

                if (error is not null)
                {
                    _program.Context.ExitScope();
                    await CanvasHasChanged();
                    return (Core.Utils.SystemClass.Void.Value, error);
                }

                await CanvasHasChanged();
                script.MoveNext(1);
            }
            if (script.ReturnType != typeof(Core.Utils.SystemClass.Void)) error = new ExecutionError(ErrorCode.InvalidReturnType, "Script finished without return");
            return (Core.Utils.SystemClass.Void.Value, error);
        }   

        private async Task<ExecutionError?> ExecuteAssign(ExecutionError? error, Assign asg, CancellationToken cancellationToken)
        {
            error = null;
            if (_program is null)
            {
                error = new ExecutionError(ErrorCode.ProgramNotFound, "Program not found");
                return error;
            }
            if (asg.Expression is null)
            {
                error = new ExecutionError(ErrorCode.Invalid, "Expression not found");
                return error;
            }

            var res = await EvaluateExpression(asg.Expression, error, cancellationToken);
            var result = res.Item1;
            error = res.Item2;

            if (error is not null)
            {
                return error;
            }
            if (result is null)
            {
                error = new ExecutionError(ErrorCode.Invalid, "Expression");
                return error;
            }
            if (!_program.Context.CurrentScope.SetVariable(asg.Identifier, result))
            {
                error = new ExecutionError(ErrorCode.InvalidReturnType, "Not compatible type");
                return error;
            }
            return error;
        }

        private async Task<(ExecutionError?, object?)> ExecuteFunction(ExecutionError? error, Function func, object? result, CancellationToken cancellationToken)
        {
            result = null;
            error = null;
            if (_wall is null)
            {
                error = new ExecutionError(ErrorCode.WallENotAssigned, "WallE image editor not found");
                return (error, result);
            }
            if (_program is null)
            {
                error = new ExecutionError(ErrorCode.ProgramNotFound, "Program not found");
                return (error, result);
            }
            if (func is NewFunction nfunc)
            {
                foreach (Script script in _program.Scripts)
                {
                    if (script.Name.Equals(nfunc.Identifier))
                    {
                        var sct = await ExecuteScript(error, script, nfunc.Arguments, cancellationToken);
                        error = sct.Item2;
                        result = sct.Item1;
                        return (error, result);
                    }
                }
            }
            if (SystemFunctions.ContainsKey(func.Identifier))
            {
                try
                {
                    object?[] values = new object?[func.Arguments.Length];
                    for (int i = 0; i < func.Arguments.Length; i++)
                    {
                        var res = await EvaluateExpression(func.Arguments[i], error, cancellationToken);
                        values[i] = res.Item1;
                        error = res.Item2;
                        await CanvasHasChanged();
                        if (error is not null) return (error, result);
                    }
                    result = SystemFunctions[func.Identifier].Invoke(_wall, [error, .. values]);
                    return (error, result);
                }
                catch (Exception ex) when (ex is InvalidCastException or IndexOutOfRangeException or NullReferenceException)
                {
                    error = new ExecutionError(ErrorCode.UnexpectedError, $"System function error: {ex.Message}");
                    await CanvasHasChanged();
                    return (error, result);
                }
                catch (Exception ex)
                {
                    error = new ExecutionError(ErrorCode.UnexpectedError, $"Function unexpected exeption: {ex.Message}");
                    await CanvasHasChanged();
                    return (error, result);
                }
            }
            error = new ExecutionError(ErrorCode.Invalid, "Function call");
            return (error, result);
        }

        private async Task<(bool, ExecutionError?)> ExecuteGoTo(ExecutionError? error, GoTo goTo, ProgramAST ast, CancellationToken cancellationToken)
        {
            error = null;
            if (_program is null)
            {
                error = new ExecutionError(ErrorCode.ProgramNotFound, "Program not found");
                return (false, error);
            }
            if (goTo.Expression is null)
            {
                error = new ExecutionError(ErrorCode.Expected, "Expected Valid integer or bool expression");
                return (false, error);
            }
            var ex = await EvaluateIntegerOrBoolExpression(goTo.Expression, error, cancellationToken);
            if (ex.Item1)
            {
                if (ast.IsLabel(goTo.LabelIdentifier))
                { 
                    ast.GoToLabel(goTo.LabelIdentifier);
                    return (true, error);
                }
                error = new ExecutionError(ErrorCode.Invalid, "Label identifier");
                return (false, error);
            }
            if (ex.Item2 is not null) error = ex.Item2;
            return (false, error);
        }

        private async Task<(ExecutionError?, object?)> ExecuteReturn(ExecutionError? error, Return ret, object? result, CancellationToken cancellationToken)
        {
            error = null;
            result = null;
            if (_program is null)
            {
                error = new ExecutionError(ErrorCode.ProgramNotFound, "Program not found");
                return (error, result);
            }
            if (ret.Expression is null)
            {
                result = Core.Utils.SystemClass.Void.Value;
                return (error, result);
            }

            var res = await EvaluateExpression(ret.Expression, error, cancellationToken);
            result = res.Item1;
            error = res.Item2;

            if (error is not null)
            {
                return (error, result);
            }
            if (result is null)
            {
                error = new ExecutionError(ErrorCode.Invalid, "Expression");
                return (error, result);
            }
            return (error, result);
        }

        private async  Task<(object?, ExecutionError?)> EvaluateExpression(Expression exp, ExecutionError? error, CancellationToken cancellationToken)
        {
            error = null;
            if (exp is Expression<IntegerOrBool> i)
            {
                var r = await EvaluateIntegerOrBoolExpression(i, error, cancellationToken);
                if (r.Item2 is not null) return (r.Item1, new ExecutionError(r.Item2.Code, r.Item2.Argument));
                return (r.Item1, null);
            }
            if (exp is Expression<System.Drawing.Color> c)
            {
                var r = await EvaluateColor(c, error, cancellationToken);
                if (r.Item2 is not null) return (r.Item1, new ExecutionError(r.Item2.Code, r.Item2.Argument));
                return (r.Item1, null);
            }
            if (exp is Expression<string> s)
            {
                var r = await EvaluateString(s, error, cancellationToken);
                if (r.Item2 is not null) return (r.Item1, new ExecutionError(r.Item2.Code, r.Item2.Argument));
                return (r.Item1, null);
            }
            error = new ExecutionError(ErrorCode.Invalid, "Expression type");
            return (null, error);
        }

        private async Task<(IntegerOrBool, ExecutionError?)> EvaluateIntegerOrBoolExpression(Expression<IntegerOrBool> exp, ExecutionError? error, CancellationToken cancellationToken)
        {
            error = null;
            if (exp is BinaryExpression<IntegerOrBool> binary)
            {
                if (binary.Left is null || binary.Right is null)
                {
                    error = new ExecutionError(ErrorCode.NullReference, "Expected Valid integer or bool expression");
                    return (0, error);
                }
                var l = await EvaluateIntegerOrBoolExpression(binary.Left, error, cancellationToken);
                IntegerOrBool left = l.Item1;
                error = l.Item2;
                if (error is not null) return (0, error);
                var r = await EvaluateIntegerOrBoolExpression(binary.Right, error, cancellationToken);
                IntegerOrBool right = r.Item1;
                error = r.Item2;
                if (error is not null) return (0, error);
                return (binary.Evaluate(new Literal<IntegerOrBool>(left, new CodeLocation()), new Literal<IntegerOrBool>(right, new CodeLocation())), error);
            }
            if (exp is Not not)
            {
                if(not.Expression is null)
                {
                    error = new ExecutionError(ErrorCode.NullReference, "Expected Valid integer or bool expression");
                    return (0, error);
                }
                var ex = await EvaluateIntegerOrBoolExpression(not.Expression, error, cancellationToken);
                IntegerOrBool expression = ex.Item1;
                error = ex.Item2;
                if (error is not null) return (0, error);
                return (not.Evaluate(new Literal<IntegerOrBool>(expression, new CodeLocation())), error);
            }
            if (exp is Minus minus)
            {
                if (minus.Expression is null)
                {
                    error = new ExecutionError(ErrorCode.NullReference, "Expected Valid integer or bool expression");
                    return (0, error);
                }
                var ex = await EvaluateIntegerOrBoolExpression(minus.Expression, error, cancellationToken);
                IntegerOrBool expression = ex.Item1;
                error = ex.Item2;
                if (error is not null) return (0, error);
                return (minus.Evaluate(new Literal<IntegerOrBool>(expression, new CodeLocation())), error);
            }
            if (exp is Variable<IntegerOrBool> variable)
            {
                if(_program is null || !_program.Context.CurrentScope.GetVariable(variable.Identifier, out object? value))
                {
                    error = new ExecutionError(ErrorCode.Invalid, "Use of an unassigned variable");
                    return (0, error);
                }
                if (value is IntegerOrBool ib) return (ib, error);
                error = new ExecutionError(ErrorCode.InvalidReturnType, "Variable type not compatible");
                return (0, error);
            }
            if (exp is FunctionExpression<IntegerOrBool> func)
            {
                ExecutionError? er = null;
                var ex = await EvaluateFunction(er, func, cancellationToken);
                IntegerOrBool value = ex.Item1;
                error = ex.Item2;
                return (value, error);
            }
            if (exp is Literal<IntegerOrBool> lit) return (lit.Evaluate(), error);
            error = new ExecutionError(ErrorCode.Invalid, "Unknow expression");
            return (0, error);
        }

        private async Task<(IntegerOrBool, ExecutionError?)> EvaluateFunction(ExecutionError? error, FunctionExpression<IntegerOrBool> func, CancellationToken cancellationToken)
        {
            error = null;
            object? result = null;
            if (func.Function is null)
            {
                error = new ExecutionError(ErrorCode.Invalid, "Function as integer or boolean expression");
                return (0, error);
            }
            var r = await ExecuteFunction(error, func.Function, result, cancellationToken);
            result = r.Item2;
            error = r.Item1;

            if (error is not null) return (0, error);
            if (result is not int i)
            {
                if (result is not bool b)
                {
                    error = new ExecutionError(ErrorCode.InvalidReturnType, "Function as integer or boolean expression");
                    return (0, error);
                }
                return (b, error);
            }
            return (i, error);
        }

        private async Task<(string, ExecutionError?)> EvaluateString(Expression<string> exp, ExecutionError? error, CancellationToken cancellationToken)
        {
            error = null;
            if (exp is BinaryExpression<string> binary)
            {
                if (binary.Left is null || binary.Right is null)
                {
                    error = new ExecutionError(ErrorCode.NullReference, "Expected Valid integer or bool expression");
                    return ("", error);
                }
                var l = await EvaluateString(binary.Left, error, cancellationToken);
                string left = l.Item1;
                error = l.Item2;
                if (error is not null) return ("", error);
                var r = await EvaluateString(binary.Right, error, cancellationToken);
                string right = r.Item1;
                error = r.Item2;
                if (error is not null) return ("", error);
                return (binary.Evaluate(new Literal<string>(left, new CodeLocation()), new Literal<string>(right, new CodeLocation())), error);
            }
            if (exp is Variable<string> variable)
            {
                if (_program is null || !_program.Context.CurrentScope.GetVariable(variable.Identifier, out object? value))
                {
                    error = new ExecutionError(ErrorCode.Invalid, "Use of an unassigned variable");
                    return ("", error);
                }
                if (value is string s) return (s, error);
                if (value is System.Drawing.Color c) return (c.Name, error);
                error = new ExecutionError(ErrorCode.InvalidReturnType, "Variable type not compatible");
                return ("", error);
            }
            if (exp is FunctionExpression<string> func)
            {
                var re = await EvaluateFunction(error, func, cancellationToken);
                string value = re.Item1;
                error = re.Item2;
                return (value, error);
            }
            if (exp is Literal<string> lit) return (lit.Evaluate(), error);
            error = new ExecutionError(ErrorCode.Invalid, "Unknow expression");
            return ("", error);
        }

        private async Task<(string, ExecutionError?)> EvaluateFunction(ExecutionError? error, FunctionExpression<string> func, CancellationToken cancellationToken)
        {
            error = null;
            object? result = null;
            if (func.Function is null)
            {
                error = new ExecutionError(ErrorCode.Invalid, "Function as string expression");
                return ("", error);
            }
            var r = await ExecuteFunction(error, func.Function, result, cancellationToken);
            result = r.Item2;
            error = r.Item1;

            if (error is not null) return ("", error);
            if (result is not string s)
            {
                if (result is not System.Drawing.Color c)
                {
                    error = new ExecutionError(ErrorCode.InvalidReturnType, "Function as sting expression");
                    return ("", error);
                }
                return (c.Name, error);
            }
            return (s, error);
        }

        private async Task<(System.Drawing.Color, ExecutionError?)> EvaluateColor(Expression<System.Drawing.Color> exp, ExecutionError? error, CancellationToken cancellationToken)
        {
            error = null;
            if (exp is Variable<System.Drawing.Color> variable)
            {
                if (_program is null || !_program.Context.CurrentScope.GetVariable(variable.Identifier, out object? value))
                {
                    error = new ExecutionError(ErrorCode.Invalid, "Use of an unassigned variable");
                    return (System.Drawing.Color.Transparent, error);
                }
                if (value is System.Drawing.Color color) return (color, error);
                error = new ExecutionError(ErrorCode.InvalidReturnType, "Variable type not compatible");
                return (System.Drawing.Color.Transparent, error);
            }
            if (exp is FunctionExpression<System.Drawing.Color> func)
            {
                var result = await EvaluateFunction(error, func, cancellationToken);
                System.Drawing.Color value = result.Item1;
                error = result.Item2;
                return (value, error);
            }
            if (exp is Literal<System.Drawing.Color> lit) return (lit.Evaluate(), error);
            error = new ExecutionError(ErrorCode.Invalid, "Unknow expression");
            return (System.Drawing.Color.Transparent, error);
        }

        private async Task<(System.Drawing.Color, ExecutionError?)> EvaluateFunction(ExecutionError? error, FunctionExpression<System.Drawing.Color> func, CancellationToken cancellationToken)
        {
            error = null;
            object? result = null;
            if (func.Function is null)
            {
                error = new ExecutionError(ErrorCode.Invalid, "Function as color expression");
                return (System.Drawing.Color.Transparent, error);
            }
            var res = await ExecuteFunction(error, func.Function, result, cancellationToken);
            result = res.Item2;
            error = res.Item1;

            if (error is not null) return (System.Drawing.Color.Transparent, error);
            if (result is not System.Drawing.Color c)
            {
                error = new ExecutionError(ErrorCode.InvalidReturnType, "Function as color expression");
                return (System.Drawing.Color.Transparent, error);
            }
            return (c, error);
        }

        public async Task CanvasHasChanged()
        {
            if (_canvasChanged != null)
                await _canvasChanged.Invoke();
        }

        public void AddCanvasChangedListener(CanvasChanged listener)
        {
            _canvasChanged += listener;
        }
    }
}
