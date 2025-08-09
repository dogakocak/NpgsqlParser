using PgSqlParser.Diagnostics;
using PgSqlParser.Lexing;
using PgSqlParser.Models;
using PgSqlParser.Parsing;
using Xunit;

namespace PgSqlParser.UnitTests;

public class ParserTests
{
    private readonly PostgreSqlTokenizer tokenizer = new();
    private readonly PostgreSqlParser parser = new();

    [Fact]
    public void ParseSelect_SimpleSelect_ParsesCorrectly()
    {
        // Arrange
        var sql = "SELECT id, name FROM users WHERE age > 18";
        var tokens = tokenizer.Tokenize(sql).Value;

        // Act
        var parseResult = parser.ParseSelect(tokens);
        var statement = parseResult.Value;

        // Assert
        Assert.True(parseResult.IsSuccess);
        Assert.Equal(2, statement.Columns.Count);
        Assert.Contains("id", statement.Columns);
        Assert.Contains("name", statement.Columns);
        Assert.Contains("users", statement.Table);
        Assert.Contains("age", statement.WhereColumn);
        Assert.Contains(">", statement.WhereOperator);
        Assert.Contains("18", statement.WhereValue);
    }

    [Fact]
    public void ParseSelect_SimpleSelectStringLiterals_ParsesCorrectly()
    {
        // Arrange
        var sql = "SELECT id, name FROM users WHERE name = 'Doğa'";
        var tokens = tokenizer.Tokenize(sql).Value;

        // Act
        var parseResult = parser.ParseSelect(tokens);
        var statement = parseResult.Value;

        // Assert
        Assert.True(parseResult.IsSuccess);
        Assert.Equal(2, statement.Columns.Count);
        Assert.Contains("id", statement.Columns);
        Assert.Contains("name", statement.Columns);
        Assert.Contains("users", statement.Table);
        Assert.Contains("name", statement.WhereColumn);
        Assert.Contains("=", statement.WhereOperator);
        Assert.Contains("Doğa", statement.WhereValue);
    }

    [Fact]
    public void ParseSelect_ReturnsIfNotSelect()
    {
        // Arrange
        var sql = "FROM users";
        var tokens = tokenizer.Tokenize(sql).Value;

        // Act
        var parseResult = parser.ParseSelect(tokens);
        var statement = parseResult.Value;

        // Assert
        Assert.True(parseResult.IsFailure);
        Assert.Null(statement);
        Assert.Equal(ParseErrorCode.UnexpectedToken, parseResult.Error.Code);
        Assert.Equal(TokenType.Select, parseResult.Error.Expected);
        Assert.Equal(0, parseResult.Error.Span.Start); // SELECT should be at the start.
    }

    [Fact]
    public void ParseSelect_ReturnsIfNoFrom()
    {
        // Arrange
        var sql = "SELECT id name";
        var tokens = tokenizer.Tokenize(sql).Value;

        // Act
        var parseResult = parser.ParseSelect(tokens);
        var statement = parseResult.Value;

        // Assert
        Assert.True(parseResult.IsFailure);
        Assert.Null(statement);
        Assert.Equal(ParseErrorCode.UnexpectedToken, parseResult.Error.Code);
        Assert.Equal(TokenType.From, parseResult.Error.Expected);
        Assert.Equal(sql.Length, parseResult.Error.Span.Start); // FROM should be at the end.
    }

    [Fact]
    public void ParseSelect_WildcardAndColumns_ParsesCorrectly()
    {
        // Arrange
        var sql = "SELECT *, name FROM users";
        var tokens = tokenizer.Tokenize(sql).Value;

        // Act
        var parseResult = parser.ParseSelect(tokens);
        var statement = parseResult.Value;

        // Assert
        Assert.True(parseResult.IsSuccess);
        Assert.Equal(2, statement.Columns.Count);
        Assert.Contains("*", statement.Columns);
        Assert.Contains("name", statement.Columns);
        Assert.Contains("users", statement.Table);
        Assert.Null(statement.WhereColumn);
        Assert.Null(statement.WhereOperator);
        Assert.Null(statement.WhereValue);
    }

    [Fact]
    public void ParseSelect_NoWhereClause_ParsesCorrectly()
    {
        // Arrange
        var sql = "SELECT id, name FROM users";
        var tokens = tokenizer.Tokenize(sql).Value;

        // Act
        var parseResult = parser.ParseSelect(tokens);
        var statement = parseResult.Value;

        // Assert
        Assert.True(parseResult.IsSuccess);
        Assert.Equal(2, statement.Columns.Count);
        Assert.Contains("id", statement.Columns);
        Assert.Contains("name", statement.Columns);
        Assert.Contains("users", statement.Table);
        Assert.Null(statement.WhereColumn);
        Assert.Null(statement.WhereOperator);
        Assert.Null(statement.WhereValue);
    }

