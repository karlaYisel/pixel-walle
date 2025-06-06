using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    public class IsBrushColor : Function<bool>
    {
        public IsBrushColor(CodeLocation location) : base(location, "IsBrushColor", [typeof(System.Drawing.Color)]) { }
    }
}
