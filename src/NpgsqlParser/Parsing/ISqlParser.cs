using NpgsqlParser.Models;

namespace NpgsqlParser.Parsing
{
    public interface ISqlParser
    {
        SelectStatement ParseSelect(List<Token> tokens);
    }
}
