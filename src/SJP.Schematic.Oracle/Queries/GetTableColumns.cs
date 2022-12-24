using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetTableColumns
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string? ColumnName { get; init; }

        public required string? ColumnTypeSchema { get; init; }

        public required string? ColumnTypeName { get; init; }

        public required int DataLength { get; init; }

        public required int CharacterLength { get; init; }

        public required int Precision { get; init; }

        public required int Scale { get; init; }

        public required string? Collation { get; init; }

        public required string? IsComputed { get; init; }

        public required string? DefaultValue { get; init; }
    }

    internal const string Sql = @$"
select
    COLUMN_NAME as ""{nameof(Result.ColumnName)}"",
    DATA_TYPE_OWNER as ""{nameof(Result.ColumnTypeSchema)}"",
    DATA_TYPE as ""{nameof(Result.ColumnTypeName)}"",
    DATA_LENGTH as ""{nameof(Result.DataLength)}"",
    DATA_PRECISION as ""{nameof(Result.Precision)}"",
    DATA_SCALE as ""{nameof(Result.Scale)}"",
    DATA_DEFAULT as ""{nameof(Result.DefaultValue)}"",
    CHAR_LENGTH as ""{nameof(Result.CharacterLength)}"",
    CHARACTER_SET_NAME as ""{nameof(Result.Collation)}"",
    VIRTUAL_COLUMN as ""{nameof(Result.IsComputed)}""
from SYS.ALL_TAB_COLS
where OWNER = :{nameof(Query.SchemaName)} and TABLE_NAME = :{nameof(Query.TableName)}
order by COLUMN_ID";
}