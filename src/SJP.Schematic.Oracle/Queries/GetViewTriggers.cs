using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetViewTriggers
{
    // The shape of a trigger is the same whether it is attached to a table or a view, so the rows are
    // described by TriggerRow and mapped by the same code.
    internal sealed record Query : ISqlQuery<TriggerRow>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    // As for GetTableDetails, the UPDATE OF column list is aggregated in a derived table so that
    // TRIGGER_BODY, a LONG column, stays out of any grouped or nested select list.
    internal const string Sql = $"""

select
    t.OWNER as "{nameof(TriggerRow.TriggerSchema)}",
    t.TRIGGER_NAME as "{nameof(TriggerRow.TriggerName)}",
    t.TRIGGER_TYPE as "{nameof(TriggerRow.TriggerType)}",
    t.TRIGGERING_EVENT as "{nameof(TriggerRow.TriggerEvent)}",
    t.WHEN_CLAUSE as "{nameof(TriggerRow.Condition)}",
    tc.UPDATE_COLUMNS as "{nameof(TriggerRow.UpdateColumns)}",
    t.TRIGGER_BODY as "{nameof(TriggerRow.Definition)}",
    t.STATUS as "{nameof(TriggerRow.EnabledStatus)}"
from SYS.ALL_TRIGGERS t
left join (
    select
        TRIGGER_OWNER,
        TRIGGER_NAME,
        listagg(COLUMN_NAME, ',') within group (order by COLUMN_NAME) as UPDATE_COLUMNS
    from SYS.ALL_TRIGGER_COLS
    where TABLE_OWNER = :{nameof(Query.SchemaName)} and TABLE_NAME = :{nameof(Query.ViewName)} and COLUMN_LIST = 'YES'
    group by TRIGGER_OWNER, TRIGGER_NAME
) tc on tc.TRIGGER_OWNER = t.OWNER and tc.TRIGGER_NAME = t.TRIGGER_NAME
where t.TABLE_OWNER = :{nameof(Query.SchemaName)} and t.TABLE_NAME = :{nameof(Query.ViewName)} and t.BASE_OBJECT_TYPE = 'VIEW'
""";
}
