using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetViewColumns
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

        public required int CharLength { get; init; }

        /// <summary>
        /// <c>C</c> when the length was declared in characters, <c>B</c> when declared in bytes,
        /// else <see langword="null" />.
        /// </summary>
        public required string? CharUsed { get; init; }

        public required int? Precision { get; init; }

        public required int? Scale { get; init; }

        public required string? Collation { get; init; }

        public required string? IsNullable { get; init; }

        /// <summary>
        /// <c>YES</c> when the column is omitted from <c>SELECT *</c>, else <c>NO</c>. Only a column
        /// declared <c>INVISIBLE</c> reaches this query; system-generated hidden columns, such as those
        /// behind function-based indexes, are filtered out.
        /// </summary>
        public required string? IsHidden { get; init; }
    }

    internal const string Sql = $"""

select
    atc.COLUMN_NAME as "{nameof(Result.ColumnName)}",
    atc.DATA_TYPE_OWNER as "{nameof(Result.ColumnTypeSchema)}",
    atc.DATA_TYPE as "{nameof(Result.ColumnTypeName)}",
    atc.DATA_LENGTH as "{nameof(Result.DataLength)}",
    atc.CHAR_LENGTH as "{nameof(Result.CharLength)}",
    atc.CHAR_USED as "{nameof(Result.CharUsed)}",
    atc.DATA_PRECISION as "{nameof(Result.Precision)}",
    atc.DATA_SCALE as "{nameof(Result.Scale)}",
    atc.CHARACTER_SET_NAME as "{nameof(Result.Collation)}",
    atc.NULLABLE as "{nameof(Result.IsNullable)}",
    atc.HIDDEN_COLUMN as "{nameof(Result.IsHidden)}"
from SYS.ALL_TAB_COLS atc
where atc.OWNER = :{nameof(Query.SchemaName)} and atc.TABLE_NAME = :{nameof(Query.ViewName)}
    and (atc.HIDDEN_COLUMN = 'NO' or atc.USER_GENERATED = 'YES')
order by atc.COLUMN_ID, atc.INTERNAL_COLUMN_ID
""";
}