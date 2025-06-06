using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.AST.Expressions.Atom;
using Core.Utils.Error;
using Core.Utils.TokenSystem;

namespace Core.AST
{
    public abstract class Expression : ASTNodeBase
    {
        public Type ReturnType;
        public Expression(CodeLocation location, Type returnType) : base(location) 
        { 
            ReturnType = returnType; 
        }
        public abstract Expression Reduce();

        public abstract bool HasExpressionFunctions();
    }

    public abstract class Expression<T> : Expression
    {
        public Expression(CodeLocation location) : base(location, typeof(T)) {}

        public abstract override Expression<T> Reduce();
    }
}
