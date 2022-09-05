namespace SJP.Schematic.Oracle.Queries;

internal static class GetUserPackageDefinition
{
    internal sealed record Query
    {
        public string PackageName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string RoutineType { get; init; } = default!;

        public int LineNumber { get; init; }

        public string? Text { get; init; }
    }

    internal const string Sql = @$"
select
    TYPE as ""{nameof(Result.RoutineType)}"",
    LINE as ""{nameof(Result.LineNumber)}"",
    TEXT as ""{nameof(Result.Text)}""
from SYS.USER_SOURCE
where NAME = :{nameof(Query.PackageName)} and TYPE in ('PACKAGE', 'PACKAGE BODY')
order by LINE";
}