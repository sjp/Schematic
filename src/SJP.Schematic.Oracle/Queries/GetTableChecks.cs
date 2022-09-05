namespace SJP.Schematic.Oracle.Queries;

internal static class GetTableChecks
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string ConstraintName { get; init; } = default!;

        public string? Definition { get; init; }

        public string EnabledStatus { get; init; } = default!;
    }

    internal const string Sql = @$"
select
    CONSTRAINT_NAME as ""{nameof(Result.ConstraintName)}"",
    SEARCH_CONDITION as ""{nameof(Result.Definition)}"",
    STATUS as ""{nameof(Result.EnabledStatus)}""
from SYS.ALL_CONSTRAINTS
where OWNER = :{nameof(Query.SchemaName)} and TABLE_NAME = :{nameof(Query.TableName)} and CONSTRAINT_TYPE = 'C'";
}