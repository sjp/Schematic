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

        public required decimal? MinValue { get; init; }

        public required decimal? MaxValue { get; init; }

        public required decimal StartValue { get; init; }
    }

    internal const string Sql = @$"
-- sys.sequences reports the bounds as sql_variant, whose runtime type follows the sequence's
-- own data type, so they're converted to a single type wide enough to hold any of them.
-- A decimal(29+, 0) sequence can hold values beyond the client decimal range
-- (79228162514264337593543950335), which would fail the whole read. Such bounds are reported
-- as missing, while the start and increment, which only get there when set explicitly, are clamped.
select
    ss.name as [{nameof(Result.SchemaName)}],
    s.name as [{nameof(Result.SequenceName)}],
    ts.name as [{nameof(Result.TypeSchemaName)}],
    t.name as [{nameof(Result.TypeName)}],
    cast(t.max_length as int) as [{nameof(Result.TypeMaxLength)}],
    cast(s.precision as int) as [{nameof(Result.Precision)}],
    cast(s.scale as int) as [{nameof(Result.Scale)}],
    case when v.StartValue > 79228162514264337593543950335 then 79228162514264337593543950335
         when v.StartValue < -79228162514264337593543950335 then -79228162514264337593543950335
         else v.StartValue end as [{nameof(Result.StartValue)}],
    case when v.Increment > 79228162514264337593543950335 then 79228162514264337593543950335
         when v.Increment < -79228162514264337593543950335 then -79228162514264337593543950335
         else v.Increment end as [{nameof(Result.Increment)}],
    case when abs(v.MinValue) <= 79228162514264337593543950335 then v.MinValue end as [{nameof(Result.MinValue)}],
    case when abs(v.MaxValue) <= 79228162514264337593543950335 then v.MaxValue end as [{nameof(Result.MaxValue)}],
    s.is_cycling as [{nameof(Result.Cycle)}],
    s.is_cached as [{nameof(Result.IsCached)}],
    s.cache_size as [{nameof(Result.CacheSize)}]
from sys.sequences s
inner join sys.schemas ss on s.schema_id = ss.schema_id
inner join sys.types t on s.user_type_id = t.user_type_id
inner join sys.schemas ts on t.schema_id = ts.schema_id
cross apply (
    select
        convert(decimal(38, 0), s.start_value) as StartValue,
        convert(decimal(38, 0), s.increment) as Increment,
        convert(decimal(38, 0), s.minimum_value) as MinValue,
        convert(decimal(38, 0), s.maximum_value) as MaxValue
) v
where s.is_ms_shipped = 0
order by ss.name, s.name";
}
