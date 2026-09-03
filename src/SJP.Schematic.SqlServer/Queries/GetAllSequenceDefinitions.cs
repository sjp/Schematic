namespace SJP.Schematic.SqlServer.Queries;

internal static class GetAllSequenceDefinitions
{
    internal sealed record Result : ISequenceDefinitionRow
    {
        public required string SchemaName { get; init; }

        public required string SequenceName { get; init; }

        public required string TypeSchemaName { get; init; }

        public required string TypeName { get; init; }

        public required int TypeMaxLength { get; init; }

        public required int Precision { get; init; }

        public required int Scale { get; init; }

        public required bool IsCached { get; init; }

        public required int? CacheSize { get; init; }

        public required bool Cycle { get; init; }

        public required decimal Increment { get; init; }

        public required decimal MinValue { get; init; }

        public required decimal MaxValue { get; init; }

        public required decimal StartValue { get; init; }
    }

    internal const string Sql = @$"
select
    schema_name(s.schema_id) as [{nameof(Result.SchemaName)}],
    s.name as [{nameof(Result.SequenceName)}],
    schema_name(t.schema_id) as [{nameof(Result.TypeSchemaName)}],
    t.name as [{nameof(Result.TypeName)}],
    cast(t.max_length as int) as [{nameof(Result.TypeMaxLength)}],
    cast(s.precision as int) as [{nameof(Result.Precision)}],
    cast(s.scale as int) as [{nameof(Result.Scale)}],
    s.start_value as [{nameof(Result.StartValue)}],
    s.increment as [{nameof(Result.Increment)}],
    s.minimum_value as [{nameof(Result.MinValue)}],
    s.maximum_value as [{nameof(Result.MaxValue)}],
    s.is_cycling as [{nameof(Result.Cycle)}],
    s.is_cached as [{nameof(Result.IsCached)}],
    s.cache_size as [{nameof(Result.CacheSize)}]
from sys.sequences s
inner join sys.types t on s.user_type_id = t.user_type_id
where s.is_ms_shipped = 0
order by [{nameof(Result.SchemaName)}], [{nameof(Result.SequenceName)}]";
}
