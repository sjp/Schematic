namespace SJP.Schematic.SqlServer.Queries;

internal static class GetAllSequenceNames
{
    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string SequenceName { get; init; }
    }

    internal const string Sql = @$"
select
    schema_name(schema_id) as [{nameof(Result.SchemaName)}],
    name as [{nameof(Result.SequenceName)}]
from sys.sequences
where is_ms_shipped = 0
order by schema_name(schema_id), name";
}