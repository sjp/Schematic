using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

/// <summary>
/// Retrieves the primary key, unique key and foreign key constraints defined on a table in a single
/// round trip. Replaces what were previously three separate queries against <c>SYS.ALL_CONSTRAINTS</c>
/// (<c>GetTablePrimaryKey</c>, <c>GetTableUniqueKeys</c>, <c>GetTableParentKeys</c>) differing only by
/// <c>CONSTRAINT_TYPE</c>. Consumers partition the result rows by <see cref="Result.ConstraintType"/>.
/// </summary>
internal static class GetTableConstraints
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string? ConstraintName { get; init; }

        /// <summary>
        /// The kind of constraint. <c>P</c> for primary key, <c>U</c> for unique key, <c>R</c> for
        /// foreign key.
        /// </summary>
        public required string ConstraintType { get; init; }

        public required string? EnabledStatus { get; init; }

        /// <summary>
        /// The index enforcing the constraint. Only populated for <c>P</c> and <c>U</c> rows.
        /// </summary>
        public required string? IndexName { get; init; }

        /// <summary>
        /// The delete rule for a foreign key. Only populated for <c>R</c> rows.
        /// </summary>
        public required string? DeleteAction { get; init; }

        public required string? ColumnName { get; init; }

        public required int ColumnPosition { get; init; }

        /// <summary>
        /// The parent constraint's owning schema. Only populated for <c>R</c> rows whose referenced
        /// constraint is itself a primary or unique key.
        /// </summary>
        public required string? ParentTableSchema { get; init; }

        public required string? ParentTableName { get; init; }

        public required string? ParentConstraintName { get; init; }

        public required string? ParentKeyType { get; init; }
    }

    internal const string Sql = $"""

select
    ac.CONSTRAINT_NAME as "{nameof(Result.ConstraintName)}",
    ac.CONSTRAINT_TYPE as "{nameof(Result.ConstraintType)}",
    ac.STATUS as "{nameof(Result.EnabledStatus)}",
    ac.INDEX_NAME as "{nameof(Result.IndexName)}",
    ac.DELETE_RULE as "{nameof(Result.DeleteAction)}",
    acc.COLUMN_NAME as "{nameof(Result.ColumnName)}",
    acc.POSITION as "{nameof(Result.ColumnPosition)}",
    pac.OWNER as "{nameof(Result.ParentTableSchema)}",
    pac.TABLE_NAME as "{nameof(Result.ParentTableName)}",
    pac.CONSTRAINT_NAME as "{nameof(Result.ParentConstraintName)}",
    pac.CONSTRAINT_TYPE as "{nameof(Result.ParentKeyType)}"
from SYS.ALL_CONSTRAINTS ac
inner join SYS.ALL_CONS_COLUMNS acc on ac.OWNER = acc.OWNER and ac.CONSTRAINT_NAME = acc.CONSTRAINT_NAME and ac.TABLE_NAME = acc.TABLE_NAME
left join SYS.ALL_CONSTRAINTS pac on pac.OWNER = ac.R_OWNER and pac.CONSTRAINT_NAME = ac.R_CONSTRAINT_NAME and pac.CONSTRAINT_TYPE in ('P', 'U')
where ac.OWNER = :{nameof(Query.SchemaName)} and ac.TABLE_NAME = :{nameof(Query.TableName)} and ac.CONSTRAINT_TYPE in ('P', 'U', 'R')
""";
}
