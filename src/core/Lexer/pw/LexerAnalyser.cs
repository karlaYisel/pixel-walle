using Core.Utils.TokenSystem;

namespace Core.Lexer.pw
{
    public class LexerAnalyser: ILexerAnalyser
    {
        private static LexerProcess? __LexicalProcess;
        public ILexerProcess Lexical
        {
            get
            {
                if (__LexicalProcess == null)
                {
                    __LexicalProcess = new LexerProcess();

                    __LexicalProcess.RegisterOperator("+", TokenValues.Add);
                    __LexicalProcess.RegisterOperator("-", TokenValues.Sub);
                    __LexicalProcess.RegisterOperator("*", TokenValues.Mul);
                    __LexicalProcess.RegisterOperator("/", TokenValues.Div);
                    __LexicalProcess.RegisterOperator("**", TokenValues.Pow);
                    __LexicalProcess.RegisterOperator("%", TokenValues.Mod);

                    __LexicalProcess.RegisterOperator("&&", TokenValues.And);
                    __LexicalProcess.RegisterOperator("||", TokenValues.Or);
                    __LexicalProcess.RegisterOperator("==", TokenValues.Eql);
                    __LexicalProcess.RegisterOperator(">=", TokenValues.GEq);
                    __LexicalProcess.RegisterOperator("<=", TokenValues.LEq);
                    __LexicalProcess.RegisterOperator(">", TokenValues.Greater);
                    __LexicalProcess.RegisterOperator("<", TokenValues.Less);
                    __LexicalProcess.RegisterOperator("!", TokenValues.Not);
                    __LexicalProcess.RegisterOperator("!=", TokenValues.NotEq);

                    __LexicalProcess.RegisterOperator("<-", TokenValues.Assign);
                    __LexicalProcess.RegisterOperator("←", TokenValues.Assign);
                    __LexicalProcess.RegisterOperator(",", TokenValues.ValueSeparator);
                    __LexicalProcess.RegisterOperator("\n", TokenValues.StatementSeparator);

                    __LexicalProcess.RegisterOperator("(", TokenValues.OpenBracket);
                    __LexicalProcess.RegisterOperator(")", TokenValues.ClosedBracket);
                    __LexicalProcess.RegisterOperator("[", TokenValues.OpenBraces);
                    __LexicalProcess.RegisterOperator("]", TokenValues.ClosedBraces);


                    __LexicalProcess.RegisterColor("\"", "\"");

                    __LexicalProcess.RegisterKeyword("int", TokenValues.integer);
                    __LexicalProcess.RegisterKeyword("bool", TokenValues.boolean);
                    __LexicalProcess.RegisterKeyword("color", TokenValues.color);
                    __LexicalProcess.RegisterKeyword("func", TokenValues.func);
                    __LexicalProcess.RegisterKeyword("return", TokenValues.Return);
                    __LexicalProcess.RegisterKeyword("void", TokenValues.Void);
                }

                return __LexicalProcess;
            }
        }
    }
}
