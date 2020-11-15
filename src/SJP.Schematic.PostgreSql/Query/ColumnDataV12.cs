namespace SJP.Schematic.PostgreSql.Query
{
    internal sealed record ColumnDataV12 : ColumnDataV10
    {
        /// <summary>
        /// If the column is a generated column, then <c>ALWAYS</c>, else <c>NEVER</c>.
        /// </summary>
        public string? IsGenerated { get; init; }

        /// <summary>
        /// If the column is a generated column, then the generation expression, else null.
        /// </summary>
        public string? GenerationExpression { get; init; }
    }
}
