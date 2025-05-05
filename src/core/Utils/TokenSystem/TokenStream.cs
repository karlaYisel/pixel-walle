using System.Collections;

namespace Core.Utils.TokenSystem
{
    public class TokenStream : IEnumerable<Token>
    {
        private List<Token> tokens;
        public int Position { get; private set; }
        public bool End => Position >= tokens.Count - 1;

        public TokenStream(IEnumerable<Token> tokens)
        {
            this.tokens = [.. tokens];
            Position = 0;
        }

        public void MoveNext(int k)
        {
            Position += k;
        }

        public void MoveBack(int k)
        {
            Position -= k;
        }

        public bool Next()
        {
            if (Position < tokens.Count - 1)
            {
                Position++;
            }

            return Position < tokens.Count;
        }

        public bool Next(TokenType type)
        {
            if (Position < tokens.Count - 1 && LookAhead(1).Type == type)
            {
                Position++;
                return true;
            }

            return false;
        }

        public bool Next(string value)
        {
            if (Position < tokens.Count - 1 && LookAhead(1).Value == value)
            {
                Position++;
                return true;
            }

            return false;
        }

        public bool CanLookAhead(int k = 0)
        {
            return tokens.Count - Position > k;
        }

        public Token LookAhead(int k = 0)
        {
            return tokens[Position + k];
        }

        public IEnumerator<Token> GetEnumerator()
        {
            for (int i = Position; i < tokens.Count; i++)
                yield return tokens[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
