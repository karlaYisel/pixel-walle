using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    internal class IsCanvasColor : Function<bool>
    {
        public IsCanvasColor(CodeLocation location) : base(location, "IsCanvasColor", [typeof(System.Drawing.Color), typeof(int), typeof(int)]) { }
    }
}
