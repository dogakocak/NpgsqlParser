using PgSqlParser.Common.Result;
using PgSqlParser.Diagnostics;
using PgSqlParser.Models;

namespace PgSqlParser.Lexing;

public interface ISqlTokenizer
{
    Result<List<Token>, ParseError> Tokenize(string sql);
}
