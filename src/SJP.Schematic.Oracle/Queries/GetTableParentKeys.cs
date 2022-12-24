using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetTableParentKeys
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string ConstraintName { get; init; }

        public required string EnabledStatus { get; init; }

        public required string DeleteAction { get; init; }

        public required string? ParentTableSchema { get; init; }

        public required string? ParentTableName { get; init; }

        public required string? ParentConstraintName { get; init; }

        public required string ParentKeyType { get; init; }

        public required string ColumnName { get; init; }

        public required int ColumnPosition { get; init; }
    }

    internal const string Sql = @$"
select
    ac.CONSTRAINT_NAME as ""{nameof(Result.ConstraintName)}"",
    ac.STATUS as ""{nameof(Result.EnabledStatus)}"",
    ac.DELETE_RULE as ""{nameof(Result.DeleteAction)}"",
    pac.OWNER as ""{nameof(Result.ParentTableSchema)}"",
    pac.TABLE_NAME as ""{nameof(Result.ParentTableName)}"",
    pac.CONSTRAINT_NAME as ""{nameof(Result.ParentConstraintName)}"",
    pac.CONSTRAINT_TYPE as ""{nameof(Result.ParentKeyType)}"",
    acc.COLUMN_NAME as ""{nameof(Result.ColumnName)}"",
    acc.POSITION as ""{nameof(Result.ColumnPosition)}""
from SYS.ALL_CONSTRAINTS ac
inner join SYS.ALL_CONS_COLUMNS acc on ac.OWNER = acc.OWNER and ac.CONSTRAINT_NAME = acc.CONSTRAINT_NAME and ac.TABLE_NAME = acc.TABLE_NAME
inner join SYS.ALL_CONSTRAINTS pac on pac.OWNER = ac.R_OWNER and pac.CONSTRAINT_NAME = ac.R_CONSTRAINT_NAME
where ac.OWNER = :{nameof(Query.SchemaName)} and ac.TABLE_NAME = :{nameof(Query.TableName)} and ac.CONSTRAINT_TYPE = 'R' and pac.CONSTRAINT_TYPE in ('P', 'U')";
}