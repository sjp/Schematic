using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetPackageDefinition
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string PackageName { get; init; }
    }

    internal sealed record Result
    {
        public required string RoutineType { get; init; }

        public required int LineNumber { get; init; }

        public required string? Text { get; init; }
    }

    internal const string Sql = $"""

select
    TYPE as "{nameof(Result.RoutineType)}",
    LINE as "{nameof(Result.LineNumber)}",
    TEXT as "{nameof(Result.Text)}"
from SYS.ALL_SOURCE
where OWNER = :{nameof(Query.SchemaName)} and NAME = :{nameof(Query.PackageName)} and TYPE in ('PACKAGE', 'PACKAGE BODY')
order by LINE
""";
}