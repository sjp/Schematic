using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetUserDefinedTypeAttributes
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TypeName { get; init; }
    }

    internal sealed record Result : IUserDefinedTypeAttributeRow
    {
        public required string? AttributeName { get; init; }

        public required string? DataType { get; init; }

        public required string? UdtSchema { get; init; }

        public required string? UdtName { get; init; }

        /// <summary>
        /// The <c>pg_type.typtype</c> of the attribute's type.
        /// </summary>
        public required string? TypeKind { get; init; }

        public required string? ElementTypeSchema { get; init; }

        public required string? ElementTypeName { get; init; }

        public required string? ElementTypeKind { get; init; }

        public required string[]? EnumLabels { get; init; }

        public required int CharacterMaximumLength { get; init; }

        public required int NumericPrecision { get; init; }

        public required int NumericPrecisionRadix { get; init; }

        public required int NumericScale { get; init; }

        public required string? CollationName { get; init; }

        public required string? IsNullable { get; init; }

        public required string? AttributeDefault { get; init; }
    }

    internal const string Sql = $"""

select
    a.attribute_name as "{nameof(Result.AttributeName)}",
    a.data_type as "{nameof(Result.DataType)}",
    a.attribute_udt_schema as "{nameof(Result.UdtSchema)}",
    a.attribute_udt_name as "{nameof(Result.UdtName)}",
    udt.typtype::text as "{nameof(Result.TypeKind)}",
    elem_ns.nspname as "{nameof(Result.ElementTypeSchema)}",
    elem.typname as "{nameof(Result.ElementTypeName)}",
    elem.typtype::text as "{nameof(Result.ElementTypeKind)}",
    lbl.labels as "{nameof(Result.EnumLabels)}",
    coalesce(a.character_maximum_length, 0) as "{nameof(Result.CharacterMaximumLength)}",
    coalesce(a.numeric_precision, 0) as "{nameof(Result.NumericPrecision)}",
    coalesce(a.numeric_precision_radix, 0) as "{nameof(Result.NumericPrecisionRadix)}",
    coalesce(a.numeric_scale, 0) as "{nameof(Result.NumericScale)}",
    a.collation_name as "{nameof(Result.CollationName)}",
    a.is_nullable as "{nameof(Result.IsNullable)}",
    a.attribute_default as "{nameof(Result.AttributeDefault)}"
from information_schema.attributes a
-- information_schema names an attribute's type as ARRAY or USER-DEFINED whenever it is not built
-- in, so the type itself is resolved to learn what kind of type it is, what an array holds, and
-- which labels an enum permits
left join pg_catalog.pg_namespace udt_ns on udt_ns.nspname = a.attribute_udt_schema
left join pg_catalog.pg_type udt on udt.typnamespace = udt_ns.oid and udt.typname = a.attribute_udt_name
left join pg_catalog.pg_type elem on elem.oid = udt.typelem
left join pg_catalog.pg_namespace elem_ns on elem_ns.oid = elem.typnamespace
left join lateral (
    select array_agg(en.enumlabel::text order by en.enumsortorder) as labels
    from pg_catalog.pg_enum en
    where en.enumtypid = case when udt.typtype = 'e' then udt.oid when elem.typtype = 'e' then elem.oid end
) lbl on true
where a.udt_schema = @{nameof(Query.SchemaName)} and a.udt_name = @{nameof(Query.TypeName)}
order by a.ordinal_position
""";
}
