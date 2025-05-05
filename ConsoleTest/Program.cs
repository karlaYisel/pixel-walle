using Core.Lexer;
using Core.Lexer.pw;
using Core.Utils.Error;
using Core.Utils.TokenSystem;

ILexerAnalyser aly =  new LexerAnalyser();
ILexerProcess lex = aly.Lexical;

string text = "Spawn(0, 0)\r\n Color(Black)\r\n n <- 5\r\n k <- 3 + 3 * 10\r\n n <- k * 2\r\n actual-x <- GetActualX()\r\n i <- 0\r\n\r\n loop-1\r\n DrawLine(1, 0, 1)\r\n i <- i + 1\r\n is-brush-color-blue <- IsBrushColor(\"Blue\")\r\n Goto [loop-ends-here] (is-brush-color-blue == 1)\r\n GoTo [loop1] (i < 10)\r\n\r\n Color(\"Blue\")\r\n GoTo [loop1] (1 == 1)\r\n\r\n loop-ends-here";

List<CompilingError> errors = new List<CompilingError>();
IEnumerable<Token> tokens = lex.GetTokens("Example Code", text, errors);

int line = 1;

foreach (Token token in tokens)
{
    if (token.Location.Line != line)
    {
        Console.WriteLine("");
        line = token.Location.Line;
    }
    Console.Write(" |" + token.Value + "| ");
}
Console.WriteLine("");




