using Core.AST.Expressions.Atom;
using Core.Utils.TokenSystem;

namespace Core.AST.Expressions
{
    public abstract class BinaryExpression<T> : Expression<T>
    {
        public Expression<T>? Right { get; private set; }
        public Expression<T>? Left { get; private set; }
        public BinaryExpression(CodeLocation location) : base(location)
        {
        }
        public void SetRigthExpression(Expression<T>? exp = null)
        {
            Right = exp;
        }
        public void SetLeftExpression(Expression<T>? exp = null)
        {
            Left = exp;
        }
        public void SetExpressions(Expression<T>? left, Expression<T>? rigth)
        {
            Left = left;
            Right = rigth;
        }
        public abstract bool TryEvaluate(out T result);
        public override Expression<T> Reduce()
        {
            if(Left is not null) Left = Left.Reduce();
            if(Right is not null) Right = Right.Reduce();
            if (TryEvaluate(out T result)) return new Literal<T>(result, Location);
            return this;

        }
        public override bool HasExpressionFunctions()
        {
            bool left = Left is not null ? Left.HasExpressionFunctions() : false;
            bool right = Right is not null ? Right.HasExpressionFunctions() : false;
            return left || right;
        }
    }
}

