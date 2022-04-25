using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetIdentifierDefaults
{
    internal sealed record Result : IIdentifierDefaults
    {
        public string Server { get; init; } = default!;

        public string Database { get; init; } = default!;

        public string Schema { get; init; } = default!;
    }

    internal const string Sql = @$"
select
    @@SERVERNAME as [{ nameof(Result.Server) }],
    db_name() as [{ nameof(Result.Database) }],
    schema_name() as [{ nameof(Result.Schema) }]";
}