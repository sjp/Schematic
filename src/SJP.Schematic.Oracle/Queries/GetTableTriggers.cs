using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetTableTriggers
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string? TriggerSchema { get; init; }

        public required string? TriggerName { get; init; }

        public required string? TriggerType { get; init; }

        public required string? TriggerEvent { get; init; }

        public required string? Condition { get; init; }

        public required string? UpdateColumns { get; init; }

        public required string? Definition { get; init; }

        public required string? EnabledStatus { get; init; }
    }

    // ALL_TRIGGERS does not record the UPDATE OF column list; ALL_TRIGGER_COLS does, flagged by
    // COLUMN_LIST = 'YES'. It is aggregated in a derived table so that TRIGGER_BODY, a LONG column,
    // stays out of any grouped or nested select list.
    internal const string Sql = $"""

select
    t.OWNER as "{nameof(Result.TriggerSchema)}",
    t.TRIGGER_NAME as "{nameof(Result.TriggerName)}",
    t.TRIGGER_TYPE as "{nameof(Result.TriggerType)}",
    t.TRIGGERING_EVENT as "{nameof(Result.TriggerEvent)}",
    t.WHEN_CLAUSE as "{nameof(Result.Condition)}",
    tc.UPDATE_COLUMNS as "{nameof(Result.UpdateColumns)}",
    t.TRIGGER_BODY as "{nameof(Result.Definition)}",
    t.STATUS as "{nameof(Result.EnabledStatus)}"
from SYS.ALL_TRIGGERS t
left join (
    select
        TRIGGER_OWNER,
        TRIGGER_NAME,
        listagg(COLUMN_NAME, ',') within group (order by COLUMN_NAME) as UPDATE_COLUMNS
    from SYS.ALL_TRIGGER_COLS
    where TABLE_OWNER = :{nameof(Query.SchemaName)} and TABLE_NAME = :{nameof(Query.TableName)} and COLUMN_LIST = 'YES'
    group by TRIGGER_OWNER, TRIGGER_NAME
) tc on tc.TRIGGER_OWNER = t.OWNER and tc.TRIGGER_NAME = t.TRIGGER_NAME
where t.TABLE_OWNER = :{nameof(Query.SchemaName)} and t.TABLE_NAME = :{nameof(Query.TableName)} and t.BASE_OBJECT_TYPE = 'TABLE'
""";
}
