using NpgsqlParser.Lexing;
using NpgsqlParser.Models;
using Xunit;

namespace NpgsqlParser.UnitTests
{
    public class TokenizerTests
    {
        private readonly PostgreSqlTokenizer tokenizer = new();

        [Fact]
        public void Tokenize_SelectFromWhere_ShouldReturnsCorrectTokens()
        {
            var sql = "SELECT id, name FROM users WHERE age > 30";
            var tokens = tokenizer.Tokenize(sql);

            Assert.Equal("select", tokens[0].Value);
            Assert.Equal(TokenType.Select, tokens[0].Type);
            
            Assert.Equal("id", tokens[1].Value);
            Assert.Equal(TokenType.Identifier, tokens[1].Type);

            Assert.Equal("name", tokens[2].Value);
            Assert.Equal(TokenType.Identifier, tokens[2].Type);

            Assert.Equal("from", tokens[3].Value);
            Assert.Equal(TokenType.From, tokens[3].Type);
            
            Assert.Equal("users", tokens[4].Value);
            Assert.Equal(TokenType.Identifier, tokens[4].Type);

            Assert.Equal("where", tokens[5].Value);
            Assert.Equal(TokenType.Where, tokens[5].Type);
            
            Assert.Equal("age", tokens[6].Value);
            Assert.Equal(TokenType.Identifier, tokens[6].Type);
            
            Assert.Equal(">", tokens[7].Value);
            Assert.Equal(TokenType.GreaterThan, tokens[7].Type);
            
            Assert.Equal("30", tokens[8].Value);
            Assert.Equal(TokenType.Number, tokens[8].Type);
        }

        [Fact]
        public void Tokenize_SelectFromWhereStringLiterals_ShouldReturnsCorrectTokens()
        {
            var sql = "SELECT id, name FROM users WHERE name = 'Doğa'";
            var tokens = tokenizer.Tokenize(sql);

            Assert.Equal("select", tokens[0].Value);
            Assert.Equal(TokenType.Select, tokens[0].Type);

            Assert.Equal("id", tokens[1].Value);
            Assert.Equal(TokenType.Identifier, tokens[1].Type);

            Assert.Equal("name", tokens[2].Value);
            Assert.Equal(TokenType.Identifier, tokens[2].Type);
            
            Assert.Equal("from", tokens[3].Value);
            Assert.Equal(TokenType.From, tokens[3].Type);
            
            Assert.Equal("users", tokens[4].Value);
            Assert.Equal(TokenType.Identifier, tokens[4].Type);
            
            Assert.Equal("where", tokens[5].Value);
            Assert.Equal(TokenType.Where, tokens[5].Type);

            Assert.Equal("name", tokens[6].Value);
            Assert.Equal(TokenType.Identifier, tokens[6].Type);
            
            Assert.Equal("=", tokens[7].Value);
            Assert.Equal(TokenType.Equal, tokens[7].Type);
            
            Assert.Equal("Doğa", tokens[8].Value);
            Assert.Equal(TokenType.StringLiteral, tokens[8].Type);
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
