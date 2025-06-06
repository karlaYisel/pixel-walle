using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.SystemClass;
using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    public class DrawLine : Function<Utils.SystemClass.Void>
    {
        public DrawLine(CodeLocation location) : base(location, "DrawLine", [typeof(int), typeof(int), typeof(int)]) { }
    }
}
