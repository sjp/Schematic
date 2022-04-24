namespace SJP.Schematic.Oracle.Queries;

internal static class GetTableUniqueKeys
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string? ConstraintName { get; init; }

        public string? EnabledStatus { get; init; }

        public string? ColumnName { get; init; }

        public int ColumnPosition { get; init; }
    }

    internal const string Sql = @$"
select
    ac.CONSTRAINT_NAME as ""{ nameof(Result.ConstraintName) }"",
    ac.STATUS as ""{ nameof(Result.EnabledStatus) }"",
    acc.COLUMN_NAME as ""{ nameof(Result.ColumnName) }"",
    acc.POSITION as ""{ nameof(Result.ColumnPosition) }""
from SYS.ALL_CONSTRAINTS ac
inner join SYS.ALL_CONS_COLUMNS acc on ac.OWNER = acc.OWNER and ac.CONSTRAINT_NAME = acc.CONSTRAINT_NAME and ac.TABLE_NAME = acc.TABLE_NAME
where ac.OWNER = :{ nameof(Query.SchemaName) } and ac.TABLE_NAME = :{ nameof(Query.TableName) } and ac.CONSTRAINT_TYPE = 'U'";
}