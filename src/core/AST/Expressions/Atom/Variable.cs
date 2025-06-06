using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.TokenSystem;

namespace Core.AST.Expressions.Atom
{
    public class Variable<T> : AtomExpression<T>
    {
        public string Identifier { get; private set; }

        public Variable(string identifier, CodeLocation location) : base(location)
        {
            Identifier = identifier;
        }
        public override bool HasExpressionFunctions()
        {
            return false;
        }
    }
}
