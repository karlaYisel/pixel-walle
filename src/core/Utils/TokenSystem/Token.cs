namespace Core.Utils.TokenSystem
{
    public class Token
    {
        public string Value { get; private set; }
        public TokenType Type { get; private set; }
        public  CodeLocation Location { get; private set; }

        public Token(string value, TokenType type, CodeLocation location)
        {
            Value = value;
            Type = type;
            Location = location;
        }
    }
}
