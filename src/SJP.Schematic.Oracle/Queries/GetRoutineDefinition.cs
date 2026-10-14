using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetRoutineDefinition
{
    internal sealed record Query : ISqlQuery<RoutineDefinitionRow>
    {
        public required string SchemaName { get; init; }

        public required string RoutineName { get; init; }
    }

    // The source lines and the arguments are read together; each branch fills its own columns and
    // leaves typed nulls in the other's, so the column types agree across the union. ALL_ARGUMENTS
    // also describes package subprograms and the fields of composite arguments, hence the
    // PACKAGE_NAME and DATA_LEVEL predicates. Ascending order sorts nulls last, so source rows come
    // first in line order, followed by the arguments in signature order.
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
from SYS.ALL_SOURCE s
where s.OWNER = :{nameof(Query.SchemaName)} and s.NAME = :{nameof(Query.RoutineName)}
    and s.TYPE in ('FUNCTION', 'PROCEDURE')

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
from SYS.ALL_ARGUMENTS a
where a.OWNER = :{nameof(Query.SchemaName)} and a.OBJECT_NAME = :{nameof(Query.RoutineName)}
    and a.PACKAGE_NAME is null and a.DATA_LEVEL = 0
order by "{nameof(RoutineDefinitionRow.LineNumber)}", "{nameof(RoutineDefinitionRow.Position)}"
""";
}
