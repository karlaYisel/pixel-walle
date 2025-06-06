using Core.AST;
using Core.Utils.Error;
using Core.Utils.TokenSystem;

namespace Core.Parser
{
    public interface IParser
    {
        ProgramAST ParseProgram(string fileName, TokenStream stream, List<CompilingError> errors);
        Script ParseScript(string fileName, TokenStream stream, List<CompilingError> errors);
    }
}
