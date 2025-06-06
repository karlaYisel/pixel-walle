using Core.AST;
using Core.Lexer;
using Core.Lexer.pw;
using Core.Parser;
using Core.Parser.pw;
using Core.Utils.Error;
using Core.Utils.TokenSystem;

ILexerAnalyser aly =  new LexerAnalyser();
ILexerProcess lex = aly.Lexical;

string text = "Spawn(0, 0)\r\n Color(Black)\r\n n <- 5\r\n k <- 3 + 3 * 10\r\n n <- k * 2\r\n actual_x <- GetActualX()\r\n i <- 0\r\n\r\n loop_1\r\n DrawLine(1, 0, 1)\r\n i <- i + 1\r\n is_brush_color_blue <- IsBrushColor(\"Blue\")\r\n Goto [loop_ends_here] (is_brush_color_blue == 1)\r\n GoTo [loop_1] (i < 10)\r\n\r\n Color(\"Blue\")\r\n GoTo [loop_1] (1 == 1)\r\n\r\n loop_ends_here";

List<CompilingError> errors = new List<CompilingError>();
IEnumerable<Token> tokens = lex.GetTokens("Example Code", text, errors);

TokenStream stream = new TokenStream(tokens);
IParserAnalyser apy = new ParserAnalyser();
IParser par = apy.Parser;

ProgramAST program = par.ParseProgram("Code", stream, errors);






