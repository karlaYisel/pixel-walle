using Core.AST;
using Core.Utils;

namespace Core.Semantic
{
    public interface ISemantic
    {
        Program CheckSemantic(ProgramAST code, params Script[] scripts);
    }
}
