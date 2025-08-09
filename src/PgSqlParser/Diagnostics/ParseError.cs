using System.Diagnostics;
using PgSqlParser.Models;

namespace PgSqlParser.Diagnostics;

[DebuggerDisplay("{Code} @ {Span.Start}")]
public readonly struct ParseError(ParseErrorCode code, TextSpan span,
                  TokenType? expected = null, TokenType? actual = null)
{
    public ParseErrorCode Code { get; } = code;
    public TextSpan Span { get; } = span;
    public TokenType? Expected { get; } = expected;
    public TokenType? Actual { get; } = actual;

    public bool IsError => Code != ParseErrorCode.None;
    public override string ToString() => $"{Code} at {Span.Start}";
}
