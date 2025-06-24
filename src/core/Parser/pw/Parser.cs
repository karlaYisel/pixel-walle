using Core.AST;
using Core.Utils.Error;
using Core.Utils.TokenSystem;
using Core.AST.Expressions.Atom;
using Core.Utils.SystemClass;
using Core.AST.Expressions.Binary;
using Core.AST.Functions;
using System.Globalization;

namespace Core.Parser.pw
{
    public class Parser : IParser
    {
        static Dictionary<string, System.Drawing.Color> colors = new Dictionary<string, System.Drawing.Color>();
        static Dictionary<string, Type> types = new Dictionary<string, Type>();

        public void RegisterColor(string colorName, System.Drawing.Color colorValue)
        {
            colors[colorName] = colorValue;
        }

        public void RegisterType(string typeName, Type type)
        {
            types[typeName] = type;
        }

        public Parser() {}

        public ProgramAST ParseProgram(string fileName, TokenStream code, List<CompilingError> errors)
        {
            ProgramAST program = new ProgramAST(fileName, errors);

            if (!code.CanLookAhead())
                return program;

            code.MoveBack(1);
            ParseCode(program, code, errors);
            return program;
        }

        public Script ParseScript(string fileName, TokenStream code, List<CompilingError> errors)
        { 
            Script script = new Script(fileName, errors);
            
            if (!code.CanLookAhead()) return script;

            List<CompilingError> errorsTemp = new List<CompilingError>();
            Type? typeReturn = null;
            if (!code.LookAhead().Type.Equals(TokenType.Keywords) || !types.ContainsKey(code.LookAhead().Value))
            {
                errorsTemp.Add(new CompilingError(code.LookAhead().Location, ErrorCode.Expected, "Function return type"));
            }
            else
            {
                string type = code.LookAhead().Value;
                if (!types.ContainsKey(type))
                    throw new NotImplementedException($"Type {type} not admitted by the lenguaje");
                typeReturn = types[type];
            }

            if(!code.Next(TokenValues.func))
                errorsTemp.Add(new CompilingError(code.LookAhead().Location, ErrorCode.Expected, "Function keyword"));

            if (!code.Next(TokenType.Identifier))
                errorsTemp.Add(new CompilingError(code.LookAhead().Location, ErrorCode.Expected, "Function name"));
            else script = new Script(code.LookAhead().Value, errors);
            if(typeReturn is not null) script.SetReturnType(typeReturn);

            if (!code.Next(TokenValues.OpenBracket))
                errorsTemp.Add(new CompilingError(code.LookAhead().Location, ErrorCode.Expected, "("));

            List<Type> typeList = new List<Type>();
            List<string> nameList = new List<string>();
            if (!code.Next(TokenValues.ClosedBracket))
                while (true)
                {
                    if (!types.ContainsKey(code.LookAhead(1).Value) && !code.Next(TokenType.Identifier))
                    {
                        errorsTemp.Add(new CompilingError(code.LookAhead().Location, ErrorCode.Expected, ")"));
                        break;
                    }

                    if (!types.ContainsKey(code.LookAhead(1).Value))
                    {
                        errorsTemp.Add(new CompilingError(code.LookAhead().Location, ErrorCode.Expected, "Argument type"));
                        typeList.Add(types["void"]);
                    }
                    else if(code.Next(TokenValues.Void))
                    {
                        errorsTemp.Add(new CompilingError(code.LookAhead().Location, ErrorCode.Invalid, "Argument type"));
                        typeList.Add(types["void"]);
                    }
                    else
                    {
                        code.Next();
                        typeList.Add(types[code.LookAhead().Value]);
                    }

                    if (!code.Next(TokenType.Identifier))
                    {
                        errorsTemp.Add(new CompilingError(code.LookAhead().Location, ErrorCode.Expected, "Argument name"));
                        nameList.Add("");
                    }
                    else
                    {
                        if (nameList.Contains(code.LookAhead().Value)) 
                            errorsTemp.Add(new CompilingError(code.LookAhead().Location, ErrorCode.RepeatedArgumnet, "Argument name already defined"));
                        nameList.Add(code.LookAhead().Value);
                    }

                    if (code.Next(TokenValues.ClosedBracket)) break;
                    if (!code.CanLookAhead(1) || code.LookAhead().Value.Equals("StatementSeparator"))
                    {
                        errorsTemp.Add(new CompilingError(code.LookAhead().Location, ErrorCode.Expected, ")"));
                        break;
                    }
                    if (!code.Next(TokenValues.ValueSeparator)) errorsTemp.Add(new CompilingError(code.LookAhead(1).Location, ErrorCode.Expected, "Value Separator"));
                }
            script.SetArgument([..typeList], [..nameList]);

            if (code.CanLookAhead(-1))
            {
                errors.AddRange(errorsTemp);
                while (code.CanLookAhead(1) && !code.Next(TokenValues.StatementSeparator))
                {
                    code.Next();
                    errors.Add(new CompilingError(code.LookAhead().Location, ErrorCode.Expected, "Statement Separator"));
                }
            }
            else
            {
                errors.Add(new CompilingError(code.LookAhead().Location, ErrorCode.Missing, "Function Declaration"));
                code.MoveBack(1);
            }

            ParseCode(script, code, errors);
            return script;
        }

