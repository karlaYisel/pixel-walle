using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    public class NewFunction : Function
    {
        public NewFunction(CodeLocation location, string identifier) : base(location, identifier, []) { }

        public void SetTypes(Type[] types)
        {
            ArgumentsTypes = types;
        }
    }
}
