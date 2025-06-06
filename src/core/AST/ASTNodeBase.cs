using Core.Utils.Error;
using Core.Utils.TokenSystem;

namespace Core.AST
{
    public abstract class ASTNodeBase
    {
        public CodeLocation Location { get; private set; }
        public ASTNodeBase(CodeLocation location)
        {
            Location = location;
        }
    }
}
