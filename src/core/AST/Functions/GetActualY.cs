using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    public class GetActualY : Function<int>
    {
        public GetActualY(CodeLocation location) : base(location, "GetActualY", []) { }
    }
}
