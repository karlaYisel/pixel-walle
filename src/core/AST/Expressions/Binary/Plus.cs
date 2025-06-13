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
    public class Plus : BinaryExpression<object>
    {
        public Plus(CodeLocation location) : base(location) { }

        public override bool TryEvaluate(out object result)
        {
            result = 0;
            if (Left is Literal<object> left && Right is Literal<object> right)
            {
                result = Evaluate(left, right);
                return true;
            }
            return false;
        }
        public override object Evaluate(Literal<object> left, Literal<object> right)
        {
            throw new NotImplementedException();
        }
    }
}
