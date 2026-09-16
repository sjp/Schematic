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
    s.name as [{nameof(Result.SchemaName)}],
    seq.name as [{nameof(Result.SequenceName)}]
from sys.sequences seq
inner join sys.schemas s on seq.schema_id = s.schema_id
where seq.is_ms_shipped = 0
order by s.name, seq.name";
}