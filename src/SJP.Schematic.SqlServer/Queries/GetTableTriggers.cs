namespace SJP.Schematic.SqlServer.Queries;

internal static class GetTableTriggers
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string TriggerName { get; init; } = default!;

        public string Definition { get; init; } = default!;

        public bool IsInsteadOfTrigger { get; init; }

        public string TriggerEvent { get; init; } = default!;

        public bool IsDisabled { get; init; }
    }

    internal const string Sql = @$"
select
    st.name as [{nameof(Result.TriggerName)}],
    sm.definition as [{nameof(Result.Definition)}],
    st.is_instead_of_trigger as [{nameof(Result.IsInsteadOfTrigger)}],
    te.type_desc as [{nameof(Result.TriggerEvent)}],
    st.is_disabled as [{nameof(Result.IsDisabled)}]
from sys.tables t
inner join sys.triggers st on t.object_id = st.parent_id
inner join sys.sql_modules sm on st.object_id = sm.object_id
inner join sys.trigger_events te on st.object_id = te.object_id
where schema_name(t.schema_id) = @{nameof(Query.SchemaName)} and t.name = @{nameof(Query.TableName)} and t.is_ms_shipped = 0";
}