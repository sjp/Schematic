namespace SJP.Schematic.Oracle.Queries;

internal static class GetTableTriggers
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string? TriggerSchema { get; init; }

        public string? TriggerName { get; init; }

        public string? TriggerType { get; init; }

        public string? TriggerEvent { get; init; }

        public string? Definition { get; init; }

        public string? EnabledStatus { get; init; }
    }

    internal const string Sql = @$"
select
    OWNER as ""{nameof(Result.TriggerSchema)}"",
    TRIGGER_NAME as ""{nameof(Result.TriggerName)}"",
    TRIGGER_TYPE as ""{nameof(Result.TriggerType)}"",
    TRIGGERING_EVENT as ""{nameof(Result.TriggerEvent)}"",
    TRIGGER_BODY as ""{nameof(Result.Definition)}"",
    STATUS as ""{nameof(Result.EnabledStatus)}""
from SYS.ALL_TRIGGERS
where TABLE_OWNER = :{nameof(Query.SchemaName)} and TABLE_NAME = :{nameof(Query.TableName)} and BASE_OBJECT_TYPE = 'TABLE'";
}