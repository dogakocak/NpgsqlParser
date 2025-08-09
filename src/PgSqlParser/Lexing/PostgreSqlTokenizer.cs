using PgSqlParser.Common.Result;
using PgSqlParser.Diagnostics;
using PgSqlParser.Models;

namespace PgSqlParser.Lexing;

public class PostgreSqlTokenizer : ISqlTokenizer
{
    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        { "select", TokenType.Select },
        { "from", TokenType.From },
        { "where", TokenType.Where }
    };

    public Result<List<Token>, ParseError> Tokenize(string input)
    {
        var tokens = new List<Token>();
        int i = 0;

        static TextSpan Span(int start, int length) => new(start, length);

        while (i < input.Length)
        {
            char current = input[i];

            // whitespace
            if (char.IsWhiteSpace(current))
            {
                i++;
                continue;
            }

            // comma
            if (current == ',')
            {
                tokens.Add(new Token(TokenType.Comma, ",", Span(i, 1)));
                i++;
                continue;
            }

            // semicolon
            if (current == ';')
            {
                tokens.Add(new Token(TokenType.Semicolon, ";", Span(i, 1)));
                i++;
                continue;
            }

            // string literal: '...'
            if (current == '\'')
            {
                int start = i++;
                while (i < input.Length && input[i] != '\'') i++;

                if (i >= input.Length)
                {
                    return Result.Failure<List<Token>, ParseError>(
                        new ParseError(ParseErrorCode.UnterminatedString, Span(start, i - start))
                    );
                }

                string value = input[(start + 1)..i];
                i++; // closing '
                tokens.Add(new Token(TokenType.StringLiteral, value, Span(start, i - start)));
                continue;
            }

            // number
            if (char.IsDigit(current))
            {
                int start = i;
                while (i < input.Length && char.IsDigit(input[i])) i++;
                tokens.Add(new Token(TokenType.Number, input[start..i], Span(start, i - start)));
                continue;
            }

            // operators
            if (current == '*')
            {
                tokens.Add(new Token(TokenType.Asterisk, "*", Span(i, 1)));
                i++;
                continue;
            }
            if (current == '=')
            {
                tokens.Add(new Token(TokenType.Equal, "=", Span(i, 1)));
                i++;
                continue;
            }
            if (current == '>')
            {
                tokens.Add(new Token(TokenType.GreaterThan, ">", Span(i, 1)));
                i++;
                continue;
            }
            if (current == '<')
            {
                tokens.Add(new Token(TokenType.LessThan, ">", Span(i, 1)));
                i++;
                continue;
            }

            // identifier or keyword
            if (char.IsLetter(current))
            {
                int start = i;
                while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_')) i++;

                string word = input[start..i].ToLower();
                if (Keywords.TryGetValue(word, out TokenType type))
                    tokens.Add(new Token(type, word, Span(start, i - start)));
                else
                    tokens.Add(new Token(TokenType.Identifier, word, Span(start, i - start)));

                continue;
            }

            // unexpected char
            return Result.Failure<List<Token>, ParseError>(
                new ParseError(ParseErrorCode.UnexpectedChar, Span(i, 1))
            );
        }

        // EOF token for parser
        tokens.Add(new Token(TokenType.EOF, string.Empty, Span(input.Length, 0)));

        return Result.Success<List<Token>, ParseError>(tokens);
    }
}