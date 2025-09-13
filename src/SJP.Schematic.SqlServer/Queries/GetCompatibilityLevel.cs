namespace SJP.Schematic.SqlServer.Queries;

internal static class GetCompatibilityLevel
{
    internal sealed record Result
    {
        public required int CompatibilityLevel { get; init; }
    }

    internal const string Sql = @$"
select compatibility_level as [{nameof(Result.CompatibilityLevel)}]
from sys.databases where name = db_name()";
}