        private void ParseCode(ProgramAST program, TokenStream code, List<CompilingError> errors)
        {
            TokenReader stream = new TokenReader(code, errors);

            if(program is Script && !stream.CheckValidIdentifier(program.Name))
            {
                errors.Add(new CompilingError(new CodeLocation { File = program.Name }, ErrorCode.Invalid, "Script name can not be a reserved word"));
            }

            while (code.CanLookAhead(1))
            {
                ASTNodeBase? node = null;

                if(stream.ParseCommand(errors, out node))
                {
                    if(node is not null) program.AddNode(node);
                    continue;
                }

                if (stream.ParseGoTo(errors, out node))
                {
                    if (node is not null) program.AddNode(node);
                    continue;
                }

                if (stream.ParseAssign(errors, out node))
                {
                    if (node is not null) program.AddNode(node);
                    continue;
                }

                if (stream.ParseReturn(errors, out node))
                {
                    if (node is not null) program.AddNode(node);
                    continue;
                }

                string label;
                CodeLocation location = code.LookAhead(1).Location;
                if (stream.ParseLabel(errors, out label))
                {
                    if(program.IsLabel(label))
                        errors.Add(new CompilingError(location, ErrorCode.Invalid, "Label already defined"));
                    if(!stream.CheckValidIdentifier(label)) 
                        errors.Add(new CompilingError(location, ErrorCode.Invalid, "Label identifier can not be a reserved word"));
                    program.AddLabel(label);
                    continue;
                }

                if (code.Next(TokenValues.StatementSeparator))
                {
                    continue;
                }

                code.Next();
                errors.Add(new CompilingError(code.LookAhead().Location, ErrorCode.Invalid, "Token"));
            }
        }

        private class TokenReader
        {
            private TokenStream stream;
            private List<CompilingError> errors;

            public TokenReader(TokenStream stream, List<CompilingError> errors)
            {
                this.stream = stream;
                this.errors = errors;
            }

            public bool ParseReturn(List<CompilingError> errors, out ASTNodeBase? Return)
            {
                Return = null;
                if (!stream.Next(TokenValues.Return)) return false;
                Return re = new Return(stream.LookAhead().Location);
                re.SetExpression(ParseExpression(errors));
                ParseStatementSeparator(errors);
                Return =  re;
                return true;
            }

            public bool ParseGoTo(List<CompilingError> errors, out ASTNodeBase? goTo)
            {
                goTo = null;
                if (!stream.Next(TokenType.Identifier)) return false;
                if (!(stream.LookAhead().Value == "Goto" || stream.LookAhead().Value == "GoTo" || stream.LookAhead().Value == "goto") || !stream.Next(TokenValues.OpenBraces))
                {
                    stream.MoveBack(1);
                    return false;
                }
                GoTo go = new GoTo(stream.LookAhead(-1).Location);
                if (!stream.Next(TokenType.Identifier)) errors.Add(new CompilingError(stream.LookAhead().Location, ErrorCode.Expected, "Valid Label"));
                else go.SetIdentifier(stream.LookAhead().Value);
                if (!stream.Next(TokenValues.ClosedBraces)) errors.Add(new CompilingError(stream.LookAhead().Location, ErrorCode.Expected, "]"));
                if (!stream.Next(TokenValues.OpenBracket)) errors.Add(new CompilingError(stream.LookAhead().Location, ErrorCode.Expected, "("));
                Expression<IntegerOrBool>? exp = ParseExpressionLv1(null, errors);
                if (exp is null) errors.Add(new CompilingError(stream.LookAhead().Location, ErrorCode.Expected, "Valid Boolean Expression"));
                else go.SetExpression(exp);
                if (!stream.Next(TokenValues.ClosedBracket)) errors.Add(new CompilingError(stream.LookAhead().Location, ErrorCode.Expected, ")"));
                ParseStatementSeparator(errors);
                goTo = go;
                return true;
            }

