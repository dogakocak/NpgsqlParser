namespace PgSqlParser.Models
{
    public class SelectStatement
    {
        public List<string> Columns { get; set; } = [];
        public string Table { get; set; } = string.Empty;
        public string? WhereColumn { get; set; }
        public string? WhereOperator { get; set; }
        public string? WhereValue { get; set; }
    }
}
