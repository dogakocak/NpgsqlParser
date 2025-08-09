using PgSqlParser.Common.Result;
using PgSqlParser.Diagnostics;
using PgSqlParser.Models;

namespace PgSqlParser.Parsing
{
    public class PostgreSqlParser : ISqlParser
    {
        public Result<SelectStatement, ParseError> ParseSelect(IReadOnlyList<Token> tokens)
        {
            var statement = new SelectStatement();
            int i = 0;

            // SELECT
            var selectTokenResult = ExpectToken(tokens, i, TokenType.Select);

            if (selectTokenResult.IsFailure)
            {
                return Result.Failure<SelectStatement, ParseError>(selectTokenResult.Error);
            }

            i++;

            // columns
            while (i < tokens.Count &&
                   (tokens[i].Type == TokenType.Identifier || tokens[i].Type == TokenType.Asterisk))
            {
                statement.Columns.Add(tokens[i++].Value);

                if (i < tokens.Count && tokens[i].Type == TokenType.Comma)
                    i++;
            }

            // FROM
            var fromTokenResult = ExpectToken(tokens, i, TokenType.From);
            if (fromTokenResult.IsFailure)
            {
                return Result.Failure<SelectStatement, ParseError>(fromTokenResult.Error);
            }

            i++;

            // table name
            var tableTokenResult = ExpectToken(tokens, i, TokenType.Identifier);
            if (tableTokenResult.IsFailure)
            {
                return Result.Failure<SelectStatement, ParseError>(tableTokenResult.Error);
            }

            statement.Table = tableTokenResult.Value.Value;
            i++;

            // optional WHERE clause
            if (i < tokens.Count && tokens[i].Type == TokenType.Where)
            {
                i++;

                // need at least 3 tokens: col op value
                if (i + 2 >= tokens.Count)
                {
                    var eof = tokens[^1]; // EOF token
                    return Result.Failure<SelectStatement, ParseError>(
                        new ParseError(ParseErrorCode.InvalidWhereClause, eof.Span)
                    );
                }

                var whereColResult = ExpectToken(tokens, i++, TokenType.Identifier);
                if (whereColResult.IsFailure)
                {
                    return Result.Failure<SelectStatement, ParseError>(whereColResult.Error);
                }

                // operator
                if (i >= tokens.Count || (tokens[i].Type != TokenType.Equal && tokens[i].Type != TokenType.GreaterThan && tokens[i].Type != TokenType.LessThan))
                {
                    var span = i < tokens.Count ? tokens[i].Span : tokens[^1].Span;
                    return Result.Failure<SelectStatement, ParseError>(
                        new ParseError(ParseErrorCode.UnexpectedToken, span, expected: TokenType.Equal, actual: i < tokens.Count ? tokens[i].Type : TokenType.EOF)
                    );
                }
                var operatorToken = tokens[i++];

                // value: number or string
                if (i >= tokens.Count || (tokens[i].Type != TokenType.Number && tokens[i].Type != TokenType.StringLiteral))
                {
                    var span = i < tokens.Count ? tokens[i].Span : tokens[^1].Span;
                    return Result.Failure<SelectStatement, ParseError>(
                        new ParseError(ParseErrorCode.UnexpectedToken, span)
                    );
                }
                var valueToken = tokens[i++];

                statement.WhereColumn = whereColResult.Value.Value;
                statement.WhereOperator = operatorToken.Value;
                statement.WhereValue = valueToken.Value;
            }

            return Result.Success<SelectStatement, ParseError>(statement);
        }

        private static Result<Token, ParseError> ExpectToken(IReadOnlyList<Token> tokens, int index, TokenType expectedType)
        {
            if (index >= tokens.Count)
            {
                var eof = tokens[^1];
                return Result.Failure<Token, ParseError>(
                    new ParseError(ParseErrorCode.UnexpectedToken, eof.Span, expected: expectedType, actual: TokenType.EOF)
                );
            }

            var token = tokens[index];
            if (token.Type != expectedType)
            {
                return Result.Failure<Token, ParseError>(
                    new ParseError(ParseErrorCode.UnexpectedToken, token.Span, expected: expectedType, actual: token.Type)
                );
            }

            return Result.Success<Token, ParseError>(token);
        }
    }
}
