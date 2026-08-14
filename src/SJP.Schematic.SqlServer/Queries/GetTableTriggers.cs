using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetTableTriggers
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string TriggerName { get; init; }

        public required string Definition { get; init; }

        public required bool IsInsteadOfTrigger { get; init; }

        public required bool IsDisabled { get; init; }

        public required bool IsInsertTrigger { get; init; }

        public required bool IsUpdateTrigger { get; init; }

        public required bool IsDeleteTrigger { get; init; }

        // Set when a trigger fires on an event other than INSERT/UPDATE/DELETE, i.e. one Schematic doesn't
        // currently model, so the caller can raise the same error as before instead of silently dropping it.
        public required string? UnsupportedTriggerEvent { get; init; }
    }

    // sys.trigger_events emits one row per (trigger, event), and a trigger fired on multiple events would
    // otherwise resend its (potentially large) definition once per event. Pivoting the events collapses
    // that back down to one row per trigger.
    internal const string Sql = @$"
select
    st.name as [{nameof(Result.TriggerName)}],
    sm.definition as [{nameof(Result.Definition)}],
    st.is_instead_of_trigger as [{nameof(Result.IsInsteadOfTrigger)}],
    st.is_disabled as [{nameof(Result.IsDisabled)}],
    cast(max(case when te.type_desc = 'INSERT' then 1 else 0 end) as bit) as [{nameof(Result.IsInsertTrigger)}],
    cast(max(case when te.type_desc = 'UPDATE' then 1 else 0 end) as bit) as [{nameof(Result.IsUpdateTrigger)}],
    cast(max(case when te.type_desc = 'DELETE' then 1 else 0 end) as bit) as [{nameof(Result.IsDeleteTrigger)}],
    max(case when te.type_desc not in ('INSERT', 'UPDATE', 'DELETE') then te.type_desc end) as [{nameof(Result.UnsupportedTriggerEvent)}]
from sys.tables t
inner join sys.triggers st on t.object_id = st.parent_id
inner join sys.sql_modules sm on st.object_id = sm.object_id
inner join sys.trigger_events te on st.object_id = te.object_id
where t.schema_id = schema_id(@{nameof(Query.SchemaName)}) and t.name = @{nameof(Query.TableName)} and t.is_ms_shipped = 0
group by st.name, sm.definition, st.is_instead_of_trigger, st.is_disabled";
}
