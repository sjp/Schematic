namespace SJP.Schematic.Oracle.Queries;

internal static class GetIdentifierDefaults
{
    internal sealed record Result
    {
        public required string? ServerHost { get; init; }

        public required string? ServerSid { get; init; }

        public required string? DatabaseName { get; init; }

        public required string? DefaultSchema { get; init; }
    }

    internal const string Sql = @$"
select
    SYS_CONTEXT('USERENV', 'SERVER_HOST') as ""{nameof(Result.ServerHost)}"",
    SYS_CONTEXT('USERENV', 'INSTANCE_NAME') as ""{nameof(Result.ServerSid)}"",
    SYS_CONTEXT('USERENV', 'DB_NAME') as ""{nameof(Result.DatabaseName)}"",
    SYS_CONTEXT('USERENV', 'CURRENT_USER') as ""{nameof(Result.DefaultSchema)}""
from DUAL";
}