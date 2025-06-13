using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Parser;
using Core.Lexer;

namespace Core.Controller
{
    public class LanguageServiceSelector
    {
        private readonly Dictionary<string, ILexerAnalyser> _lexers;
        private readonly Dictionary<string, IParserAnalyser> _parsers;

        public LanguageServiceSelector(Dictionary<string, ILexerAnalyser> lexers,
                                       Dictionary<string, IParserAnalyser> parsers)
        {
            _lexers = lexers;
            _parsers = parsers;
        }

        public ILexerAnalyser GetLexer(string extension)
            => _lexers.TryGetValue(extension, out var lexer)
               ? lexer
               : throw new NotSupportedException($"Lexer for '{extension}' not found");

        public IParserAnalyser GetParser(string extension)
            => _parsers.TryGetValue(extension, out var parser)
               ? parser
               : throw new NotSupportedException($"Parser for '{extension}' not found");
    }

}
