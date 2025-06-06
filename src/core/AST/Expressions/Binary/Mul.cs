using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.AST.Expressions.Atom;
using Core.Utils.SystemClass;
using Core.Utils.TokenSystem;

namespace Core.AST.Expressions.Binary
{
    public class Mul : BinaryExpression<IntegerOrBool>
    {
        public Mul(CodeLocation location) : base(location) { }

        public override bool TryEvaluate(out IntegerOrBool result)
        {
            result = 0;
            if (Left is Literal<IntegerOrBool> left && Right is Literal<IntegerOrBool> right)
            {
                result = Evaluate(left, right);
                return true;
            }
            return false;
        }
        public static IntegerOrBool Evaluate(Literal<IntegerOrBool> left, Literal<IntegerOrBool> right)
        {
            return left.Evaluate() * right.Evaluate();
        }
    }
}
