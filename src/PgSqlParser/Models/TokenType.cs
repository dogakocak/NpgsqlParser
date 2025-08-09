namespace PgSqlParser.Models;

public enum TokenType
{
    Select, From, Where,
    Identifier, Number, Comma, Asterisk, Equal, GreaterThan, LessThan,
    StringLiteral,
    Semicolon,
    Unknown,
    EOF
}
