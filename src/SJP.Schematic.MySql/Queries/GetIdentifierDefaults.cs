using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Queries;

internal static class GetIdentifierDefaults
{
    internal sealed record Result : IIdentifierDefaults
    {
        public required string Server { get; init; }

        public required string Database { get; init; }

        public required string Schema { get; init; }
    }

    internal const string Sql = @$"
select
    @@hostname as `{nameof(Result.Server)}`,
    database() as `{nameof(Result.Database)}`,
    schema() as `{nameof(Result.Schema)}`";
}