            public bool CheckValidIdentifier(string identifier)
            {
                if (types.ContainsKey(identifier)
                    ||identifier == "return"
                    ||identifier == "Goto"
                    ||identifier == "GoTo"
                    ||identifier == "goto"
                    ||Enum.IsDefined(typeof(FunctionIdentifier), identifier)) return false;
                return true;
            }

            public bool ParseLabel(List<CompilingError> errors, out string label)
            {
                label = "";
                if (!stream.Next(TokenType.Identifier)) return false;
                string identifier = stream.LookAhead().Value;
                ParseStatementSeparator(errors);
                label = identifier;
                return true;
            }

            public bool ParseAssign(List<CompilingError> errors, out ASTNodeBase? assg)
            {
                assg = null;
                if (!stream.Next(TokenType.Identifier)) return false;
                string identifier = stream.LookAhead().Value;
                if (!stream.Next(TokenValues.Assign))
                {
                    stream.MoveBack(1);
                    return false;
                }
                Assign assign = new Assign(stream.LookAhead(-1).Location, identifier);
                if(!CheckValidIdentifier(assign.Identifier)) errors.Add(new CompilingError(assign.Location, ErrorCode.Invalid, "Variable name can not be a reserved word"));
                Expression? exp = ParseExpression(errors);
                if (exp is null) errors.Add(new CompilingError(stream.LookAhead().Location, ErrorCode.Expected, "Valid Expression"));
                else if (exp is Literal<System.Drawing.Color> exps) exp = new Literal<string>(exps.Evaluate().Name, exps.Location);
                assign.SetExpression(exp);
                ParseStatementSeparator(errors);
                assg =  assign;
                return true;
            }

            public bool ParseCommand(List<CompilingError> errors, out ASTNodeBase? function)
            {  
                function = null;
                Function? func = ParseFunction(errors);
                if (func is null) return false;
                ParseStatementSeparator(errors);
                function = func;
                return true;
            }

            private void ParseStatementSeparator(List<CompilingError> errors)
            {
                while (stream.CanLookAhead(1) && !stream.Next(TokenValues.StatementSeparator))
                {
                    stream.Next();
                    errors.Add(new CompilingError(stream.LookAhead().Location, ErrorCode.Expected, "Statement Separator"));
                }
            }

            private Function? ParseFunction(List<CompilingError> errors)
            {
                if (!stream.Next(TokenType.Identifier)) return null;
                string identifier = stream.LookAhead().Value;
                if (!stream.Next(TokenValues.OpenBracket))
                {
                    stream.MoveBack(1);
                    return null;
                }
                object? instance; 
                if(Enum.IsDefined(typeof(FunctionIdentifier), identifier))
                {
                    Type? type = Type.GetType($"Core.AST.Functions.{identifier}");
                    if (type is null) throw new NotImplementedException($"Funtion {identifier} added as System Funtion but never implemented.");
                    instance = Activator.CreateInstance(type, [stream.LookAhead(-1).Location]);
                }
                else
                {
                    instance = new NewFunction(stream.LookAhead(-1).Location, identifier);
                }
                if (!(instance is Function func))
                {
                    throw new NotImplementedException($"Funtion {identifier} added as System Funtion but linked with a non Funtion-Based class.");
                }

                List<Expression> expressions = new List<Expression>();
                if(!stream.Next(TokenValues.ClosedBracket)) 
                    while (true)
                    {
                        Expression? expr = ParseExpression(errors);
                        if (expr is null)
                        {
                            errors.Add(new CompilingError(stream.LookAhead(0).Location, ErrorCode.Expected, ")"));
                             break;
                        }
                        if (expr.HasExpressionFunctions()) errors.Add(new CompilingError(expr.Location, ErrorCode.InvalidFunctionArguments, "A function can not contains another function as part of it's arguments"));
                        expressions.Add(expr);
                        if (stream.Next(TokenValues.ClosedBracket)) break;
                        if (!stream.CanLookAhead(1) || stream.LookAhead().Value.Equals("StatementSeparator"))
                        {
                            errors.Add(new CompilingError(stream.LookAhead().Location, ErrorCode.Expected, ")"));
                            break;
                        }
                        if (!stream.Next(TokenValues.ValueSeparator)) errors.Add(new CompilingError(stream.LookAhead(1).Location, ErrorCode.Expected, "Value Separator"));
                    }
                func.SetArguments([..expressions]);
                return func;
            }

