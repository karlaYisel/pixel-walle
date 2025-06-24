using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    public class SetCanvasColor : Function<Utils.SystemClass.Void>
    {
        public SetCanvasColor(CodeLocation location) : base(location, "SetCanvasColor", [typeof(System.Drawing.Color)]) { }
    }
}
