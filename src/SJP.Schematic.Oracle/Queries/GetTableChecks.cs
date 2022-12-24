using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetTableChecks
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
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
where OWNER = :{nameof(Query.SchemaName)} and TABLE_NAME = :{nameof(Query.TableName)} and CONSTRAINT_TYPE = 'C'";
}