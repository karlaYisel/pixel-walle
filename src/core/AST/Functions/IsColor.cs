using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.SystemClass;
using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    public class IsColor : Function<IntegerOrBool>
    {
        public IsColor(CodeLocation location) : base(location, "IsColor", [typeof(System.Drawing.Color), typeof(int), typeof(int)]) { }
    }
}
