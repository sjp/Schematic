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

        public required string? Definition { get; init; }

        public required string? EnabledStatus { get; init; }
    }

    internal const string Sql = $"""

select
    OWNER as "{nameof(Result.TriggerSchema)}",
    TRIGGER_NAME as "{nameof(Result.TriggerName)}",
    TRIGGER_TYPE as "{nameof(Result.TriggerType)}",
    TRIGGERING_EVENT as "{nameof(Result.TriggerEvent)}",
    TRIGGER_BODY as "{nameof(Result.Definition)}",
    STATUS as "{nameof(Result.EnabledStatus)}"
from SYS.ALL_TRIGGERS
where TABLE_OWNER = :{nameof(Query.SchemaName)} and TABLE_NAME = :{nameof(Query.TableName)} and BASE_OBJECT_TYPE = 'TABLE'
""";
}