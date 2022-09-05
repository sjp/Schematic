using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetIdentifierDefaults
{
    internal sealed record Result : IIdentifierDefaults
    {
        public string? Server { get; init; }

        public string? Database { get; init; }

        public string? Schema { get; init; }
    }

    internal const string Sql = @$"
select
    pg_catalog.host(pg_catalog.inet_server_addr()) as ""{nameof(Result.Server)}"",
    pg_catalog.current_database() as ""{nameof(Result.Database)}"",
    pg_catalog.current_schema() as ""{nameof(Result.Schema)}""";
}