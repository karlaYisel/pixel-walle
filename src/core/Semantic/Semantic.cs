using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils;
using Core.Utils.Environment;
using Core.AST;
using Core.AST.Functions;
using Core.Utils.SystemClass;
using Core.Utils.Error;
using Core.AST.Expressions;
using Core.AST.Expressions.Atom;
using Core.AST.Expressions.Binary;
using System.Runtime.CompilerServices;

namespace Core.Semantic
{
    public class Semantic : ISemantic
    {
        public Semantic() { }
        public Program CheckSemantic(ProgramAST code, params Script[] scripts)
        {
            Program program = new Program(code);
            program.AddScripts(scripts);

            CheckProgram(code, scripts);
            foreach (Script script in scripts) { CheckScript(script, scripts); }

            return program;
        }

        private void CheckProgram(ProgramAST code, Script[] scripts)
        {
            code.MoveTo(0);
            if (!code.CanLookAhead() || code.LookAhead() is not Spawn spw)
                code.Errors.Add(new CompilingError(new Utils.TokenSystem.CodeLocation { File = code.Name }, Utils.Error.ErrorCode.Missing, "Spawn function"));
            while (code.CanLookAhead(1))
            {
                code.MoveNext(1);
                if (code.LookAhead() is Spawn spawn)
                    code.Errors.Add(new CompilingError(spawn.Location, Utils.Error.ErrorCode.Invalid, "Spawn function out of context"));
            }
            code.MoveTo(0);
            CheckSemanticCode(code, scripts);
        }

        private void CheckScript(Script code, Script[] scripts)
        {
            code.MoveTo(-1);
            while (code.CanLookAhead(1))
            {
                code.MoveNext(1);
                if (code.LookAhead() is Spawn spawn)
                    code.Errors.Add(new CompilingError(spawn.Location, Utils.Error.ErrorCode.Invalid, "Spawn function out of context"));
            }
            if (code.ReturnType != typeof(Utils.SystemClass.Void))
            {
                bool ret = false;
                code.MoveTo(0);
                while (code.CanLookAhead())
                {
                    if (code.LookAhead() is Return)
                        ret = true;
                    code.MoveNext(1);
                }
                if (!ret)
                    code.Errors.Add(new CompilingError(new Utils.TokenSystem.CodeLocation { File = code.Name }, ErrorCode.NotReturn, "Return not found"));
            }
            code.MoveTo(0);
            CheckSemanticCode(code, scripts);
        }

