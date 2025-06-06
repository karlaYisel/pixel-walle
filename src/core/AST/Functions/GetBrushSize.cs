using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.SystemClass;
using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    public class GetBrushSize : Function<IntegerOrBool>
    {
        public GetBrushSize(CodeLocation location) : base(location, "GetBrushSize", []) { }
    }
}
