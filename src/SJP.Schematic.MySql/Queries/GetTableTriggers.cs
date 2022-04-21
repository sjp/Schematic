namespace SJP.Schematic.MySql.Queries;

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
    }

    internal const string Sql = @$"
select
    tr.trigger_name as `{ nameof(Result.TriggerName) }`,
    tr.action_statement as `{ nameof(Result.Definition) }`,
    tr.action_timing as `{ nameof(Result.Timing) }`,
    tr.event_manipulation as `{ nameof(Result.TriggerEvent) }`
from information_schema.triggers tr
where tr.event_object_schema = @{ nameof(Query.SchemaName) } and tr.event_object_table = @{ nameof(Query.TableName) }";
}