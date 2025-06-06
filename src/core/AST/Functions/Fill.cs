using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.SystemClass;
using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    public class Fill : Function<Utils.SystemClass.Void>
    {
        public Fill(CodeLocation location) : base(location, "Fill", []) { }
    }
}
