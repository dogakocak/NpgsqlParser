using PgSqlParser.Diagnostics;
using PgSqlParser.Lexing;
using PgSqlParser.Models;
using Xunit;

namespace PgSqlParser.UnitTests;

public class TokenizerTests
{
    private readonly PostgreSqlTokenizer tokenizer = new();

    [Fact]
    public void Tokenize_SelectFromWhere_ShouldReturnsCorrectTokens()
    {
        var sql = "SELECT id, name FROM users WHERE age > 30";
        var tokenizeResult = tokenizer.Tokenize(sql);

        Assert.True(tokenizeResult.IsSuccess);

        var tokens = tokenizeResult.Value;

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
    public void Tokenize_WhenStringLiteralIsUnterminated_ShouldReturnError()
    {
        var sql = "SELECT id, name FROM users WHERE name = 'Doga";
        int position = sql.IndexOf('\'');

        var tokenizeResult = tokenizer.Tokenize(sql);

        Assert.True(tokenizeResult.IsFailure);
        Assert.Equal(ParseErrorCode.UnterminatedString, tokenizeResult.Error.Code);
        Assert.Equal(position, tokenizeResult.Error.Position);
    }

    [Theory]
    [InlineData("\0", '\0')]
    [InlineData("\uFFFF", '\uFFFF')]
    [InlineData("SELECT id, name FROM users \0", '\0')]
    [InlineData("abc\0def", '\0')]              
    [InlineData("price = 42 \uFFFF", '\uFFFF')]
    public void Tokenize_ShouldFail_WithUnexpectedChar_OnEdgeChars(string input, char unexpectedChar)
    {
        var result = tokenizer.Tokenize(input);

        Assert.True(result.IsFailure);
        Assert.Equal(ParseErrorCode.UnexpectedChar, result.Error.Code);

        int unexpectedCharPosition = input.IndexOf(unexpectedChar);

        Assert.Equal(unexpectedCharPosition, result.Error.Position);
    }

    [Fact]
    public void Tokenize_ShouldReport_ExactPosition_AfterValidPrefix()
    {
        var input = "SELECT id, name FROM users WHERE id = 123 " + '\0';

        var result = tokenizer.Tokenize(input);

        Assert.True(result.IsFailure);
        Assert.Equal(ParseErrorCode.UnexpectedChar, result.Error.Code);

        var expected = input.IndexOf('\0');
        Assert.Equal(expected, result.Error.Position);
    }

    [Fact]
    public void Tokenize_SelectFromWhereStringLiterals_ShouldReturnsCorrectTokens()
    {
        var sql = "SELECT id, name FROM users WHERE name = 'Doğa'";
        var tokenizeResult = tokenizer.Tokenize(sql);
        var tokens = tokenizeResult.Value;

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
        var tokenizeResult = tokenizer.Tokenize(sql);

        var tokens = tokenizeResult.Value;

        Assert.Contains(tokens, t => t.Type == TokenType.Asterisk);
    }

    [Fact]
    public void Tokenize_UnknownToken_ReturnsIdentifier()
    {
        var sql = "SELECT abc FROM table";
        var tokenizeResult = tokenizer.Tokenize(sql);

        var tokens = tokenizeResult.Value;

        Assert.Equal("abc", tokens[1].Value);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);
    }
}
