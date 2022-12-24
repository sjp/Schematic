using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetViewName
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal const string Sql = @$"
select v.OWNER as ""{nameof(Result.SchemaName)}"", v.VIEW_NAME as ""{nameof(Result.ViewName)}""
from SYS.ALL_VIEWS v
inner join SYS.ALL_OBJECTS o on v.OWNER = o.OWNER and v.VIEW_NAME = o.OBJECT_NAME
where v.OWNER = :{nameof(Query.SchemaName)} and v.VIEW_NAME = :{nameof(Query.ViewName)} and o.ORACLE_MAINTAINED <> 'Y'";
}