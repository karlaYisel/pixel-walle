using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.SystemClass;
using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    public class DrawFullCircle : Function<Utils.SystemClass.Void>
    {
        public DrawFullCircle(CodeLocation location) : base(location, "DrawFullCircle", [typeof(int), typeof(int), typeof(int)]) { }
    }
}
