using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.TokenSystem;

namespace Core.AST
{
    public class Assign : ASTNodeBase
    {
        public string Identifier { get; private set; }
        public Expression? Expression { get; private set; }
        public Assign(CodeLocation location, string identifier) : base(location)
        {
            Identifier = identifier;
        }
        public void SetExpression(Expression exp)
        {
            Expression = exp;
        }
    }
}
