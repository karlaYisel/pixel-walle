using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.SystemClass;
using Core.Utils.TokenSystem;

namespace Core.AST
{
    public class Return : ASTNodeBase
    {
        public Expression? Expression { get; private set; }
        public Return(CodeLocation location) : base(location) { }
        public void SetExpression(Expression exp)
        {
            Expression = exp;
        }
    }
}
