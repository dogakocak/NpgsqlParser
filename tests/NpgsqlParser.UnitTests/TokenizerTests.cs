using NpgsqlParser.Lexing;
using NpgsqlParser.Models;
using Xunit;

namespace NpgsqlParser.UnitTests
{
    public class TokenizerTests
    {
        private readonly PostgreSqlTokenizer tokenizer = new();

        [Fact]
        public void Tokenize_SelectFromWhere_ReturnsCorrectTokens()
        {
            var sql = "SELECT id, name FROM users WHERE age > 30";
            var tokens = tokenizer.Tokenize(sql);

            Assert.Equal(TokenType.Select, tokens[0].Type);
            Assert.Equal("select", tokens[0].Value);
            
            Assert.Equal(TokenType.Identifier, tokens[1].Type); // id
            Assert.Equal(TokenType.Identifier, tokens[2].Type); // name
            Assert.Equal(TokenType.From, tokens[3].Type);
            Assert.Equal("from", tokens[3].Value);
            Assert.Equal(TokenType.Identifier, tokens[4].Type); // users
            Assert.Equal(TokenType.Where, tokens[5].Type);
            Assert.Equal("where", tokens[5].Value);
            Assert.Equal(TokenType.Identifier, tokens[6].Type); // age
            Assert.Equal("age", tokens[6].Value); // age
            Assert.Equal(TokenType.GreaterThan, tokens[7].Type);
            Assert.Equal(TokenType.Number, tokens[8].Type);
            Assert.Equal("30", tokens[8].Value);
        }

        [Fact]
        public void Tokenize_WithAsterisk_ReturnsAsretirskToken()
        {
            var sql = "SELECT * FROM customers";
            var tokens = tokenizer.Tokenize(sql);

            Assert.Contains(tokens, t => t.Type == TokenType.Asterisk);
        }

        [Fact]
        public void Tokenize_UnknownToken_ReturnsIdentifier()
        {
            var sql = "SELECT abc FROM table";
            var tokens = tokenizer.Tokenize(sql);

            Assert.Equal("abc", tokens[1].Value);
            Assert.Equal(TokenType.Identifier, tokens[1].Type);
        }
    }
}