            //Expressions
            private Expression<IntegerOrBool>? TryFirstNonNull(Expression<IntegerOrBool>? left, List<CompilingError> errors,  params Func<Expression<IntegerOrBool>?, List<CompilingError>, Expression<IntegerOrBool>?>[] funcs)
            {
                foreach (var func in funcs)
                {
                    var result = func(left, errors);
                    if (result is not null) return result;
                }
                return left;
            }
            private Expression<IntegerOrBool>? TryFirstNonNullAtomic(List<CompilingError> errors, params Func<List<CompilingError>, Expression<IntegerOrBool>?>[] funcs)
            {
                foreach (var func in funcs)
                {
                    var result = func(errors);
                    if (result is not null) return result;
                }
                return null;
            }

            private Expression? ParseExpression(List<CompilingError> errors)
            {
                int position = stream.Position;
                Expression? exp = ParseNonTypedExpression(null, errors);
                int lPosition = stream.Position;
                stream.MoveTo(position);
                Expression? nExp = ParseExpressionLv1(null, errors);
                if (lPosition < stream.Position)
                {
                    exp = nExp;
                    lPosition = stream.Position;
                }
                stream.MoveTo(position);
                nExp = ParseStringExpression(null, errors);
                if (lPosition < stream.Position)
                {
                    exp = nExp;
                    lPosition = stream.Position;
                }
                stream.MoveTo(position);
                nExp = ParseColor(errors);
                if (lPosition == position + 1 && nExp is not null)
                    return nExp;
                stream.MoveTo(lPosition);
                return exp;
            }

            private Expression<object>? ParseNonTypedExpression(Expression<object>? left, List<CompilingError> errors)
            {
                Expression<object>? newLeft = ParseObjectLiteral(left, errors);
                Expression<object>? exp = ParseObjectPlus(newLeft, errors);
                return exp;
            }

            private Expression<object>? ParseObjectLiteral(Expression<object>? left, List<CompilingError> errors)
            {
                int position = stream.Position;
                Function? exp = ParseFunction(errors);
                if (exp is not null)
                {
                    FunctionExpression<object> func = new FunctionExpression<object>(exp, stream.LookAhead().Location, out bool valid);
                    if (!valid)
                    {
                        stream.MoveTo(position);
                        return null;
                    }
                    return func;
                }
                if (!stream.Next(TokenType.Identifier))
                    return null;
                return new Variable<object>(stream.LookAhead().Value, stream.LookAhead().Location);
            }

            private Expression<object>? ParseObjectPlus(Expression<object>? left, List<CompilingError> errors)
            {
                if (left is null || !stream.Next(TokenValues.Add))
                    return left;

                Plus binary = new Plus(stream.LookAhead().Location);

                binary.SetLeftExpression(left);

                Expression<object>? right = ParseObjectLiteral(null, errors);
                if (right == null)
                {
                    stream.MoveBack(1);
                    return null;
                }
                binary.SetRigthExpression(right);
                return ParseObjectPlus(binary, errors);
            }

            private Expression<IntegerOrBool>? ParseExpressionLv1(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                Expression<IntegerOrBool>? newLeft = ParseExpressionLv2(left, errors);
                Expression<IntegerOrBool>? exp = ParseExpressionLv1_(newLeft, errors);
                return exp;
            }

