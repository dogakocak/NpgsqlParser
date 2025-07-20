using NpgsqlParser.Models;

namespace NpgsqlParser.Lexing
{
    public class PostgreSqlTokenizer : ISqlTokenizer
    {
        private static readonly Dictionary<string, TokenType> Keywords = new()
        {
            { "select", TokenType.Select },
            { "from", TokenType.From },
            { "where", TokenType.Where }
        };

        public List<Token> Tokenize(string input)
        {
            var tokens = new List<Token>();
            var parts = input.Split([' ', ',', ';', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                string word = part.ToLower();

                if (Keywords.TryGetValue(word, out TokenType value))
                {
                    tokens.Add(new Token(value, word));
                }
                else if (int.TryParse(part, out _))
                {
                    tokens.Add(new Token(TokenType.Number, part));
                }
                else if (part == "*")
                {
                    tokens.Add(new Token(TokenType.Asterisk, part));
                }
                else if (part == "=")
                {
                    tokens.Add(new Token(TokenType.Equal, part));
                }
                else if (part == ">")
                {
                    tokens.Add(new Token(TokenType.GreaterThan, part));
                }
                else
                {
                    tokens.Add(new Token(TokenType.Identifier, part));
                }
            }

            return tokens;
        }
    }
}
