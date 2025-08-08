using PgSqlParser.Models;

namespace PgSqlParser.Parsing
{
    public class PostgreSqlParser : ISqlParser
    {
        public SelectStatement ParseSelect(List<Token> tokens)
        {
            var statement = new SelectStatement();
            int i = 0;

            ExpectToken(tokens, i, TokenType.Select, "Expected 'SELECT' at the beginning of the statement.");
            i++;

            while (i < tokens.Count &&
                  (tokens[i].Type == TokenType.Identifier || tokens[i].Type == TokenType.Asterisk))
            {
                statement.Columns.Add(tokens[i++].Value);

                if (i < tokens.Count && tokens[i].Type == TokenType.Comma)
                {
                    i++;
                }
            }

            ExpectToken(tokens, i, TokenType.From, "Expected 'FROM' after column definitions.");
            i++;

            var tableToken = ExpectToken(tokens, i, TokenType.Identifier, "Expected table name after 'FROM'.");
            statement.Table = tableToken.Value;
            i++;

            if (i < tokens.Count && tokens[i].Type == TokenType.Where)
            {
                i++;

                if (i + 2 >= tokens.Count)
                {
                    throw new ArgumentException("Invalid WHERE clause.");
                }

                statement.WhereColumn = tokens[i++].Value;
                statement.WhereOperator = tokens[i++].Value;
                statement.WhereValue = tokens[i++].Value;
            }

            return statement;
        }
        private static Token ExpectToken(List<Token> tokens, int i, TokenType expectedType, string message)
        {
            if (i >= tokens.Count || tokens[i].Type != expectedType)
            {
                throw new ArgumentException(message);
            }
            return tokens[i];
        }
    }
}
