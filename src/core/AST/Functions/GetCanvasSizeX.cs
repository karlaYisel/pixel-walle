using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    public class GetCanvasSizeX : Function<int>
    {
        public GetCanvasSizeX(CodeLocation location) : base(location, "GetCanvasSizeX", []) { }
    }
}
