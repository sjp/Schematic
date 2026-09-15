using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetViewTriggers
{
    // The shape of a trigger is the same whether it is attached to a table or a view, so the rows are
    // described by GetTableTriggers.Result and mapped by the same code.
    internal sealed record Query : ISqlQuery<GetTableTriggers.Result>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    // parent_class = 1 restricts the join to triggers whose parent is an object, i.e. DML triggers,
    // as opposed to the database-scoped DDL triggers that also live in sys.triggers.
    internal const string Sql = @$"
select
    st.name as [{nameof(GetTableTriggers.Result.TriggerName)}],
    sm.definition as [{nameof(GetTableTriggers.Result.Definition)}],
    st.is_instead_of_trigger as [{nameof(GetTableTriggers.Result.IsInsteadOfTrigger)}],
    st.is_disabled as [{nameof(GetTableTriggers.Result.IsDisabled)}],
    ev.IsInsertTrigger as [{nameof(GetTableTriggers.Result.IsInsertTrigger)}],
    ev.IsUpdateTrigger as [{nameof(GetTableTriggers.Result.IsUpdateTrigger)}],
    ev.IsDeleteTrigger as [{nameof(GetTableTriggers.Result.IsDeleteTrigger)}],
    ev.IsOtherTrigger as [{nameof(GetTableTriggers.Result.IsOtherTrigger)}]
from sys.views v
inner join sys.triggers st on v.object_id = st.parent_id and st.parent_class = 1
inner join sys.sql_modules sm on st.object_id = sm.object_id
cross apply (
    select
        cast(max(case when te.type_desc = 'INSERT' then 1 else 0 end) as bit) as IsInsertTrigger,
        cast(max(case when te.type_desc = 'UPDATE' then 1 else 0 end) as bit) as IsUpdateTrigger,
        cast(max(case when te.type_desc = 'DELETE' then 1 else 0 end) as bit) as IsDeleteTrigger,
        cast(max(case when te.type_desc not in ('INSERT', 'UPDATE', 'DELETE') then 1 else 0 end) as bit) as IsOtherTrigger
    from sys.trigger_events te
    where te.object_id = st.object_id
    group by te.object_id
) ev
where v.schema_id = schema_id(@{nameof(Query.SchemaName)}) and v.name = @{nameof(Query.ViewName)} and v.is_ms_shipped = 0";
}
