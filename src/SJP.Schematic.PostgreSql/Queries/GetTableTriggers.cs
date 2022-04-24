namespace SJP.Schematic.PostgreSql.Queries;

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

        public string Timing { get; init; } = default!;

        public string TriggerEvent { get; init; } = default!;

        public string EnabledFlag { get; init; } = default!;
    }

    internal const string Sql = @$"
select
    tr.tgname as ""{ nameof(Result.TriggerName) }"",
    tgenabled as ""{ nameof(Result.EnabledFlag) }"",
    itr.action_statement as ""{ nameof(Result.Definition) }"",
    itr.action_timing as ""{ nameof(Result.Timing) }"",
    itr.event_manipulation as ""{ nameof(Result.TriggerEvent) }""
from pg_catalog.pg_class t
inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
inner join pg_catalog.pg_trigger tr on t.oid = tr.tgrelid
inner join information_schema.triggers itr on ns.nspname = itr.event_object_schema and itr.event_object_table = t.relname and itr.trigger_name = tr.tgname
where t.relkind = 'r'
    and t.relname = @{ nameof(Query.TableName) }
    and ns.nspname = @{ nameof(Query.SchemaName) }";
}