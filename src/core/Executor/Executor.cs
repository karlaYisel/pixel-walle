using Core.AST;
using Core.AST.Expressions;
using Core.AST.Expressions.Atom;
using Core.AST.Functions;
using Core.Utils;
using Core.Utils.Error;
using Core.Utils.SystemClass;
using Core.Utils.TokenSystem;
using Core.Utils.ImageEditor;

namespace Core.Executor
{
    public class Executor : IExecutor
    {
        private Program? _program;
        private PixelWallE.IPixelWallE? _wall; 
        private Dictionary<string, Func<PixelWallE.IPixelWallE, object?[], object>> SystemFunctions;

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

        public void ExecuteCode(out ExecutionError? error, CancellationToken cancellationToken)
        {
            error = null;
            if (_program is null)
            {
                error = new ExecutionError(ErrorCode.ProgramNotFound, "Program not found");
                return;
            }
            if (_wall is null)
            {
                error = new ExecutionError(ErrorCode.WallENotAssigned, "WallE image editor not found");
                return;
            }

            _wall.SetBrushColor(out error, System.Drawing.Color.Transparent);
            if (error is not null) return;
            _wall.SetSize(out error, 1);
            if (error is not null) return;

            ProgramAST code = _program.Code;
            code.MoveTo(0);

            while (code.CanLookAhead())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    error = new ExecutionError(ErrorCode.ExecutionCancelled, "Execution was cancelled");
                    return;
                }

                ASTNodeBase node = code.LookAhead();

                if (node is Assign asg)
                {
                    ExecuteAssign(out error, asg, cancellationToken);
                }

                else if (node is Function func)
                {
                    object? r;
                    ExecuteFunction(out error, func, out r, cancellationToken);
                }

                else if (node is GoTo goTo)
                {
                    int? label;
                    if (ExecuteGoTo(out error, goTo, out label, code, cancellationToken) && label is int lab)
                    {
                        code.MoveTo(lab);
                        continue;
                    }
                }

                else if (node is Return ret)
                {
                    object? result;
                    ExecuteReturn(out error, ret, out result, cancellationToken);
                    _program.Context.ExitScope();
                    if (result is not null)
                    {
                        error = new ExecutionError(ErrorCode.InvalidReturnType, "Return type not void");
                    }
                    return;
                }

                if (error is not null)
                {
                    _program.Context.ExitScope();
                    return;
                }