        private void CheckSemanticCode(ProgramAST code, Script[] scripts)
        {
            Dictionary<string, Type> variables = new Dictionary<string, Type>();
            if (code is Script sct)
            {
                for (int i = 0; i < sct.ArgumentNames.Length; i++)
                {
                    Type type = sct.ArgumentTypes[i];
                    if (sct.ArgumentNames[i].Equals("")) continue;
                    if (type == typeof(Core.Utils.SystemClass.Void))
                        variables[sct.ArgumentNames[i]] = typeof(Core.Utils.SystemClass.Void);
                    else
                    {
                        if (!variables.ContainsKey(sct.ArgumentNames[i]))
                            variables[sct.ArgumentNames[i]] = type == typeof(int) || type == typeof(bool) ? typeof(IntegerOrBool): type;
                        else if (variables[sct.ArgumentNames[i]] != typeof(Core.Utils.SystemClass.Void) && variables[sct.ArgumentNames[i]] != (type == typeof(int) || type == typeof(bool) ? typeof(IntegerOrBool) : type))
                            variables[sct.ArgumentNames[i]] = typeof(Core.Utils.SystemClass.Void);
                    }
                }
            }

            ASTNodeBase node;
            while (code.CanLookAhead())
            {
                node = code.LookAhead();

                if (node is Function func)
                    CheckFunction(func, variables, scripts, code.Errors);

                if (node is Assign asg)
                {
                    if (asg.Expression is null)
                    {
                        code.Errors.Add(new CompilingError(asg.Location, ErrorCode.Expected, "Valid Expression"));
                        variables[asg.Identifier] = typeof(Core.Utils.SystemClass.Void);
                    }
                    else
                    {
                        Type type = CheckExpression(asg.Expression, variables, scripts, code.Errors, out Expression expResult);
                        asg.SetExpression(expResult);
                        if (type == typeof(Core.Utils.SystemClass.Void))
                        {
                            code.Errors.Add(new CompilingError(asg.Location, ErrorCode.Invalid, "Expression"));
                            variables[asg.Identifier] = typeof(Core.Utils.SystemClass.Void);
                        }
                        else
                        {
                            if (!variables.ContainsKey(asg.Identifier))
                                variables[asg.Identifier] = type;
                            else if (variables[asg.Identifier] != typeof(Core.Utils.SystemClass.Void) && variables[asg.Identifier] != type)
                                code.Errors.Add(new CompilingError(asg.Location, ErrorCode.InvalidReturnType, $"Can not convert expression type \"{type.Name}\" to \"{variables[asg.Identifier].Name}\""));
                        }
                    }
                }

                if (node is Return ret)
                    CheckReturn(ret, code, variables, scripts, code.Errors);

                if (node is GoTo goTo)
                {
                    CheckGoTo(goTo, code, variables, scripts, code.Errors);
                    code.MoveNext(1);
                    break;
                }

                code.MoveNext(1);
            }

            int position = code.GetPosition();

            List<Assign> objectAssign = new List<Assign>();
            while (code.CanLookAhead())
            {
                node = code.LookAhead();

                if (node is Assign asg)
                {
                    if (asg.Expression is null)
                    {
                        code.Errors.Add(new CompilingError(asg.Location, ErrorCode.Expected, "Valid Expression"));
                        variables[asg.Identifier] = typeof(Core.Utils.SystemClass.Void);
                    }
                    else
                    {
                        if (!variables.ContainsKey(asg.Identifier))
                        {
                            if (asg.Expression.ReturnType != typeof(object))
                                variables[asg.Identifier] = asg.Expression.ReturnType;
                            else
                            {
                                variables[asg.Identifier] = typeof(object);
                                objectAssign.Add(asg);
                            }
                        }
                        else if (asg.Expression.ReturnType == typeof(object))
                            objectAssign.Add(asg);
                        else if (variables[asg.Identifier] == typeof(object))
                            variables[asg.Identifier] = asg.Expression.ReturnType;
                        else if (variables[asg.Identifier] != typeof(Core.Utils.SystemClass.Void) && variables[asg.Identifier] != asg.Expression.ReturnType)
                        {
                            code.Errors.Add(new CompilingError(asg.Location, ErrorCode.InvalidReturnType, $"Can not convert expression type \"{asg.Expression.ReturnType.Name}\" to \"{variables[asg.Identifier].Name}\""));
                            variables[asg.Identifier] = typeof(Core.Utils.SystemClass.Void);
                        }
                    }
                }

                code.MoveNext(1);
            }

            CheckObjectAssign(ref objectAssign, variables, scripts, code.Errors, code);
            code.SetVariablesTypes(variables);

            code.MoveTo(position);

            while (code.CanLookAhead())
            {
                node = code.LookAhead();

                if (node is Function func)
                    CheckFunction(func, variables, scripts, code.Errors);

                if (node is Assign asg)
                    CheckAssign(asg, variables, scripts, code.Errors);

                if (node is Return ret)
                    CheckReturn(ret, code, variables, scripts, code.Errors);

                if (node is GoTo goTo)
                    CheckGoTo(goTo, code, variables, scripts, code.Errors);

                code.MoveNext(1);
            }

            code.MoveTo(0);
        }

        private void CheckObjectAssign(ref List<Assign> objectAssign, Dictionary<string, Type> variables, Script[] scripts, List<CompilingError> errors, ProgramAST code)
        { 
            List<Assign> obj = new List<Assign>();
            foreach (Assign assign in objectAssign) { obj.Add(assign); }

            Expression? exp = null;
            do
            {
                objectAssign.Clear();
                foreach (Assign assign in obj) { objectAssign.Add(assign); }
                foreach (Assign asg in objectAssign)
                {
                    if (asg.Expression is not Expression<object> obEx)
                    {
                        obj.Remove(asg);
                        continue;
                    }
                    exp = CheckObjectExpression(obEx, variables, scripts, errors);
                    if (exp is not Expression<object>)
                    {
                        asg.SetExpression(exp);
                        if (!variables.ContainsKey(asg.Identifier) || variables[asg.Identifier] == typeof(object) || exp is null) 
                            variables[asg.Identifier] = exp is null ? typeof(Core.Utils.SystemClass.Void) : exp.ReturnType;
                        else if (variables[asg.Identifier] != typeof(Core.Utils.SystemClass.Void) && variables[asg.Identifier] != asg.Expression.ReturnType) 
                            variables[asg.Identifier] = typeof(Core.Utils.SystemClass.Void);
                        obj.Remove(asg);
                    }
                }
            } while (objectAssign.Count != obj.Count);

            foreach (Assign assign in obj)
                errors.Add(new CompilingError(assign.Location, ErrorCode.Invalid, "Variables in expression create a circular assign dependency"));
        }

