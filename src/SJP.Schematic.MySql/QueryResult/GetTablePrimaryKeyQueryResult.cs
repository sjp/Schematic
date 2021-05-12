using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.QueryResult
{
    internal sealed record GetTablePrimaryKeyQueryResult
    {
        public string ConstraintName { get; init; } = default!;

        public string ColumnName { get; init; } = default!;
    }
}
