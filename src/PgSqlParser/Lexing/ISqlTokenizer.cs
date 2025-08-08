using PgSqlParser.Models;

namespace PgSqlParser.Lexing
{
    public interface ISqlTokenizer
    {
        List<Token> Tokenize(string sql);
    }
}