        private Expression? CheckObjectExpression(Expression<object>? exp, Dictionary<string, Type> variables, Script[] scripts, List<CompilingError> errors)
        {
            if (exp is null) return null;
            if (exp is BinaryExpression<object> binary)
            {
                Expression? left = CheckObjectExpression(binary.Left, variables, scripts, errors);
                Expression? right = CheckObjectExpression(binary.Right, variables, scripts, errors);

                if (left is null || right is null) return null;
                if (left is Expression<object> || right is Expression<object>) return exp;
                if (left is Expression<IntegerOrBool> iLeft && right is Expression<IntegerOrBool> iRight)
                {
                    Add add = new Add(exp.Location);
                    add.SetExpressions(iLeft, iRight);
                    return add;
                }
                if (left is Expression<string> sLeft && right is Expression<string> sRight)
                {
                    Conct conct = new Conct(exp.Location);
                    conct.SetExpressions(sLeft, sRight);
                    return conct;
                }
                if(left.ReturnType != right.ReturnType)
                {
                    errors.Add(new CompilingError(exp.Location, ErrorCode.Invalid, "Expression mix types"));
                }
            }
            else if (exp is Variable<object> variable)
            {
                if(!variables.ContainsKey(variable.Identifier) || variables[variable.Identifier] == typeof(Core.Utils.SystemClass.Void))
                {
                    errors.Add(new CompilingError(variable.Location, ErrorCode.Invalid, "Variable identifier not assigned or not all assigns of this identifier are valid expressions"));
                    return null;
                }
                if(variables[variable.Identifier] == typeof(IntegerOrBool))
                {
                    Variable<IntegerOrBool> iOrB = new Variable<IntegerOrBool>(variable.Identifier, variable.Location);
                    return iOrB;
                }
                if (variables[variable.Identifier] == typeof(string))
                {
                    Variable<string> varString = new Variable<string>(variable.Identifier, variable.Location);
                    return varString;
                }
                if (variables[variable.Identifier] == typeof(System.Drawing.Color))
                {
                    Variable<System.Drawing.Color> varString = new Variable<System.Drawing.Color>(variable.Identifier, variable.Location);
                    return varString;
                }
                if (variables[variable.Identifier] == typeof(object))
                    return exp;
            }
            else if (exp is FunctionExpression<object> func)
            {
                if (func.Function is null)
                {
                    errors.Add(new CompilingError(exp.Location, ErrorCode.Expected, $"Valid function"));
                    return null;
                }
                if (func.Function is NewFunction nfunc)
                {
                    foreach (Script script in scripts)
                    {
                        if (script.Name.Equals(nfunc.Identifier))
                        {
                            if (script.ReturnType == typeof(int) || script.ReturnType == typeof(bool))
                            {
                                return new FunctionExpression<IntegerOrBool>(func.Function, func.Location, out bool validFunction);
                            }
                            if (script.ReturnType == typeof(System.Drawing.Color))
                            {
                                return new FunctionExpression<System.Drawing.Color>(func.Function, func.Location, out bool validFunction);
                            }
                            if (script.ReturnType == typeof(string))
                            {
                                return new FunctionExpression<string>(func.Function, func.Location, out bool validFunction);
                            }
                            errors.Add(new CompilingError(exp.Location, ErrorCode.Invalid, "Function return type not admited"));
                            return null;
                        }
                    }
                    errors.Add(new CompilingError(func.Location, ErrorCode.FunctionNotFound, "Function identifier is not associated to any defined function"));
                    return null;
                }
                if (func.Function is Function<int> || func.Function is Function<bool>) return new FunctionExpression<IntegerOrBool>(func.Function, func.Location, out bool validFunction);
                if (func.Function is Function<string>) return new FunctionExpression<string>(func.Function, func.Location, out bool validFunction);
                if (func.Function is Function<System.Drawing.Color>) return new FunctionExpression<System.Drawing.Color>(func.Function, func.Location, out bool validFunction);
            }
            errors.Add(new CompilingError(exp.Location, ErrorCode.Invalid, "Expression return type not admited"));
            return null;
        }

        private void CheckFunction(Function func, Dictionary<string, Type> variables, Script[] scripts, List<CompilingError> errors)
        {
            if (func is NewFunction nfunc)
            {
                bool IsDefined = false;
                foreach (Script script in scripts)
                {
                    if (script.Name.Equals(nfunc.Identifier))
                    {
                        nfunc.SetTypes(script.ArgumentTypes);
                        IsDefined = true;
                        break;
                    }
                }
                if (!IsDefined)
                    errors.Add(new CompilingError(func.Location, ErrorCode.FunctionNotFound, "Function identifier is not associated to any defined function"));
            }
            for ( int i = 0; i < func.Arguments.Length; i++)
            {
                CheckExpression(func.Arguments[i], variables, scripts, errors, out Expression expResult);
                func.Arguments[i] = expResult;
            }
            func.CheckTypes(errors);
        }

