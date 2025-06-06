using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    public class GetCanvasSizeY : Function<int>
    {
        public GetCanvasSizeY(CodeLocation location) : base(location, "GetCanvasSizeY", []) { }
    }
}
