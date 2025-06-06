using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    public class IsBrushSize : Function<bool>
    {
        public IsBrushSize(CodeLocation location) : base(location, "IsBrushSize", [typeof(int)]) { }
    }
}
