using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.AST.Functions;
using Core.Utils.SystemClass;
using Core.Utils.TokenSystem;

namespace Core.AST.Expressions.Atom
{
    internal class FunctionExpression<T> : AtomExpression<T>
    {
        public Function? Function { get; private set; }
        public FunctionExpression(Function function, CodeLocation location, out bool validFunction) : base(location)
        {
            validFunction = false;
            if (function is NewFunction || function is Function<T> || (typeof(T) == typeof(IntegerOrBool) && (function is Function<int> || function is Function<bool>)))
            {
                Function = function;
                validFunction = true;
            }
        }
        public override bool HasExpressionFunctions()
        {
            return true;
        }
    }
}
