using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    public class Move : Function<Utils.SystemClass.Void>
    {
        public Move(CodeLocation location) : base(location, "Move", [typeof(int), typeof(int)]) { }
    }
}
