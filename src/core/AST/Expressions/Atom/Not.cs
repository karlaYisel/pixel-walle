using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.SystemClass;
using Core.Utils.TokenSystem;

namespace Core.AST.Expressions.Atom
{
    public class Not : AtomExpression<IntegerOrBool>
    {
        public Expression<IntegerOrBool>? Expression { get; private set; }
        public Not(CodeLocation location) : base(location) {}
        public void SetExpression(Expression<IntegerOrBool>? exp = null)
        {
            Expression = exp;
        }

        public bool TryEvaluate(out IntegerOrBool result)
        {
            result = 0;
            if (Expression is Literal<IntegerOrBool> expression)
            {
                result = Evaluate(expression);
                return true;
            }
            return false;
        }
        public static IntegerOrBool Evaluate(Literal<IntegerOrBool> expression)
        { 
            return !expression.Evaluate();
        }
        public override Expression<IntegerOrBool> Reduce()
        {
            if (Expression is not null) Expression = Expression.Reduce();
            if (TryEvaluate(out IntegerOrBool result)) return new Literal<IntegerOrBool>(result, Location);
            if (Expression is Not not && not.Expression is Not notnot) Expression = notnot.Expression;
            return this;
        }

        public override bool HasExpressionFunctions()
        {
            return Expression is not null ? Expression.HasExpressionFunctions() : false;
        }
    }
}
