using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetMaterializedViewColumns
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
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
    atc.COLUMN_NAME as ""{nameof(Result.ColumnName)}"",
    atc.DATA_TYPE_OWNER as ""{nameof(Result.ColumnTypeSchema)}"",
    atc.DATA_TYPE as ""{nameof(Result.ColumnTypeName)}"",
    atc.DATA_LENGTH as ""{nameof(Result.DataLength)}"",
    atc.DATA_PRECISION as ""{nameof(Result.Precision)}"",
    atc.DATA_SCALE as ""{nameof(Result.Scale)}"",
    atc.DATA_DEFAULT as ""{nameof(Result.DefaultValue)}"",
    atc.CHAR_LENGTH as ""{nameof(Result.CharacterLength)}"",
    atc.CHARACTER_SET_NAME as ""{nameof(Result.Collation)}"",
    atc.VIRTUAL_COLUMN as ""{nameof(Result.IsComputed)}""
from SYS.ALL_TAB_COLS atc
where OWNER = :{nameof(Query.SchemaName)} and TABLE_NAME = :{nameof(Query.ViewName)}
order by atc.COLUMN_ID";
}