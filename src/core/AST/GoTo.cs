using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.SystemClass;
using Core.Utils.TokenSystem;

namespace Core.AST
{
    public class GoTo : ASTNodeBase
    {
        public string LabelIdentifier { get; private set; } = "";
        public Expression<IntegerOrBool>? Expression { get; private set; }
        public GoTo(CodeLocation location) : base(location){}
        public void SetIdentifier(string identifier)
        {
            LabelIdentifier = identifier;
        }
        public void SetExpression(Expression<IntegerOrBool> exp)
        {
            Expression = exp;
        }
    }
}
