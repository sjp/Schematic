using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetUserDefinedTypeSpecification
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TypeName { get; init; }
    }

    internal sealed record Result
    {
        public required string? Definition { get; init; }
    }

    internal const string Sql = $"""

select s.TEXT as "{nameof(Result.Definition)}"
from SYS.ALL_SOURCE s
where s.OWNER = :{nameof(Query.SchemaName)} and s.NAME = :{nameof(Query.TypeName)} and s.TYPE = 'TYPE'
order by s.LINE
""";
}
