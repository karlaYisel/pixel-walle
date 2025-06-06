using Core.Utils.TokenSystem;

namespace Core.AST.Expressions.Atom
{
    public class Literal<T> : AtomExpression<T>
    {
        private T Value;

        public Literal(T value, CodeLocation location) : base(location)
        {
            Value = value;
        }

        public T Evaluate()
        {
            return Value;
        }
        public override bool HasExpressionFunctions()
        {
            return false;
        }
    }
}