            private Expression<IntegerOrBool>? ParseExpressionLv1_(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                return TryFirstNonNull(left, errors, ParseAnd);
            }

            private Expression<IntegerOrBool>? ParseExpressionLv2(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                Expression<IntegerOrBool>? newLeft = ParseExpressionLv3(left, errors);
                Expression<IntegerOrBool>? exp = ParseExpressionLv2_(newLeft, errors);
                return exp;
            }

            private Expression<IntegerOrBool>? ParseExpressionLv2_(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                return TryFirstNonNull(left, errors, ParseOr);
            }

            private Expression<IntegerOrBool>? ParseExpressionLv3(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                Expression<IntegerOrBool>? newLeft = ParseExpressionLv4(left, errors);
                Expression<IntegerOrBool>? exp = ParseExpressionLv3_(newLeft, errors);
                return exp;
            }

            private Expression<IntegerOrBool>? ParseExpressionLv3_(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                return TryFirstNonNull(left, errors, ParseEql, ParseGEq, ParseLEq, ParseGreater, ParseLess, ParseNotEq);
            }

            private Expression<IntegerOrBool>? ParseExpressionLv4(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                Expression<IntegerOrBool>? newLeft = ParseExpressionLv5(left, errors);
                Expression<IntegerOrBool>? exp = ParseExpressionLv4_(newLeft, errors);
                return exp;
            }

            private Expression<IntegerOrBool>? ParseExpressionLv4_(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                return TryFirstNonNull(left, errors, ParseAdd, ParseSub);
            }

            private Expression<IntegerOrBool>? ParseExpressionLv5(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                Expression<IntegerOrBool>? newLeft = ParseExpressionLv6(left, errors);
                Expression<IntegerOrBool>? exp = ParseExpressionLv5_(newLeft, errors);
                return exp;
            }

            private Expression<IntegerOrBool>? ParseExpressionLv5_(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                return TryFirstNonNull(left, errors, ParseMod);
            }

            private Expression<IntegerOrBool>? ParseExpressionLv6(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                Expression<IntegerOrBool>? newLeft = ParseExpressionLv7(left, errors);
                Expression<IntegerOrBool>? exp = ParseExpressionLv6_(newLeft, errors);
                return exp;
            }

            private Expression<IntegerOrBool>? ParseExpressionLv6_(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                return TryFirstNonNull(left, errors, ParseMul, ParseDiv);
            }

            private Expression<IntegerOrBool>? ParseExpressionLv7(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                Expression<IntegerOrBool>? newLeft = ParseExpressionLv8(left, errors);
                Expression<IntegerOrBool>? exp = ParseExpressionLv7_(newLeft, errors);
                return exp;
            }

            private Expression<IntegerOrBool>? ParseExpressionLv7_(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                return TryFirstNonNull(left, errors, ParsePow);
            }

            private Expression<IntegerOrBool>? ParseExpressionLv8(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                return TryFirstNonNullAtomic(errors, ParseNot, ParseMinus, ParseParentheses, ParseNumber, ParseBoolean, ParseFunctionIntegerOrBool, ParseVariableIntegerOrBool);
            }

            private Expression<IntegerOrBool>? ParseAnd(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                if (left is null || !stream.Next(TokenValues.And))
                    return null;

                And binary = new And(stream.LookAhead().Location);

                binary.SetLeftExpression(left);

                Expression<IntegerOrBool>? right = ParseExpressionLv2(null, errors);
                if (right == null)
                {
                    stream.MoveBack(1);
                    return null;
                }
                binary.SetRigthExpression(right);

                return ParseExpressionLv1_(binary, errors);
            }

            private Expression<IntegerOrBool>? ParseOr(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                if (left is null || !stream.Next(TokenValues.Or))
                    return null;

                Or binary = new Or(stream.LookAhead().Location);

                binary.SetLeftExpression(left);

                Expression<IntegerOrBool>? right = ParseExpressionLv3(null, errors);
                if (right == null)
                {
                    stream.MoveBack(1);
                    return null;
                }
                binary.SetRigthExpression(right);

                return ParseExpressionLv2_(binary, errors);
            }