                code.MoveNext(1);
            }
        }

        private object ExecuteScript(out ExecutionError? error, Script script, Expression[] Argumnets, CancellationToken cancellationToken) 
        {
            error = null;
            if (_program is null)
            {
                error = new ExecutionError(ErrorCode.ProgramNotFound, "Program not found");
                return Core.Utils.SystemClass.Void.Value;
            }

            script = script.ShallowCopy();
            script.MoveTo(0);
            if (script.ArgumentNames.Length != Argumnets.Length)
            {
                error = new ExecutionError(ErrorCode.InvalidFunctionArgumentsCount, "Function Argumnet Count");
                return Core.Utils.SystemClass.Void.Value;
            }
            _program.Context.EnterScope(script.GetVariablesTypes());
            for (int i = 0; i < script.ArgumentNames.Length; i++)
            {
                if (!_program.Context.CurrentScope.SetVariable(script.ArgumentNames[i], Argumnets[i]))
                {
                    error = new ExecutionError(ErrorCode.InvalidFunctionArguments, "Function Argumnet Type");
                    _program.Context.ExitScope();
                    return Core.Utils.SystemClass.Void.Value;
                }
            }

            while (script.CanLookAhead())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    error = new ExecutionError(ErrorCode.ExecutionCancelled, "Execution was cancelled");
                    return Core.Utils.SystemClass.Void.Value;
                }

                ASTNodeBase node = script.LookAhead();

                if (node is Assign asg)
                {
                    ExecuteAssign(out error, asg, cancellationToken);
                }

                else if (node is Function func)
                {
                    object? r;
                    ExecuteFunction(out error, func, out r, cancellationToken);
                }

                else if (node is GoTo goTo)
                {
                    int? label;
                    if (ExecuteGoTo(out error, goTo, out label, script, cancellationToken) && label is int lab)
                    {
                        script.MoveTo(lab);
                        continue;
                    }
                }

                else if (node is Return ret)
                {
                    object? result;
                    ExecuteReturn(out error, ret, out result, cancellationToken);
                    _program.Context.ExitScope();
                    if (result is null || result is Core.Utils.SystemClass.Void)
                    {
                        return Core.Utils.SystemClass.Void.Value;
                    }
                    if (script.ReturnType == typeof(int)) return (int)result;
                    if (script.ReturnType == typeof(bool)) return (bool)result;
                    if (script.ReturnType == typeof(System.Drawing.Color)) return (System.Drawing.Color)result;
                    if (script.ReturnType == typeof(string))
                    {
                        return (result is System.Drawing.Color col)? col.Name: (string)result;
                    }
                    error = new ExecutionError(ErrorCode.InvalidReturnType, "Return type not defined");
                    return Core.Utils.SystemClass.Void.Value;
                }

                if (error is not null)
                {
                    _program.Context.ExitScope();
                    return Core.Utils.SystemClass.Void.Value;
                }

                script.MoveNext(1);
            }
            if (script.ReturnType != typeof(Core.Utils.SystemClass.Void)) error = new ExecutionError(ErrorCode.InvalidReturnType, "Script finished without return");
            return Core.Utils.SystemClass.Void.Value;
        }   

        private void ExecuteAssign(out ExecutionError? error, Assign asg, CancellationToken cancellationToken)
        {
            error = null;
            if (_program is null)
            {
                error = new ExecutionError(ErrorCode.ProgramNotFound, "Program not found");
                return;
            }
            if (asg.Expression is null)
            {
                error = new ExecutionError(ErrorCode.Invalid, "Expression not found");
                return;
            }

            CompilingError? er;
            var result = EvaluateExpression(asg.Expression, out er, cancellationToken);

            if (er is not null)
            {
                error = new ExecutionError(er.Code, er.Argument);
                return;
            }
            if (result is null)
            {
                error = new ExecutionError(ErrorCode.Invalid, "Expression");
                return;
            }
            if (!_program.Context.CurrentScope.SetVariable(asg.Identifier, result))
            {
                error = new ExecutionError(ErrorCode.InvalidReturnType, "Not compatible type");
                return;
            }
        }

        private void ExecuteFunction(out ExecutionError? error, Function func, out object? result, CancellationToken cancellationToken)
        {
            result = null;
            error = null;
            if (_wall is null)
            {
                error = new ExecutionError(ErrorCode.WallENotAssigned, "WallE image editor not found");
                return;
            }
            if (_program is null)
            {
                error = new ExecutionError(ErrorCode.ProgramNotFound, "Program not found");
                return;
            }
            if (func is NewFunction nfunc)
            {
                foreach (Script script in _program.Scripts)
                {
                    if (script.Name.Equals(nfunc.Identifier))
                    {
                        result = ExecuteScript(out error, script, nfunc.Arguments, cancellationToken);
                        return;
                    }
                }
            }
            if (SystemFunctions.ContainsKey(func.Identifier))
            {
                try
                {
                    result = SystemFunctions[func.Identifier].Invoke(_wall, [error, .. func.Arguments]);
                    return;
                }
                catch (Exception ex) when (ex is InvalidCastException or IndexOutOfRangeException or NullReferenceException)
                {
                    error = new ExecutionError(ErrorCode.UnexpectedError, $"System function error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    error = new ExecutionError(ErrorCode.UnexpectedError, $"Function unexpected exeption: {ex.Message}");
                    return;
                }
            }
            error = new ExecutionError(ErrorCode.Invalid, "Function call");
            return;
        }

        private bool ExecuteGoTo(out ExecutionError? error, GoTo goTo, out int? labelPosition, ProgramAST ast, CancellationToken cancellationToken)
        {
            error = null;
            labelPosition = null;
            if (_program is null)
            {
                error = new ExecutionError(ErrorCode.ProgramNotFound, "Program not found");
                return false;
            }
            if (goTo.Expression is null)
            {
                error = new ExecutionError(ErrorCode.Expected, "Expected Valid integer or bool expression");
                return false;
            }
            CompilingError? er;
            if (EvaluateIntegerOrBoolExpression(goTo.Expression, out er, cancellationToken))
            {
                if (ast.IsLabel(goTo.LabelIdentifier))
                { 
                    ast.GoToLabel(goTo.LabelIdentifier);
                    return true;
                }
                return false;
            }
            if (er is not null) error = new ExecutionError(er.Code, er.Argument);
            return false;
        }

        private void ExecuteReturn(out ExecutionError? error, Return ret, out object? result, CancellationToken cancellationToken)
        {
            error = null;
            result = null;
            if (_program is null)
            {
                error = new ExecutionError(ErrorCode.ProgramNotFound, "Program not found");
                return;
            }
            if (ret.Expression is null)
            {
                result = Core.Utils.SystemClass.Void.Value;
                return;
            }

            CompilingError? er;
            result = EvaluateExpression(ret.Expression, out er, cancellationToken);

            if (er is not null)
            {
                error = new ExecutionError(er.Code, er.Argument);
                return;
            }
            if (result is null)
            {
                error = new ExecutionError(ErrorCode.Invalid, "Expression");
                return;
            }
        }

        private object? EvaluateExpression(Expression exp, out CompilingError? error, CancellationToken cancellationToken)
        {
            if (exp is Expression<IntegerOrBool> i) return EvaluateIntegerOrBoolExpression(i, out error, cancellationToken);
            if (exp is Expression<System.Drawing.Color> c) return EvaluateColor(c, out error, cancellationToken);
            if (exp is Expression<string> s) return EvaluateString(s, out error, cancellationToken);
            error = new CompilingError(exp.Location, ErrorCode.Invalid, "Expression type");
            return null;
        }

        private IntegerOrBool EvaluateIntegerOrBoolExpression(Expression<IntegerOrBool> exp, out CompilingError? error, CancellationToken cancellationToken)
        {
            error = null;
            if (exp is BinaryExpression<IntegerOrBool> binary)
            {
                if (binary.Left is null || binary.Right is null)
                {
                    error = new CompilingError(exp.Location, ErrorCode.NullReference, "Expected Valid integer or bool expression");
                    return 0;
                }
                IntegerOrBool left = EvaluateIntegerOrBoolExpression(binary.Left, out error, cancellationToken);
                if (error is not null) return 0;
                IntegerOrBool right = EvaluateIntegerOrBoolExpression(binary.Right, out error, cancellationToken);
                if (error is not null) return 0;
                return binary.Evaluate(new Literal<IntegerOrBool>(left, new CodeLocation()), new Literal<IntegerOrBool>(right, new CodeLocation()));
            }
            if (exp is Not not)
            {
                if(not.Expression is null)
                {
                    error = new CompilingError(exp.Location, ErrorCode.NullReference, "Expected Valid integer or bool expression");
                    return 0;
                }
                IntegerOrBool expression = EvaluateIntegerOrBoolExpression(not.Expression, out error, cancellationToken);
                if (error is not null) return 0;
                return not.Evaluate(new Literal<IntegerOrBool>(expression, new CodeLocation()));
            }
            if (exp is Minus minus)
            {
                if (minus.Expression is null)
                {
                    error = new CompilingError(exp.Location, ErrorCode.NullReference, "Expected Valid integer or bool expression");
                    return 0;
                }
                IntegerOrBool expression = EvaluateIntegerOrBoolExpression(minus.Expression, out error, cancellationToken);
                if (error is not null) return 0;
                return minus.Evaluate(new Literal<IntegerOrBool>(expression, new CodeLocation()));
            }
            if (exp is Variable<IntegerOrBool> variable)
            {
                if(_program is null || !_program.Context.CurrentScope.GetVariable(variable.Identifier, out object? value))
                {
                    error = new CompilingError(exp.Location, ErrorCode.Invalid, "Use of an unassigned variable");
                    return 0;
                }
                if (value is int i) return i;
                if (value is bool b) return b;
                error = new CompilingError(exp.Location, ErrorCode.InvalidReturnType, "Variable type not compatible");
                return 0;
            }
            if (exp is FunctionExpression<IntegerOrBool> func)
            {
                ExecutionError? er;
                IntegerOrBool value = EvaluateFunction(out er, func, cancellationToken);
                if (er is not null) error = new CompilingError(exp.Location, er.Code, er.Argument);
                return value;
            }
            if (exp is Literal<IntegerOrBool> lit) return lit.Evaluate();
            error = new CompilingError(exp.Location, ErrorCode.Invalid, "Unknow expression");
            return 0;
        }

        private IntegerOrBool EvaluateFunction(out ExecutionError? error, FunctionExpression<IntegerOrBool> func, CancellationToken cancellationToken)
        {
            object? result;
            if (func.Function is null)
            {
                error = new ExecutionError(ErrorCode.Invalid, "Function as integer or boolean expression");
                return 0;
            }
            ExecuteFunction(out error, func.Function, out result, cancellationToken);

            if (error is not null) return 0;
            if (result is not int i)
            {
                if (result is not bool b)
                {
                    error = new ExecutionError(ErrorCode.InvalidReturnType, "Function as integer or boolean expression");
                    return 0;
                }
                return b;
            }
            return i;
        }

        private string EvaluateString(Expression<string> exp, out CompilingError? error, CancellationToken cancellationToken)
        {
            error = null;
            if (exp is BinaryExpression<string> binary)
            {
                if (binary.Left is null || binary.Right is null)
                {
                    error = new CompilingError(exp.Location, ErrorCode.NullReference, "Expected Valid integer or bool expression");
                    return "";
                }
                string left = EvaluateString(binary.Left, out error, cancellationToken);
                if (error is not null) return "";
                string right = EvaluateString(binary.Right, out error, cancellationToken);
                if (error is not null) return "";
                return binary.Evaluate(new Literal<string>(left, new CodeLocation()), new Literal<string>(right, new CodeLocation()));
            }
            if (exp is Variable<string> variable)
            {
                if (_program is null || !_program.Context.CurrentScope.GetVariable(variable.Identifier, out object? value))
                {
                    error = new CompilingError(exp.Location, ErrorCode.Invalid, "Use of an unassigned variable");
                    return "";
                }
                if (value is string s) return s;
                if (value is System.Drawing.Color c) return c.Name;
                error = new CompilingError(exp.Location, ErrorCode.InvalidReturnType, "Variable type not compatible");
                return "";
            }
            if (exp is FunctionExpression<string> func)
            {
                ExecutionError? er;
                string value = EvaluateFunction(out er, func, cancellationToken);
                if (er is not null) error = new CompilingError(exp.Location, er.Code, er.Argument);
                return value;
            }
            if (exp is Literal<string> lit) return lit.Evaluate();
            error = new CompilingError(exp.Location, ErrorCode.Invalid, "Unknow expression");
            return "";
        }

        private string EvaluateFunction(out ExecutionError? error, FunctionExpression<string> func, CancellationToken cancellationToken)
        {
            object? result;
            if (func.Function is null)
            {
                error = new ExecutionError(ErrorCode.Invalid, "Function as string expression");
                return "";
            }
            ExecuteFunction(out error, func.Function, out result, cancellationToken);

            if (error is not null) return "";
            if (result is not string s)
            {
                if (result is not System.Drawing.Color c)
                {
                    error = new ExecutionError(ErrorCode.InvalidReturnType, "Function as sting expression");
                    return "";
                }
                return c.Name;
            }
            return s;
        }

        private System.Drawing.Color EvaluateColor(Expression<System.Drawing.Color> exp, out CompilingError? error, CancellationToken cancellationToken)
        {
            error = null;
            if (exp is Variable<System.Drawing.Color> variable)
            {
                if (_program is null || !_program.Context.CurrentScope.GetVariable(variable.Identifier, out object? value))
                {
                    error = new CompilingError(exp.Location, ErrorCode.Invalid, "Use of an unassigned variable");
                    return System.Drawing.Color.Transparent;
                }
                if (value is System.Drawing.Color color) return color;
                error = new CompilingError(exp.Location, ErrorCode.InvalidReturnType, "Variable type not compatible");
                return System.Drawing.Color.Transparent;
            }
            if (exp is FunctionExpression<System.Drawing.Color> func)
            {
                ExecutionError? er;
                System.Drawing.Color value = EvaluateFunction(out er, func, cancellationToken);
                if (er is not null) error = new CompilingError(exp.Location, er.Code, er.Argument);
                return value;
            }
            if (exp is Literal<System.Drawing.Color> lit) return lit.Evaluate();
            error = new CompilingError(exp.Location, ErrorCode.Invalid, "Unknow expression");
            return System.Drawing.Color.Transparent;
        }

        private System.Drawing.Color EvaluateFunction(out ExecutionError? error, FunctionExpression<System.Drawing.Color> func, CancellationToken cancellationToken)
        {
            object? result;
            if (func.Function is null)
            {
                error = new ExecutionError(ErrorCode.Invalid, "Function as color expression");
                return System.Drawing.Color.Transparent;
            }
            ExecuteFunction(out error, func.Function, out result, cancellationToken);

            if (error is not null) return System.Drawing.Color.Transparent;
            if (result is not System.Drawing.Color c)
            {
                error = new ExecutionError(ErrorCode.InvalidReturnType, "Function as color expression");
                return System.Drawing.Color.Transparent;
            }
            return c;
        }
    }
}
