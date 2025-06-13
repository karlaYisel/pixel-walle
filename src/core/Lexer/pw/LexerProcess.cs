using Core.Utils.Error;
using Core.Utils.TokenSystem;

namespace Core.Lexer.pw
{
    public class LexerProcess : ILexerProcess
    {
        Dictionary<string, string> operators = new Dictionary<string, string>();
        Dictionary<string, string> stringcolor = new Dictionary<string, string>();
        Dictionary<string, string> keywords = new Dictionary<string, string>();

        public void RegisterOperator(string op, string tokenValue)
        {
            operators[op] = tokenValue;
        }

        public void RegisterString(string start, string end)
        {
            stringcolor[start] = end;
        }

        public void RegisterKeyword(string key, string tokenValue)
        {
            keywords[key] = tokenValue;
        }

        private bool MatchSymbol(TokenReader stream, List<Token> tokens, CodeLocation Location)
        {
            foreach (var op in operators.Keys.OrderByDescending(k => k.Length))
                if (stream.Match(op))
                {
                    tokens.Add(new Token(operators[op], TokenType.Symbol, Location));
                    return true;
                }
            return false;
        }

        private bool MatchString(TokenReader stream, List<Token> tokens, List<CompilingError> errors, CodeLocation Location)
        {
            foreach (var start in stringcolor.Keys.OrderByDescending(k => k.Length))
            {
                string text;
                if (stream.Match(start))
                {
                    if (!stream.ReadUntil(stringcolor[start], out text))
                        errors.Add(new CompilingError(Location, ErrorCode.Expected, stringcolor[start]));
                    tokens.Add(new Token(text, TokenType.String, Location));
                    return true;
                }
            }
            return false;
        }

        private bool MatchKeyword(TokenReader stream, List<Token> tokens, List<CompilingError> errors, CodeLocation Location)
        {
            foreach (var key in keywords.Keys.OrderByDescending(k => k.Length))
                if (stream.Match(key))
                {
                    tokens.Add(new Token(keywords[key], TokenType.Keywords, Location));
                    return true;
                }
            return false;
        }

        public IEnumerable<Token> GetTokens(string fileName, string code, List<CompilingError> errors)
        {
            List<Token> tokens = new List<Token>();

            TokenReader stream = new TokenReader(fileName, code);

            CodeLocation Location;

            while (!stream.EOF)
            {
                Location = stream.Location;
                string value;

                if (MatchSymbol(stream, tokens, Location))
                    continue;

                if (stream.ReadWhiteSpace())
                    continue;

                if (stream.ReadBoolean(out value))
                {
                    tokens.Add(new Token(value, TokenType.Boolean, Location));
                    continue;
                }

                if (MatchKeyword(stream, tokens, errors, Location))
                    continue;

                if (stream.ReadID(out value))
                {
                    tokens.Add(new Token(value, TokenType.Identifier, Location));
                    continue;
                }

                if (stream.ReadNumber(out value))
                {
                    int d;
                    if (!int.TryParse(value, out d))
                        errors.Add(new CompilingError(Location, ErrorCode.Invalid, "Number format"));
                    tokens.Add(new Token(value, TokenType.Number, Location));
                    continue;
                }

                if (MatchString(stream, tokens, errors, Location))
                    continue;

                var unkOp = stream.ReadAny();
                errors.Add(new CompilingError(Location, ErrorCode.Unknown, $"'{unkOp.ToString()}'"));
            }

            return tokens;
        }


        class TokenReader
        {
            string FileName;
            string code;
            int pos;
            int line;
            int lastLB;

            public TokenReader(string fileName, string code)
            {
                FileName = fileName;
                this.code = code;
                pos = 0;
                line = 1;
                lastLB = -1;
            }

            public CodeLocation Location
            {
                get
                {
                    return new CodeLocation
                    {
                        File = FileName,
                        Line = line,
                        Column = pos - lastLB
                    };
                }
            }

            public char Peek()
            {
                if (pos < 0 || pos >= code.Length)
                    throw new InvalidOperationException();

                return code[pos];
            }

            public bool EOF
            {
                get { return pos >= code.Length; }
            }

            public bool EOL
            {
                get { return EOF || code[pos] == '\n'; }
            }

            public bool ContinuesWith(string prefix)
            {
                if (pos + prefix.Length > code.Length)
                    return false;
                for (int i = 0; i < prefix.Length; i++)
                    if (code[pos + i] != prefix[i])
                        return false;
                return true;
            }

            public bool Match(string prefix)
            {
                if (ContinuesWith(prefix))
                {
                    for (int i = 0; i < prefix.Length; i++) ReadAny();
                    return true;
                }

                return false;
            }

            public bool ValidIdCharacter(char c, bool begining)
            {
                return begining ? char.IsLetter(c) : char.IsLetterOrDigit(c) || c == '_';
            }

            public bool ReadID(out string id)
            {
                id = "";
                while (!EOL && ValidIdCharacter(Peek(), id.Length == 0))
                    id += ReadAny();
                return id.Length > 0;
            }

            public bool ReadBoolean(out string boolean)
            {
                boolean = "";
                if ((ContinuesWith("true") || ContinuesWith("True")) && !ValidIdCharacter(code[pos + 4], false)) boolean = "true";
                if ((ContinuesWith("false") || ContinuesWith("False")) && !ValidIdCharacter(code[pos + 5], false)) boolean = "false";

                pos += boolean.Length;
                return boolean.Length > 0;
            }

            public bool ReadNumber(out string number)
            {
                number = "";
                while (!EOL && char.IsDigit(Peek()))
                    number += ReadAny();

                return number.Length > 0;
            }

            public bool ReadUntil(string end, out string text)
            {
                text = "";
                while (!Match(end))
                {
                    if (EOL || EOF)
                        return false;
                    text += ReadAny();
                }
                return true;
            }

            public bool ReadWhiteSpace()
            {
                if (char.IsWhiteSpace(Peek()))
                {
                    ReadAny();
                    return true;
                }
                return false;
            }

            public char ReadAny()
            {
                if (EOF)
                    throw new InvalidOperationException();

                if (EOL)
                {
                    line++;
                    lastLB = pos;
                }
                return code[pos++];
            }
        }
    }
}
