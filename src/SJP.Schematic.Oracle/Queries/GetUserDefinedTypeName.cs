using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetUserDefinedTypeName
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TypeName { get; init; }
    }

    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string TypeName { get; init; }
    }

    internal const string Sql = $"""

select t.OWNER as "{nameof(Result.SchemaName)}", t.TYPE_NAME as "{nameof(Result.TypeName)}"
from SYS.ALL_TYPES t
inner join SYS.ALL_OBJECTS o on o.OWNER = t.OWNER and o.OBJECT_NAME = t.TYPE_NAME
where t.OWNER = :{nameof(Query.SchemaName)} and t.TYPE_NAME = :{nameof(Query.TypeName)}
    and o.OBJECT_TYPE = 'TYPE' and o.ORACLE_MAINTAINED <> 'Y'
""";
}