            private Expression<IntegerOrBool>? ParseEql(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                if (left is null || !stream.Next(TokenValues.Eql))
                    return null;

                Eql binary = new Eql(stream.LookAhead().Location);

                binary.SetLeftExpression(left);

                Expression<IntegerOrBool>? right = ParseExpressionLv4(null, errors);
                if (right == null)
                {
                    stream.MoveBack(1);
                    return null;
                }
                binary.SetRigthExpression(right);

                return ParseExpressionLv3_(binary, errors);
            }

            private Expression<IntegerOrBool>? ParseGEq(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                if (left is null || !stream.Next(TokenValues.GEq))
                    return null;

                GEq binary = new GEq(stream.LookAhead().Location);

                binary.SetLeftExpression(left);

                Expression<IntegerOrBool>? right = ParseExpressionLv4(null, errors);
                if (right == null)
                {
                    stream.MoveBack(1);
                    return null;
                }
                binary.SetRigthExpression(right);

                return ParseExpressionLv3_(binary, errors);
            }

            private Expression<IntegerOrBool>? ParseLEq(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                if (left is null || !stream.Next(TokenValues.LEq))
                    return null;

                LEq binary = new LEq(stream.LookAhead().Location);

                binary.SetLeftExpression(left);

                Expression<IntegerOrBool>? right = ParseExpressionLv4(null, errors);
                if (right == null)
                {
                    stream.MoveBack(1);
                    return null;
                }
                binary.SetRigthExpression(right);
                return ParseExpressionLv3_(binary, errors);
            }

            private Expression<IntegerOrBool>? ParseGreater(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                if (left is null || !stream.Next(TokenValues.Greater))
                    return null;

                Greater binary = new Greater(stream.LookAhead().Location);

                binary.SetLeftExpression(left);

                Expression<IntegerOrBool>? right = ParseExpressionLv4(null, errors);
                if (right == null)
                {
                    stream.MoveBack(1);
                    return null;
                }
                binary.SetRigthExpression(right);
                return ParseExpressionLv3_(binary, errors);
            }

            private Expression<IntegerOrBool>? ParseLess(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                if (left is null || !stream.Next(TokenValues.Less))
                    return null;

                Less binary = new Less(stream.LookAhead().Location);

                binary.SetLeftExpression(left);

                Expression<IntegerOrBool>? right = ParseExpressionLv4(null, errors);
                if (right == null)
                {
                    stream.MoveBack(1);
                    return null;
                }
                binary.SetRigthExpression(right);
                return ParseExpressionLv3_(binary, errors);
            }

            private Expression<IntegerOrBool>? ParseNotEq(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                if (left is null || !stream.Next(TokenValues.NotEq))
                    return null;

                NotEq binary = new NotEq(stream.LookAhead().Location);

                binary.SetLeftExpression(left);

                Expression<IntegerOrBool>? right = ParseExpressionLv4(null, errors);
                if (right == null)
                {
                    stream.MoveBack(1);
                    return null;
                }
                binary.SetRigthExpression(right);
                return ParseExpressionLv3_(binary, errors);
            }

            private Expression<IntegerOrBool>? ParseAdd(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                if (left is null || !stream.Next(TokenValues.Add))
                    return null;

                Add binary = new Add(stream.LookAhead().Location);

                binary.SetLeftExpression(left);

                Expression<IntegerOrBool>? right = ParseExpressionLv5(null, errors);
                if (right == null)
                {
                    stream.MoveBack(1);
                    return null;
                }
                binary.SetRigthExpression(right);
                return ParseExpressionLv4_(binary, errors);
            }

            private Expression<IntegerOrBool>? ParseSub(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                if (left is null || !stream.Next(TokenValues.Sub))
                    return null;

                Sub binary = new Sub(stream.LookAhead().Location);

                binary.SetLeftExpression(left);

                Expression<IntegerOrBool>? right = ParseExpressionLv5(null, errors);
                if (right == null)
                {
                    stream.MoveBack(1);
                    return null;
                }
                binary.SetRigthExpression(right);
                return ParseExpressionLv4_(binary, errors);
            }

