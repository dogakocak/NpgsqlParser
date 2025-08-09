using PgSqlParser.Diagnostics;
using PgSqlParser.Lexing;
using PgSqlParser.Models;
using Xunit;

namespace PgSqlParser.UnitTests;

public class TokenizerTests
{
    private readonly PostgreSqlTokenizer tokenizer = new();

    [Fact]
    public void Tokenize_SelectFromWhere_ShouldReturnCorrectTokens()
    {
        // Arrange
        var sql = "SELECT id, name FROM users WHERE age > 30";

        // Act
        var result = tokenizer.Tokenize(sql);

        // Assert
        Assert.True(result.IsSuccess);

        var tokens = result.Value;
        Assert.Equal("select", tokens[0].Value);
        Assert.Equal(TokenType.Select, tokens[0].Type);

        Assert.Equal("id", tokens[1].Value);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);

        Assert.Equal(",", tokens[2].Value);
        Assert.Equal(TokenType.Comma, tokens[2].Type);

        Assert.Equal("name", tokens[3].Value);
        Assert.Equal(TokenType.Identifier, tokens[3].Type);

        Assert.Equal("from", tokens[4].Value);
        Assert.Equal(TokenType.From, tokens[4].Type);

        Assert.Equal("users", tokens[5].Value);
        Assert.Equal(TokenType.Identifier, tokens[5].Type);

        Assert.Equal("where", tokens[6].Value);
        Assert.Equal(TokenType.Where, tokens[6].Type);

        Assert.Equal("age", tokens[7].Value);
        Assert.Equal(TokenType.Identifier, tokens[7].Type);

        Assert.Equal(">", tokens[8].Value);
        Assert.Equal(TokenType.GreaterThan, tokens[8].Type);

        Assert.Equal("30", tokens[9].Value);
        Assert.Equal(TokenType.Number, tokens[9].Type);
    }

    [Fact]
    public void Tokenize_WhenStringLiteralIsUnterminated_ShouldReturnError()
    {
        // Arrange
        var sql = "SELECT id, name FROM users WHERE name = 'Doga";
        int position = sql.IndexOf('\'');

        // Act
        var result = tokenizer.Tokenize(sql);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ParseErrorCode.UnterminatedString, result.Error.Code);
        Assert.Equal(position, result.Error.Span.Start);
    }

    [Theory]
    [InlineData("\0", '\0')]
    [InlineData("\uFFFF", '\uFFFF')]
    [InlineData("SELECT id, name FROM users \0", '\0')]
    [InlineData("abc\0def", '\0')]
    [InlineData("price = 42 \uFFFF", '\uFFFF')]
    public void Tokenize_ShouldFail_WithUnexpectedChar_OnEdgeChars(string input, char unexpectedChar)
    {
        // Arrange
        var expectedPos = input.IndexOf(unexpectedChar);

        // Act
        var result = tokenizer.Tokenize(input);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ParseErrorCode.UnexpectedChar, result.Error.Code);
        Assert.Equal(expectedPos, result.Error.Span.Start);
    }

    [Fact]
    public void Tokenize_InvalidChar_ReturnsUnexpectedChar()
    {
        // Arrange
        var input = "SELECT id, name FROM users WHERE id = 123 " + '\0';
        var expectedPos = input.IndexOf('\0');

        // Act
        var result = tokenizer.Tokenize(input);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ParseErrorCode.UnexpectedChar, result.Error.Code);
        Assert.Equal(expectedPos, result.Error.Span.Start);
    }

    [Fact]
    public void Tokenize_SelectFromWhereStringLiterals_ShouldReturnCorrectTokens()
    {
        // Arrange
        var sql = "SELECT id, name FROM users WHERE name = 'Doğa'";

        // Act
        var result = tokenizer.Tokenize(sql);

        // Assert
        Assert.True(result.IsSuccess);

        var tokens = result.Value;
        Assert.Equal("select", tokens[0].Value);
        Assert.Equal(TokenType.Select, tokens[0].Type);

        Assert.Equal("id", tokens[1].Value);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);

        Assert.Equal(",", tokens[2].Value);
        Assert.Equal(TokenType.Comma, tokens[2].Type);

        Assert.Equal("name", tokens[3].Value);
        Assert.Equal(TokenType.Identifier, tokens[3].Type);

        Assert.Equal("from", tokens[4].Value);
        Assert.Equal(TokenType.From, tokens[4].Type);

        Assert.Equal("users", tokens[5].Value);
        Assert.Equal(TokenType.Identifier, tokens[5].Type);

        Assert.Equal("where", tokens[6].Value);
        Assert.Equal(TokenType.Where, tokens[6].Type);

        Assert.Equal("name", tokens[7].Value);
        Assert.Equal(TokenType.Identifier, tokens[7].Type);

        Assert.Equal("=", tokens[8].Value);
        Assert.Equal(TokenType.Equal, tokens[8].Type);

        Assert.Equal("Doğa", tokens[9].Value);
        Assert.Equal(TokenType.StringLiteral, tokens[9].Type);
    }

    [Fact]
    public void Tokenize_WithAsterisk_ReturnsAsteriskToken()
    {
        // Arrange
        var sql = "SELECT * FROM customers";

        // Act
        var result = tokenizer.Tokenize(sql);

        // Assert
        Assert.True(result.IsSuccess);
        var tokens = result.Value;
        Assert.Contains(tokens, t => t.Type == TokenType.Asterisk);
    }

    [Fact]
    public void Tokenize_UnknownToken_ReturnsIdentifier()
    {
        // Arrange
        var sql = "SELECT abc FROM table";

        // Act
        var result = tokenizer.Tokenize(sql);

        // Assert
        Assert.True(result.IsSuccess);
        var tokens = result.Value;
        Assert.Equal("abc", tokens[1].Value);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);
    }

    [Fact]
    public void Tokenize_InvalidOperatorInWhere_ReturnsError()
    {
        // Arrange
        var sql = "SELECT id FROM users WHERE age ? 18";

        // Act
        var tokenizeResult = tokenizer.Tokenize(sql);

        // Assert
        Assert.True(tokenizeResult.IsFailure);
        Assert.Equal(ParseErrorCode.UnexpectedChar, tokenizeResult.Error.Code);
    }
}
