using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    public class GetColorCount : Function<int>
    {
        public GetColorCount(CodeLocation location) : base(location, "GetColorCount", [typeof(System.Drawing.Color), typeof(int), typeof(int), typeof(int), typeof(int)]) { }
    }
}
