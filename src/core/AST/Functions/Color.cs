using Core.Utils.TokenSystem;

namespace Core.AST.Functions
{
    public class Color : Function<Utils.SystemClass.Void>
    {
        public Color(CodeLocation location) : base(location, "Color", [typeof(System.Drawing.Color)]) { }
    }
}
