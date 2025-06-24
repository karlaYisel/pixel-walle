using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.SystemClass;
using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    public class DrawFullRectangle : Function<Utils.SystemClass.Void>
    {
        public DrawFullRectangle(CodeLocation location) : base(location, "DrawFullRectangle", [typeof(int), typeof(int), typeof(int), typeof(int), typeof(int)]) { }
    }
}
