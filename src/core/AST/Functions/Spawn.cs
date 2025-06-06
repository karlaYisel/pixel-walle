using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    public class Spawn : Function<Utils.SystemClass.Void>
    {
        public Spawn(CodeLocation location) : base(location, "Spawn", [typeof(int), typeof(int)]) { }
    }
}
