using System.Diagnostics;

namespace PgSqlParser.Diagnostics
{
    [DebuggerDisplay("{Code} @ {Position}")]
    public readonly struct ParseError
    {
        public ParseErrorCode Code { get; }
        public int Position { get; }

        public ParseError(ParseErrorCode code, int position)
            => (Code, Position) = (code, position);

        public bool IsError => Code != ParseErrorCode.None;
        public override string ToString() => $"{Code} at {Position}";
    }
}
