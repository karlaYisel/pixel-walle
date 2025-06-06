using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.Error;
using Core.Utils.SystemClass;
using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    public class Size : Function<Utils.SystemClass.Void>
    {
        public Size(CodeLocation location) : base(location, "Size", [typeof(int)]) { }
    }
}
