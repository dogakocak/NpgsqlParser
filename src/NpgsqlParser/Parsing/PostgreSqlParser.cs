using NpgsqlParser.Models;

namespace NpgsqlParser.Parsing
{
    public class PostgreSqlParser : ISqlParser
    {
        public SelectStatement ParseSelect(List<Token> tokens)
        {
            var statement = new SelectStatement();
            int i = 0;

            // Now, we wait for only SELECT statements.
            if (tokens[i].Type != TokenType.Select)
            {
                throw new ArgumentException("Expected 'SELECT' at the beginning of the statement.");
            }

            i++;

            while (tokens[i].Type == TokenType.Identifier || tokens[i].Type == TokenType.Asterisk)
            {
                statement.Columns.Add(tokens[i].Value);
                i++;

                if (tokens[i].Type == TokenType.Comma)
                {
                    i++;
                }
            }

            if (tokens[i].Type != TokenType.From)
            {
                throw new ArgumentException("Expected 'FROM' after column definitions.");
            }

            i++;

            if (tokens[i].Type != TokenType.Identifier)
            {
                throw new ArgumentException("Expected table name after 'FROM'.");
            }

            statement.Table = tokens[i].Value;
            i++;

            if (i < tokens.Count && tokens[i].Type == TokenType.Where)
            {
                i++;
                statement.WhereColumn = tokens[i++].Value;
                statement.WhereOperator = tokens[i++].Value;
                statement.WhereValue = tokens[i++].Value;
            }

            return statement;
        }
    }
}
