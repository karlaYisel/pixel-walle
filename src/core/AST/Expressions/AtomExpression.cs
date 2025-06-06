using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Core.AST.Expressions.Atom;
using Core.Utils.TokenSystem;

namespace Core.AST.Expressions
{
    public abstract class AtomExpression<T> : Expression<T>
    {
        public AtomExpression(CodeLocation location) : base(location) {}
        public override Expression<T> Reduce() { return this; }
    }
}
