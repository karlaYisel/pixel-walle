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
    public class Conct : BinaryExpression<string>
    {
        public Conct(CodeLocation location) : base(location) { }

        public override bool TryEvaluate(out string result)
        {
            result = "";
            if (Left is Literal<string> left && Right is Literal<string> right)
            {
                result = Evaluate(left, right);
                return true;
            }
            return false;
        }
        public override string Evaluate(Literal<string> left, Literal<string> right)
        {
            return left.Evaluate() + right.Evaluate();
        }
    }
}
