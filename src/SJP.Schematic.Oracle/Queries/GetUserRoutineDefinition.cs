using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetUserRoutineDefinition
{
    internal sealed record Query : ISqlQuery<RoutineDefinitionRow>
    {
        public required string RoutineName { get; init; }
    }

    // the same shape as GetRoutineDefinition, over the USER_* views for the connection's own schema
    internal const string Sql = $"""

select
    s.TYPE as "{nameof(RoutineDefinitionRow.RoutineType)}",
    s.LINE as "{nameof(RoutineDefinitionRow.LineNumber)}",
    s.TEXT as "{nameof(RoutineDefinitionRow.Text)}",
    cast(null as number) as "{nameof(RoutineDefinitionRow.Position)}",
    cast(null as varchar2(128)) as "{nameof(RoutineDefinitionRow.ArgumentName)}",
    cast(null as varchar2(9)) as "{nameof(RoutineDefinitionRow.InOut)}",
    cast(null as varchar2(128)) as "{nameof(RoutineDefinitionRow.ArgumentTypeSchema)}",
    cast(null as varchar2(128)) as "{nameof(RoutineDefinitionRow.ArgumentTypeName)}",
    0 as "{nameof(RoutineDefinitionRow.DataLength)}",
    cast(null as number) as "{nameof(RoutineDefinitionRow.Precision)}",
    cast(null as number) as "{nameof(RoutineDefinitionRow.Scale)}",
    cast(null as varchar2(1)) as "{nameof(RoutineDefinitionRow.Defaulted)}"
from SYS.USER_SOURCE s
where s.NAME = :{nameof(Query.RoutineName)} and s.TYPE in ('FUNCTION', 'PROCEDURE')

union all

select
    cast(null as varchar2(12)),
    cast(null as number),
    cast(null as varchar2(4000)),
    a.POSITION,
    a.ARGUMENT_NAME,
    a.IN_OUT,
    a.TYPE_OWNER,
    a.DATA_TYPE,
    nvl(a.DATA_LENGTH, 0),
    a.DATA_PRECISION,
    a.DATA_SCALE,
    a.DEFAULTED
from SYS.USER_ARGUMENTS a
where a.OBJECT_NAME = :{nameof(Query.RoutineName)} and a.PACKAGE_NAME is null and a.DATA_LEVEL = 0
order by "{nameof(RoutineDefinitionRow.LineNumber)}", "{nameof(RoutineDefinitionRow.Position)}"
""";
}
