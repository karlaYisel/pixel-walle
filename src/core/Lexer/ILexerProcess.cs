using Core.Utils.Error;
using Core.Utils.TokenSystem;

namespace Core.Lexer
{
    public interface ILexerProcess
    {
        IEnumerable<Token> GetTokens(string fileName, string code, List<CompilingError> errors);
    }
}
