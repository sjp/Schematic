using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Queries;

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
    @@hostname as `{ nameof(Result.Server) }`,
    database() as `{ nameof(Result.Database) }`,
    schema() as `{ nameof(Result.Schema) }`";
}