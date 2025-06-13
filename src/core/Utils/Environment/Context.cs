using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utils.Environment
{
    public class Context
    {
        public Scope CurrentScope { get; private set; }

        public Context(Scope globalScope)
        {
            CurrentScope = globalScope;
        }

        public void EnterScope(Dictionary<string, Type> types)
        {
            CurrentScope = new Scope(CurrentScope);
            CurrentScope.SetVariables(types);
        }

        public bool ExitScope()
        {
            if (CurrentScope.Parent is not null)
            {
                CurrentScope = CurrentScope.Parent;
                return true;
            }
            return false;
        }
    }
}
