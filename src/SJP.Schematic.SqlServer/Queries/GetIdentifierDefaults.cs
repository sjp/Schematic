using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Queries;

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
    @@SERVERNAME as [{nameof(Result.Server)}],
    db_name() as [{nameof(Result.Database)}],
    schema_name() as [{nameof(Result.Schema)}]";
}