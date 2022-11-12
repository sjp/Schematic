namespace SJP.Schematic.Oracle.Queries;

internal static class GetViewChecks
{
    internal sealed record Query
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal sealed record Result
    {
        public required string ConstraintName { get; init; }

        public required string? Definition { get; init; }

        public required string EnabledStatus { get; init; }
    }

    internal const string Sql = @$"
select
    CONSTRAINT_NAME as ""{nameof(Result.ConstraintName)}"",
    SEARCH_CONDITION as ""{nameof(Result.Definition)}"",
    STATUS as ""{nameof(Result.EnabledStatus)}""
from SYS.ALL_CONSTRAINTS
where OWNER = :{nameof(Query.SchemaName)} and TABLE_NAME = :{nameof(Query.ViewName)} and CONSTRAINT_TYPE = 'C'";
}