            private Expression<IntegerOrBool>? ParseMod(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                if (left is null || !stream.Next(TokenValues.Mod))
                    return null;

                Mod binary = new Mod(stream.LookAhead().Location);

                binary.SetLeftExpression(left);

                Expression<IntegerOrBool>? right = ParseExpressionLv6(null, errors);
                if (right == null)
                {
                    stream.MoveBack(1);
                    return null;
                }
                binary.SetRigthExpression(right);
                return ParseExpressionLv5_(binary, errors);
            }
            
            private Expression<IntegerOrBool>? ParseMul(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                if (left is null || !stream.Next(TokenValues.Mul))
                    return null;

                Mul binary = new Mul(stream.LookAhead().Location);

                binary.SetLeftExpression(left);

                Expression<IntegerOrBool>? right = ParseExpressionLv7(null, errors);
                if (right == null)
                {
                    stream.MoveBack(1);
                    return null;
                }
                binary.SetRigthExpression(right);
                return ParseExpressionLv6_(binary, errors);
            }
            
            private Expression<IntegerOrBool>? ParseDiv(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                if (left is null || !stream.Next(TokenValues.Div))
                    return null;

                Div binary = new Div(stream.LookAhead().Location);

                binary.SetLeftExpression(left);

                Expression<IntegerOrBool>? right = ParseExpressionLv7(null, errors);
                if (right == null)
                {
                    stream.MoveBack(1);
                    return null;
                }
                binary.SetRigthExpression(right);
                return ParseExpressionLv6_(binary, errors);
            }

            private Expression<IntegerOrBool>? ParsePow(Expression<IntegerOrBool>? left, List<CompilingError> errors)
            {
                if (left is null || !stream.Next(TokenValues.Pow))
                    return null;

                Pow binary = new Pow(stream.LookAhead().Location);

                binary.SetLeftExpression(left);

                Expression<IntegerOrBool>? right = ParseExpressionLv8(null, errors);
                if (right == null)
                {
                    stream.MoveBack(1);
                    return null;
                }
                binary.SetRigthExpression(right);
                return ParseExpressionLv7_(binary, errors);
            }

            private Expression<IntegerOrBool>? ParseNot(List<CompilingError> errors)
            {
                if (!stream.Next(TokenValues.Not))
                    return null;
                Not unary = new Not(stream.LookAhead().Location);
                unary.SetExpression(ParseExpressionLv8(null, errors));
                return unary;
            }

            private Expression<IntegerOrBool>? ParseMinus(List<CompilingError> errors)
            {
                if (!stream.Next(TokenValues.Sub))
                    return null;
                Minus unary = new Minus(stream.LookAhead().Location);
                Expression<IntegerOrBool>? exp = ParseExpressionLv8(null, errors);
                if (exp is null)
                {
                    stream.MoveBack(1);
                    return null;
                }
                unary.SetExpression(exp);
                return unary;
            }

            private Expression<IntegerOrBool>? ParseParentheses(List<CompilingError> errors)
            {
                if (!stream.Next(TokenValues.OpenBracket))
                    return null;
                Expression<IntegerOrBool>? exp = ParseExpressionLv1(null, errors);
                if (exp is null)
                {
                    stream.MoveBack(1);
                    return null;
                }
                if (!stream.Next(TokenValues.ClosedBracket))
                {
                    errors.Add(new CompilingError(stream.LookAhead().Location, ErrorCode.Expected, ")"));
                }
                return exp;
            }

            private FunctionExpression<IntegerOrBool>? ParseFunctionIntegerOrBool(List<CompilingError> errors)
            {
                int position = stream.Position;
                Function? exp = ParseFunction(errors);
                if (exp is null)
                    return null;
                FunctionExpression<IntegerOrBool> func = new FunctionExpression<IntegerOrBool>(exp, stream.LookAhead().Location, out bool valid);
                if (!valid)
                {
                    stream.MoveTo(position);
                    return null;
                }
                return func;
            }

            private Variable<IntegerOrBool>? ParseVariableIntegerOrBool(List<CompilingError> errors)
            {
                if (!stream.Next(TokenType.Identifier))
                    return null;
                return new Variable<IntegerOrBool>(stream.LookAhead().Value, stream.LookAhead().Location);
            }

            private Literal<IntegerOrBool>? ParseBoolean(List<CompilingError> errors)
            {
                if (!stream.Next(TokenType.Boolean))
                    return null;
                return new Literal<IntegerOrBool>(bool.Parse(stream.LookAhead().Value), stream.LookAhead().Location);
            }

