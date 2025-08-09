namespace PgSqlParser.Models;

public sealed class Token(TokenType type, string value, TextSpan span)
{
    public TokenType Type { get; } = type;
    public string Value { get; } = value;
    public TextSpan Span { get; } = span;

    public override string ToString() => $"{Type}: '{Value}' @ {Span.Start}";
}

