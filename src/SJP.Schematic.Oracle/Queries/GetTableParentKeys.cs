namespace SJP.Schematic.Oracle.Queries;

internal static class GetTableParentKeys
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string ConstraintName { get; init; } = default!;

        public string EnabledStatus { get; init; } = default!;

        public string DeleteAction { get; init; } = default!;

        public string? ParentTableSchema { get; init; }

        public string? ParentTableName { get; init; }

        public string? ParentConstraintName { get; init; }

        public string ParentKeyType { get; init; } = default!;

        public string ColumnName { get; init; } = default!;

        public int ColumnPosition { get; init; }
    }

    internal const string Sql = @$"
select
    ac.CONSTRAINT_NAME as ""{ nameof(Result.ConstraintName) }"",
    ac.STATUS as ""{ nameof(Result.EnabledStatus) }"",
    ac.DELETE_RULE as ""{ nameof(Result.DeleteAction) }"",
    pac.OWNER as ""{ nameof(Result.ParentTableSchema) }"",
    pac.TABLE_NAME as ""{ nameof(Result.ParentTableName) }"",
    pac.CONSTRAINT_NAME as ""{ nameof(Result.ParentConstraintName) }"",
    pac.CONSTRAINT_TYPE as ""{ nameof(Result.ParentKeyType) }"",
    acc.COLUMN_NAME as ""{ nameof(Result.ColumnName) }"",
    acc.POSITION as ""{ nameof(Result.ColumnPosition) }""
from SYS.ALL_CONSTRAINTS ac
inner join SYS.ALL_CONS_COLUMNS acc on ac.OWNER = acc.OWNER and ac.CONSTRAINT_NAME = acc.CONSTRAINT_NAME and ac.TABLE_NAME = acc.TABLE_NAME
inner join SYS.ALL_CONSTRAINTS pac on pac.OWNER = ac.R_OWNER and pac.CONSTRAINT_NAME = ac.R_CONSTRAINT_NAME
where ac.OWNER = :{ nameof(Query.SchemaName) } and ac.TABLE_NAME = :{ nameof(Query.TableName) } and ac.CONSTRAINT_TYPE = 'R' and pac.CONSTRAINT_TYPE in ('P', 'U')";
}