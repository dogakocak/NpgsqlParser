using NpgsqlParser.Models;

namespace NpgsqlParser.Lexing
{
    public interface ISqlTokenizer
    {
        List<Token> Tokenize(string sql);
    }
}