        private void CheckGoTo(GoTo goTo, ProgramAST code, Dictionary<string, Type> variables, Script[] scripts, List<CompilingError> errors)
        {
            if (!code.IsLabel(goTo.LabelIdentifier))
                errors.Add(new CompilingError(goTo.Location, ErrorCode.Invalid, "Label identifier not found"));
            if (goTo.Expression is not null)
                CheckExpression(goTo.Expression, variables, scripts, errors);
        }   

        private void CheckAssign(Assign asg, Dictionary<string, Type> variables, Script[] scripts, List<CompilingError> errors)
        {
            if (asg.Expression is not null)
            {
                CheckExpression(asg.Expression, variables, scripts, errors, out Expression expResult);
                asg.SetExpression(expResult);
            }
        }

        private void CheckReturn(Return ret, ProgramAST code, Dictionary<string, Type> variables, Script[] scripts, List<CompilingError> errors)
        {
            if (ret.Expression is not null)
            {
                CheckExpression(ret.Expression, variables, scripts, code.Errors, out Expression expResult);
                ret.SetExpression(expResult);
            }
            if (code is not Script sct || sct.ReturnType == typeof(Core.Utils.SystemClass.Void))
            {
                if (ret.Expression is not null)
                    code.Errors.Add(new CompilingError(ret.Location, ErrorCode.InvalidReturnType, "This function's return type is 'void', 'return' keyword can not return an Expression"));
            }
            else if (sct.ReturnType == typeof(int) || sct.ReturnType == typeof(bool))
            {
                if (ret.Expression is not Expression<IntegerOrBool>)
                    code.Errors.Add(new CompilingError(ret.Location, ErrorCode.InvalidReturnType, $"This function's return type is '{sct.ReturnType.Name}', 'return' keyword can return an Expression type '{sct.ReturnType.Name}'"));
            }
            else if (sct.ReturnType == typeof(Color))
            {
                if (ret.Expression is not Expression<Color> && ret.Expression is not Expression<string>)
                    code.Errors.Add(new CompilingError(ret.Location, ErrorCode.InvalidReturnType, $"This function's return type is '{sct.ReturnType.Name}', 'return' keyword can return an Expression type '{sct.ReturnType.Name}'"));
            }
            else if (ret.Expression is null || sct.ReturnType != ret.Expression.ReturnType)
                code.Errors.Add(new CompilingError(ret.Location, ErrorCode.InvalidReturnType, $"This function's return type is '{sct.ReturnType.Name}', 'return' keyword can return an Expression type '{sct.ReturnType.Name}'"));
        }

        private Type CheckExpression(Expression exp, Dictionary<string, Type> variables, Script[] scripts, List<CompilingError> errors, out Expression expResult)
        {
            expResult = exp;
            if (exp is Expression<System.Drawing.Color>) return typeof(System.Drawing.Color);
            if (exp is Expression<IntegerOrBool> expIntBool)
            {
                CheckExpression(expIntBool, variables, scripts, errors);
                return typeof(IntegerOrBool);
            }
            if (exp is Expression<string> expString)
            {
                CheckExpression(expString, variables, scripts, errors);
                return typeof(IntegerOrBool);
            }
            if (exp is Expression<object> objExp)
            {
                Expression? ex = CheckObjectExpression(objExp, variables, scripts, errors);
                if (ex is not null) exp = ex;
                if (exp is Expression<object>) return typeof(object);
                return CheckExpression(exp, variables, scripts, errors, out expResult);
            }
            else errors.Add(new CompilingError(exp.Location, ErrorCode.Invalid, $"Unknow type of Expression"));
            return typeof(object);
        } 

