using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    public class Respawn : Function<Utils.SystemClass.Void>
    {
        public Respawn(CodeLocation location) : base(location, "Respawn", [typeof(int), typeof(int)]) { }
    }
}
