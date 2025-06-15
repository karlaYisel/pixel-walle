using Core.AST.Expressions.Atom;
using Core.Utils.Error;
using Core.Utils.SystemClass;
using Core.Utils.TokenSystem;

namespace Core.AST
{
    public abstract class Function : ASTNodeBase
    {
        public string Identifier { get; protected set; }
        public Type[] ArgumentsTypes { get; protected set; }
        public Expression[] Arguments { get; protected set; }
        public Function(CodeLocation location, string identifier, Type[] types) : base(location)
        {
            Identifier = identifier;
            ArgumentsTypes = types;
            Arguments = new Expression[types.Length];
        }

        public void SetArguments(Expression[] arguments)
        {
            Arguments = arguments;
        }

        public bool CheckTypes(List<CompilingError> errors)
        {
            if (Arguments.Length != ArgumentsTypes.Length)
            {
                errors.Add(new CompilingError(Location, ErrorCode.InvalidFunctionArgumentsCount, $"There's not overload of this method with {Arguments.Length} arguments"));
                return false;
            }
            bool valid = true;
            for (int i = 0; i < Arguments.Length; i++)
            {
                if (!(ArgumentsTypes[i] == typeof(Core.Utils.SystemClass.Void) || Arguments[i].ReturnType == ArgumentsTypes[i] || (Arguments[i].ReturnType == typeof(IntegerOrBool) && (ArgumentsTypes[i] == typeof(int) || (ArgumentsTypes[i] == typeof(bool)))) || (ArgumentsTypes[i] == typeof(string) && Arguments[i].ReturnType == typeof(System.Drawing.Color)) ))
                {
                    errors.Add(new CompilingError(Arguments[i].Location, ErrorCode.InvalidFunctionArguments, $"Expected argument type {ArgumentsTypes[i].Name}"));
                    valid = false;
                }
            }
            return valid;
        }
    }

    public abstract class Function<T> : Function
    {
        public Type ReturnType => typeof(T);

        public Function(CodeLocation location, string identifier, Type[] types) : base(location, identifier, types) {}
    }
}
