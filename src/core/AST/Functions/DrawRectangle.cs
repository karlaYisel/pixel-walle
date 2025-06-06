using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.SystemClass;
using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    public class DrawRectangle : Function<Utils.SystemClass.Void>
    {
        public DrawRectangle(CodeLocation location) : base(location, "DrawRectangle", [typeof(int), typeof(int), typeof(int), typeof(int)]) { }
    }
}
