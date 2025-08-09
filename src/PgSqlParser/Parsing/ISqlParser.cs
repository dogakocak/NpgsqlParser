using PgSqlParser.Common.Result;
using PgSqlParser.Diagnostics;
using PgSqlParser.Models;

namespace PgSqlParser.Parsing
{
    public interface ISqlParser
    {
        Result<SelectStatement, ParseError> ParseSelect(IReadOnlyList<Token> tokens);
    }
}