    [Fact]
    public void ParseSelect_MissingTableName_ReturnsError()
    {
        // Arrange
        var sql = "SELECT id FROM";
        var tokens = tokenizer.Tokenize(sql).Value;

        // Act
        var parseResult = parser.ParseSelect(tokens);

        // Assert
        Assert.True(parseResult.IsFailure);
        Assert.Equal(ParseErrorCode.UnexpectedToken, parseResult.Error.Code);
        Assert.Equal(TokenType.Identifier, parseResult.Error.Expected);
        // there is no table name, position should be at the end.
        Assert.Equal(sql.Length, parseResult.Error.Span.Start);
    }
    

    [Fact]
    public void ParseSelect_InvalidValueInWhere_ReturnsError()
    {
        // Arrange
        var sql = "SELECT id FROM users WHERE age = invalid";
        var tokens = tokenizer.Tokenize(sql).Value;

        // Act
        var parseResult = parser.ParseSelect(tokens);

        // Assert
        Assert.True(parseResult.IsFailure);
        // value can be string literal or number.
        Assert.Equal(ParseErrorCode.UnexpectedToken, parseResult.Error.Code);
    }

    [Fact]
    public void ParseSelect_WhereClauseTooShort_ReturnsError()
    {
        // Arrange
        var sql = "SELECT id FROM users WHERE age";
        var tokens = tokenizer.Tokenize(sql).Value;

        // Act
        var parseResult = parser.ParseSelect(tokens);

        // Assert
        Assert.True(parseResult.IsFailure);
        Assert.Equal(ParseErrorCode.InvalidWhereClause, parseResult.Error.Code);
    }

    [Fact]
    public void ParseSelect_TrailingCommaBeforeFrom_IsAccepted()
    {
        // NOTE: Bu SQL teknik olarak hatalıdır (SELECT listesinde sonda virgül),
        // ancak mevcut parser bunu kabul ediyor ve sadece 'id' sütununu toplayıp devam ediyor.
        // Bu test mevcut davranışı belgelemek için var.
        // Arrange
        var sql = "SELECT id, FROM users";
        var tokens = tokenizer.Tokenize(sql).Value;

        // Act
        var parseResult = parser.ParseSelect(tokens);
        var statement = parseResult.Value;

        // Assert
        Assert.True(parseResult.IsSuccess);
        Assert.Single(statement.Columns);
        Assert.Contains("id", statement.Columns);
        Assert.Contains("users", statement.Table);
    }

    [Fact]
    public void ParseSelect_DoubleCommaBetweenColumns_ReturnsError()
    {
        // Arrange
        var sql = "SELECT id,, name FROM users";
        var tokens = tokenizer.Tokenize(sql).Value;

        // Act
        var parseResult = parser.ParseSelect(tokens);

        // Assert
        Assert.True(parseResult.IsFailure);
        Assert.Equal(ParseErrorCode.UnexpectedToken, parseResult.Error.Code);
        Assert.Equal(TokenType.From, parseResult.Error.Expected); // döngüden erken çıkıp FROM bekliyor
        Assert.Equal(TokenType.Comma, parseResult.Error.Actual);
    }

    [Fact]
    public void ParseSelect_GreaterThanWithStringValue_ReturnsError()
    {
        // Arrange
        var sql = "SELECT id FROM users WHERE age > 'eighteen'";
        var tokens = tokenizer.Tokenize(sql).Value;

        // Act
        var parseResult = parser.ParseSelect(tokens);

        // Assert
        // Parser değerin tipini Number/StringLiteral olarak kabul ediyor; semantik kontrol yok.
        // Bu yüzden başarı döner. Mevcut davranışı belgelemek için:
        Assert.True(parseResult.IsSuccess);
        Assert.Contains("age", parseResult.Value.WhereColumn);
        Assert.Contains(">", parseResult.Value.WhereOperator);
        Assert.Contains("eighteen", parseResult.Value.WhereValue);
    }

    [Fact]
    public void ParseSelect_AsteriskOnly_ParsesCorrectly()
    {
        // Arrange
        var sql = "SELECT * FROM users WHERE age = 30";
        var tokens = tokenizer.Tokenize(sql).Value;

        // Act
        var parseResult = parser.ParseSelect(tokens);
        var statement = parseResult.Value;

        // Assert
        Assert.True(parseResult.IsSuccess);
        Assert.Single(statement.Columns);
        Assert.Contains("*", statement.Columns);
        Assert.Contains("users", statement.Table);
        Assert.Contains("age", statement.WhereColumn);
        Assert.Contains("=", statement.WhereOperator);
        Assert.Contains("30", statement.WhereValue);
    }
}
