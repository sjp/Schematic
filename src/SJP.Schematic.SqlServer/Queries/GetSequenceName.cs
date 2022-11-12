namespace SJP.Schematic.SqlServer.Queries;

internal static class GetSequenceName
{
    internal sealed record Query
    {
        public required string SchemaName { get; init; }

        public required string SequenceName { get; init; }
    }

    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string SequenceName { get; init; }
    }

    internal const string Sql = @$"
select top 1 schema_name(schema_id) as [{nameof(Result.SchemaName)}], name as [{nameof(Result.SequenceName)}]
from sys.sequences
where schema_id = schema_id(@{nameof(Query.SchemaName)}) and name = @{nameof(Query.SequenceName)} and is_ms_shipped = 0";
}