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

        public required int Precision { get; init; }

        public required int Scale { get; init; }

        public required string? Collation { get; init; }

        public required string? IsComputed { get; init; }

        public required string? DefaultValue { get; init; }

        /// <summary>
        /// <c>YES</c> when the column is a 12c identity column, else <c>NO</c>.
        /// </summary>
        public required string? IsIdentity { get; init; }

        /// <summary>
        /// <c>YES</c> when an explicit <c>NULL</c> is replaced by a generated value, else <c>NO</c>.
        /// </summary>
        public required string? DefaultOnNull { get; init; }

        /// <summary>
        /// <c>ALWAYS</c> or <c>BY DEFAULT</c> for an identity column, else <see langword="null" />.
        /// </summary>
        public required string? GenerationType { get; init; }

        /// <summary>
        /// The name of the sequence backing an identity column, else <see langword="null" />.
        /// </summary>
        public required string? SequenceName { get; init; }

        /// <summary>
        /// The parameters of the sequence backing an identity column, as a comma-separated list of
        /// <c>NAME: VALUE</c> pairs, else <see langword="null" />.
        /// </summary>
        public required string? IdentityOptions { get; init; }
    }

    internal const string Sql = $"""

select
    c.COLUMN_NAME as "{nameof(Result.ColumnName)}",
    c.DATA_TYPE_OWNER as "{nameof(Result.ColumnTypeSchema)}",
    c.DATA_TYPE as "{nameof(Result.ColumnTypeName)}",
    c.DATA_LENGTH as "{nameof(Result.DataLength)}",
    c.DATA_PRECISION as "{nameof(Result.Precision)}",
    c.DATA_SCALE as "{nameof(Result.Scale)}",
    c.DATA_DEFAULT as "{nameof(Result.DefaultValue)}",
    c.CHARACTER_SET_NAME as "{nameof(Result.Collation)}",
    c.VIRTUAL_COLUMN as "{nameof(Result.IsComputed)}",
    c.IDENTITY_COLUMN as "{nameof(Result.IsIdentity)}",
    c.DEFAULT_ON_NULL as "{nameof(Result.DefaultOnNull)}",
    ic.GENERATION_TYPE as "{nameof(Result.GenerationType)}",
    ic.SEQUENCE_NAME as "{nameof(Result.SequenceName)}",
    ic.IDENTITY_OPTIONS as "{nameof(Result.IdentityOptions)}"
from SYS.ALL_TAB_COLS c
left join SYS.ALL_TAB_IDENTITY_COLS ic
    on ic.OWNER = c.OWNER and ic.TABLE_NAME = c.TABLE_NAME and ic.COLUMN_NAME = c.COLUMN_NAME
where c.OWNER = :{nameof(Query.SchemaName)} and c.TABLE_NAME = :{nameof(Query.TableName)} and c.HIDDEN_COLUMN = 'NO'
order by c.COLUMN_ID
""";
}