        private void CheckExpression(Expression<string> exp, Dictionary<string, Type> variables, Script[] scripts, List<CompilingError> errors)
        {
            if (exp is BinaryExpression<string> binary)
            {
                if (binary.Left is null)
                    errors.Add(new CompilingError(exp.Location, ErrorCode.Expected, $"Valid Expression"));
                else CheckExpression(binary.Left, variables, scripts, errors);
                if (binary.Right is null)
                    errors.Add(new CompilingError(exp.Location, ErrorCode.Expected, $"Valid Expression"));
                else CheckExpression(binary.Right, variables, scripts, errors);
            }
            else if (exp is Variable<string> variable)
            {
                if (!variables.ContainsKey(variable.Identifier))
                    errors.Add(new CompilingError(exp.Location, ErrorCode.Invalid, $"Variable \"{variable.Identifier}\" never assigned"));
                else if (variables[variable.Identifier] != typeof(string) && variables[variable.Identifier] != typeof(object))
                    errors.Add(new CompilingError(exp.Location, ErrorCode.Invalid, $"Variable type"));
            }
            else if (exp is FunctionExpression<string> func)
            {
                if (func.Function is null)
                {
                    errors.Add(new CompilingError(exp.Location, ErrorCode.Expected, $"Valid function"));
                    return;
                }
                bool IsDefined = true;
                if (func.Function is NewFunction nfunc)
                {
                    IsDefined = false;
                    foreach (Script script in scripts)
                    {
                        if (script.Name.Equals(nfunc.Identifier))
                        {
                            if (script.ReturnType != typeof(string))
                                errors.Add(new CompilingError(func.Location, ErrorCode.InvalidReturnType, $"Can not convert type \"{script.ReturnType.Name}\" to a string expression"));
                            IsDefined = true;
                            break;
                        }
                    }
                    if (!IsDefined)
                    {
                        errors.Add(new CompilingError(func.Location, ErrorCode.FunctionNotFound, "Function identifier is not associated to any defined function"));
                        return;
                    }
                }
                else if (func.Function is not Function<string>)
                    errors.Add(new CompilingError(func.Location, ErrorCode.InvalidReturnType, $"Can not convert return type to string"));
                CheckFunction(func.Function, variables, scripts, errors);
            }
        }

        private void CheckExpression(Expression<IntegerOrBool> exp, Dictionary<string, Type> variables, Script[] scripts, List<CompilingError> errors)
        { 
            if (exp is BinaryExpression<IntegerOrBool> binary)
            {
                if (binary.Left is null)
                    errors.Add(new CompilingError(exp.Location, ErrorCode.Expected, $"Valid Expression"));
                else CheckExpression(binary.Left, variables, scripts, errors);
                if (binary.Right is null)
                    errors.Add(new CompilingError(exp.Location, ErrorCode.Expected, $"Valid Expression"));
                else CheckExpression(binary.Right, variables, scripts, errors);
            }
            else if (exp is Not not)
            {
                if (not.Expression is null)
                    errors.Add(new CompilingError(exp.Location, ErrorCode.Expected, $"Valid Expression"));
                else CheckExpression(not.Expression, variables, scripts, errors);
            }
            else if (exp is Minus minus)
            {
                if (minus.Expression is null)
                    errors.Add(new CompilingError(exp.Location, ErrorCode.Expected, $"Valid Expression"));
                else CheckExpression(minus.Expression, variables, scripts, errors);
            }
            else if (exp is Variable<IntegerOrBool> variable)
            {
                if (!variables.ContainsKey(variable.Identifier))
                    errors.Add(new CompilingError(exp.Location, ErrorCode.Invalid, $"Variable \"{variable.Identifier}\" never assigned"));
                else if (variables[variable.Identifier] != typeof(IntegerOrBool) && variables[variable.Identifier] != typeof(object))
                    errors.Add(new CompilingError(exp.Location, ErrorCode.Invalid, $"Variable type"));
            }
            else if (exp is FunctionExpression<IntegerOrBool> func)
            {
                if (func.Function is null)
                {
                    errors.Add(new CompilingError(exp.Location, ErrorCode.Expected, $"Valid function"));
                    return;
                }
                bool IsDefined = true;
                if (func.Function is NewFunction nfunc)
                {
                    IsDefined = false;
                    foreach (Script script in scripts)
                    {
                        if (script.Name.Equals(nfunc.Identifier))
                        {
                            if (script.ReturnType != typeof(int) && script.ReturnType != typeof(bool))
                                errors.Add(new CompilingError(func.Location, ErrorCode.InvalidReturnType, $"Can not convert type \"{script.ReturnType.Name}\" to an integer or boolean expression"));
                            IsDefined = true;
                            break;
                        }
                    }
                    if (!IsDefined)
                    {
                        errors.Add(new CompilingError(func.Location, ErrorCode.FunctionNotFound, "Function identifier is not associated to any defined function"));
                        return;
                    }
                }
                else if (func.Function is not Function<int> && func.Function is not Function<bool>)
                    errors.Add(new CompilingError(func.Location, ErrorCode.InvalidReturnType, $"Can not convert return type to an integer or boolean expression"));
                CheckFunction(func.Function, variables, scripts, errors);
            }
        }
    }
}
