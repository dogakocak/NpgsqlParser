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

        while (i < input.Length)
        {
            char current = input[i];

            if (char.IsWhiteSpace(current) || current == ',' || current == ';')
            {
                i++;
                continue;
            }

            if (current == '\'') // Beginning string literal
            {
                int start = ++i;
                while (i < input.Length && (input[i] != '\'')) i++;

                if (i >= input.Length)
                {
                    return Result.Failure<List<Token>, ParseError>
                    (
                        new ParseError(ParseErrorCode.UnterminatedString, start -1)
                    );
                }

                string str = input[start..i];
                tokens.Add(new Token(TokenType.StringLiteral, str));
                i++; // closing '
                continue;
            }

            if (char.IsDigit(current))
            {
                int start = i;
                while (i < input.Length && char.IsDigit(input[i])) i++;
                tokens.Add(new Token(TokenType.Number, input[start..i]));
                continue;
            }

            if (current == '*')
            {
                tokens.Add(new Token(TokenType.Asterisk, "*"));
                i++;
                continue;
            }

            if (current == '=')
            {
                tokens.Add(new Token(TokenType.Equal, "="));
                i++;
                continue;
            }

            if (current == '>')
            {
                tokens.Add(new Token(TokenType.GreaterThan, ">"));
                i++;
                continue;
            }

            // Identifier or keyword
            if (char.IsLetter(current))
            {
                int start = i;
                while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_')) i++;

                string word = input[start..i].ToLower();

                if (Keywords.TryGetValue(word, out TokenType type))
                {
                    tokens.Add(new Token(type, word));
                }
                else
                {
                    tokens.Add(new Token(TokenType.Identifier, word));
                }

                continue;
            }

            return Result.Failure<List<Token>, ParseError>
            (
                new ParseError(ParseErrorCode.UnexpectedChar, i)
            );
        }

        return Result.Success<List<Token>, ParseError>(tokens);
    }
}
