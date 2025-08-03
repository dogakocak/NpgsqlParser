using NpgsqlParser.Lexing;
using NpgsqlParser.Parsing;
using Xunit;

namespace NpgsqlParser.UnitTests
{
    public class ParserTests
    {
        private readonly PostgreSqlTokenizer tokenizer = new();
        private readonly PostgreSqlParser parser = new();

        [Fact]
        public void ParseSelect_SimpleSelect_ParsesCorrectly()
        {
            var sql = "SELECT id, name FROM users WHERE age > 18";
            var tokens = tokenizer.Tokenize(sql);
            var statement = parser.ParseSelect(tokens);

            Assert.Equal(2, statement.Columns.Count);
            Assert.Contains("id", statement.Columns);
            Assert.Contains("name", statement.Columns);
            Assert.Contains("users", statement.Table);
            Assert.Contains("age", statement.WhereColumn);
            Assert.Contains(">", statement.WhereOperator);
            Assert.Contains("18", statement.WhereValue);
        }

        [Fact]
        public void ParseSelect_SimpleSelectStringLiterals_ParsesCorrectly()
        {
            var sql = "SELECT id, name FROM users WHERE name = \'Doğa\'";
            var tokens = tokenizer.Tokenize(sql);
            var statement = parser.ParseSelect(tokens);

            Assert.Equal(2, statement.Columns.Count);
            Assert.Contains("id", statement.Columns);
            Assert.Contains("name", statement.Columns);
            Assert.Contains("users", statement.Table);
            Assert.Contains("name", statement.WhereColumn);
            Assert.Contains("=", statement.WhereOperator);
            Assert.Contains("Doğa", statement.WhereValue);
        }

        [Fact]
        public void ParseSelect_ThrowsIfNotSelect()
        {
            var tokens = tokenizer.Tokenize("FROM users");

            Assert.Throws<ArgumentException>(() => parser.ParseSelect(tokens));
        }

        [Fact]
        public void ParseSelect_ThrowsIfNoFrom()
        {
            var tokens = tokenizer.Tokenize("SELECT id name");

            Assert.Throws<ArgumentException>(() => parser.ParseSelect(tokens));
        }
    }
}
