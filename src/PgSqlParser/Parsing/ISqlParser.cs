using PgSqlParser.Models;

namespace PgSqlParser.Parsing
{
    public interface ISqlParser
    {
        SelectStatement ParseSelect(List<Token> tokens);
    }
}
