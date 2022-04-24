namespace SJP.Schematic.Oracle.Queries;

internal static class GetTableColumns
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
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
    COLUMN_NAME as ""{ nameof(Result.ColumnName) }"",
    DATA_TYPE_OWNER as ""{ nameof(Result.ColumnTypeSchema) }"",
    DATA_TYPE as ""{ nameof(Result.ColumnTypeName) }"",
    DATA_LENGTH as ""{ nameof(Result.DataLength) }"",
    DATA_PRECISION as ""{ nameof(Result.Precision) }"",
    DATA_SCALE as ""{ nameof(Result.Scale) }"",
    DATA_DEFAULT as ""{ nameof(Result.DefaultValue) }"",
    CHAR_LENGTH as ""{ nameof(Result.CharacterLength) }"",
    CHARACTER_SET_NAME as ""{ nameof(Result.Collation) }"",
    VIRTUAL_COLUMN as ""{ nameof(Result.IsComputed) }""
from SYS.ALL_TAB_COLS
where OWNER = :{ nameof(Query.SchemaName) } and TABLE_NAME = :{ nameof(Query.TableName) }
order by COLUMN_ID";
}