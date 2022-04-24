namespace SJP.Schematic.Oracle.Queries;

internal static class GetMaterializedViewColumns
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string? ColumnName { get; init; }

        public string? ColumnTypeSchema { get; init; }

        public string? ColumnTypeName { get; init; }

        public int DataLength { get; init; }

        public int CharacterLength { get; init; }

        public int Precision { get; init; }

        public int Scale { get; init; }

        public string? Collation { get; init; }

        public string? IsComputed { get; init; }

        public string? DefaultValue { get; init; }
    }

    internal const string Sql = @$"
select
    atc.COLUMN_NAME as ""{ nameof(Result.ColumnName) }"",
    atc.DATA_TYPE_OWNER as ""{ nameof(Result.ColumnTypeSchema) }"",
    atc.DATA_TYPE as ""{ nameof(Result.ColumnTypeName) }"",
    atc.DATA_LENGTH as ""{ nameof(Result.DataLength) }"",
    atc.DATA_PRECISION as ""{ nameof(Result.Precision) }"",
    atc.DATA_SCALE as ""{ nameof(Result.Scale) }"",
    atc.DATA_DEFAULT as ""{ nameof(Result.DefaultValue) }"",
    atc.CHAR_LENGTH as ""{ nameof(Result.CharacterLength) }"",
    atc.CHARACTER_SET_NAME as ""{ nameof(Result.Collation) }"",
    atc.VIRTUAL_COLUMN as ""{ nameof(Result.IsComputed) }""
from SYS.ALL_TAB_COLS atc
where OWNER = :{ nameof(Query.SchemaName) } and TABLE_NAME = :{ nameof(Query.ViewName) }
order by atc.COLUMN_ID";
}