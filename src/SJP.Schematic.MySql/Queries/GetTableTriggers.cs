namespace SJP.Schematic.MySql.Queries;

internal static class GetTableTriggers
{
    internal sealed record Query
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string TriggerName { get; init; }

        public required string Definition { get; init; }

        public required string Timing { get; init; }

        public required string TriggerEvent { get; init; }
    }

    internal const string Sql = @$"
select
    tr.trigger_name as `{nameof(Result.TriggerName)}`,
    tr.action_statement as `{nameof(Result.Definition)}`,
    tr.action_timing as `{nameof(Result.Timing)}`,
    tr.event_manipulation as `{nameof(Result.TriggerEvent)}`
from information_schema.triggers tr
where tr.event_object_schema = @{nameof(Query.SchemaName)} and tr.event_object_table = @{nameof(Query.TableName)}";
}