using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetTablePrimaryKey
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string? ConstraintName { get; init; }

        public required string? EnabledStatus { get; init; }

        public required string? ColumnName { get; init; }

        public required int ColumnPosition { get; init; }
    }

    internal const string Sql = @$"
select
    ac.CONSTRAINT_NAME as ""{nameof(Result.ConstraintName)}"",
    ac.STATUS as ""{nameof(Result.EnabledStatus)}"",
    acc.COLUMN_NAME as ""{nameof(Result.ColumnName)}"",
    acc.POSITION as ""{nameof(Result.ColumnPosition)}""
from SYS.ALL_CONSTRAINTS ac
inner join SYS.ALL_CONS_COLUMNS acc on ac.OWNER = acc.OWNER and ac.CONSTRAINT_NAME = acc.CONSTRAINT_NAME and ac.TABLE_NAME = acc.TABLE_NAME
where ac.OWNER = :{nameof(Query.SchemaName)} and ac.TABLE_NAME = :{nameof(Query.TableName)} and ac.CONSTRAINT_TYPE = 'P'";
}