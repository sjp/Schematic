using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetAllSchemaComments
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string CommentProperty { get; init; }
    }

    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string? Comment { get; init; }
    }

    internal const string Sql = @$"
select
    s.name as [{nameof(Result.SchemaName)}],
    ep.value as [{nameof(Result.Comment)}]
from sys.schemas s
left join sys.extended_properties ep on s.schema_id = ep.major_id and ep.name = @{nameof(Query.CommentProperty)} and ep.minor_id = 0 and ep.class = 3
order by s.name";
}
