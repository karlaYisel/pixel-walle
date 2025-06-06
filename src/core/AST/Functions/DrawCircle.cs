using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.SystemClass;
using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    public class DrawCircle : Function<Utils.SystemClass.Void>
    {
        public DrawCircle(CodeLocation location) : base(location, "DrawCircle", [typeof(int), typeof(int), typeof(int)]) { }
    }
}
