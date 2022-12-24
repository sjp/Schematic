using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetTableChildKeys
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string? ChildTableSchema { get; init; }

        public required string? ChildTableName { get; init; }

        public required string? ChildKeyName { get; init; }

        public required string? EnabledStatus { get; init; }

        public required string? DeleteAction { get; init; }

        public required string? ParentKeyName { get; init; }

        public required string? ParentKeyType { get; init; }
    }

    internal const string Sql = @$"
select
    ac.OWNER as ""{nameof(Result.ChildTableSchema)}"",
    ac.TABLE_NAME as ""{nameof(Result.ChildTableName)}"",
    ac.CONSTRAINT_NAME as ""{nameof(Result.ChildKeyName)}"",
    ac.STATUS as ""{nameof(Result.EnabledStatus)}"",
    ac.DELETE_RULE as ""{nameof(Result.DeleteAction)}"",
    pac.CONSTRAINT_NAME as ""{nameof(Result.ParentKeyName)}"",
    pac.CONSTRAINT_TYPE as ""{nameof(Result.ParentKeyType)}""
from SYS.ALL_CONSTRAINTS ac
inner join SYS.ALL_CONSTRAINTS pac on pac.OWNER = ac.R_OWNER and pac.CONSTRAINT_NAME = ac.R_CONSTRAINT_NAME
where pac.OWNER = :{nameof(Query.SchemaName)} and pac.TABLE_NAME = :{nameof(Query.TableName)} and ac.CONSTRAINT_TYPE = 'R' and pac.CONSTRAINT_TYPE in ('P', 'U')";
}