            private Literal<IntegerOrBool>? ParseNumber(List<CompilingError> errors)
            {
                if (!stream.Next(TokenType.Number))
                    return null;
                if (!int.TryParse(stream.LookAhead().Value, out int result))
                    errors.Add(new CompilingError(stream.LookAhead().Location, ErrorCode.Invalid, "Integral constant"));
                return new Literal<IntegerOrBool>(result, stream.LookAhead().Location);
            }

            private Literal<System.Drawing.Color>? ParseColor(List<CompilingError> errors)
            {
                if (!stream.Next(TokenType.String))
                    return null;
                string value = stream.LookAhead(0).Value.ToLower();
                System.Drawing.Color color = System.Drawing.Color.FromArgb(0);
                if (colors.ContainsKey(value)) color = colors[value];
                else if (!IsHexadecimalColor(value, out color))
                {
                    stream.MoveBack(1);
                    return null;
                }
                return new Literal<System.Drawing.Color>(color, stream.LookAhead().Location);
            }

            private bool IsHexadecimalColor(string value, out System.Drawing.Color color)
            {
                color = System.Drawing.Color.FromArgb(0);
                if (string.IsNullOrEmpty(value)) return false;
                if (!value[0].Equals('#')) return false;
                string argb = value.Substring(1);
                if(argb.Length < 8) argb = "FF" + argb;
                if(argb.Length != 8) return false;
                int intArgb = 0;
                if (!int.TryParse(argb, NumberStyles.HexNumber, null, out intArgb)) return false;
                color = System.Drawing.Color.FromArgb(intArgb);
                return true;
            }

            private Expression<string>? TryFirstNonNullString(List<CompilingError> errors, params Func<List<CompilingError>, Expression<string>?>[] funcs)
            {
                foreach (var func in funcs)
                {
                    var result = func(errors);
                    if (result is not null) return result;
                }
                return null;
            }

            private Expression<string>? ParseStringExpression(Expression<string>? left, List<CompilingError> errors)
            {
                Expression<string>? newLeft = ParseStringLiteral(left, errors);
                Expression<string>? exp = ParseConct(newLeft, errors);
                return exp;
            }

            private Expression<string>? ParseStringLiteral(Expression<string>? left, List<CompilingError> errors)
            {
                return TryFirstNonNullString(errors, ParseFunctionString, ParseVariableString, ParseColorAsString, ParseString);
            }

            private Expression<string>? ParseConct(Expression<string>? left, List<CompilingError> errors)
            {
                if (left is null || !stream.Next(TokenValues.Add))
                    return left;

                Conct binary = new Conct(stream.LookAhead().Location);

                binary.SetLeftExpression(left);

                Expression<string>? right = ParseStringLiteral(null, errors);
                if (right == null)
                {
                    stream.MoveBack(1);
                    return null;
                }
                binary.SetRigthExpression(right);
                return ParseStringExpression(binary, errors);
            }

            private FunctionExpression<string>? ParseFunctionString(List<CompilingError> errors) 
            {
                int position = stream.Position;
                Function? exp = ParseFunction(errors);
                if (exp is null)
                    return null;
                FunctionExpression<string> func = new FunctionExpression<string>(exp, stream.LookAhead().Location, out bool valid);
                if (!valid)
                {
                    stream.MoveTo(position);
                    return null;
                }
                return func;
            }

            private Variable<string>? ParseVariableString(List<CompilingError> errors)
            {
                if (!stream.Next(TokenType.Identifier))
                    return null;
                return new Variable<string>(stream.LookAhead().Value, stream.LookAhead().Location);
            }

            private Literal<string>? ParseColorAsString(List<CompilingError> errors)
            {
                Literal<System.Drawing.Color>? color = ParseColor(errors);
                if (color is null) return null;
                return new Literal<string>(color.Evaluate().Name, color.Location);
            }

            private Literal<string>? ParseString(List<CompilingError> errors)
            {
                if (!stream.Next(TokenType.String))
                    return null;
                return new Literal<string>(stream.LookAhead().Value, stream.LookAhead().Location);
            }
        }
    }
}
