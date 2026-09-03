namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetAllUserDefinedTypeDefinitions
{
    internal sealed record Result : IUserDefinedTypeDefinitionRow
    {
        public required string? SchemaName { get; init; }

        public required string? TypeName { get; init; }

        /// <summary>
        /// The <c>pg_type.typtype</c> of the type.
        /// </summary>
        public required string? TypeKind { get; init; }

        /// <summary>
        /// <c>pg_type.typnotnull</c>, set for a domain declared <c>NOT NULL</c>.
        /// </summary>
        public required bool IsNotNull { get; init; }

        /// <summary>
        /// <c>pg_type.typdefault</c>, the default expression declared by a domain.
        /// </summary>
        public required string? DefaultValue { get; init; }

        /// <summary>
        /// The name of the type a domain is defined over, or of a range's subtype.
        /// </summary>
        public required string? BaseTypeName { get; init; }

        /// <summary>
        /// The schema of the type named by <see cref="BaseTypeName"/>.
        /// </summary>
        public required string? BaseTypeSchema { get; init; }

        public required int CharacterMaximumLength { get; init; }

        public required int NumericPrecision { get; init; }

        public required int NumericPrecisionRadix { get; init; }

        public required int NumericScale { get; init; }

        public required string? CollationName { get; init; }

        /// <summary>
        /// The labels of an enum type, ordered by <c>pg_enum.enumsortorder</c>.
        /// </summary>
        public required string[]? EnumLabels { get; init; }
    }

    internal const string Sql = $"""

select
    n.nspname as "{nameof(Result.SchemaName)}",
    t.typname as "{nameof(Result.TypeName)}",
    t.typtype::text as "{nameof(Result.TypeKind)}",
    t.typnotnull as "{nameof(Result.IsNotNull)}",
    t.typdefault as "{nameof(Result.DefaultValue)}",
    -- information_schema names a domain's underlying type as USER-DEFINED or ARRAY whenever it is
    -- not a built-in one, in which case udt_name names it instead
    coalesce(nullif(nullif(d.data_type, 'USER-DEFINED'), 'ARRAY'), d.udt_name, bt.typname) as "{nameof(Result.BaseTypeName)}",
    coalesce(d.udt_schema, bt_ns.nspname) as "{nameof(Result.BaseTypeSchema)}",
    coalesce(d.character_maximum_length, 0) as "{nameof(Result.CharacterMaximumLength)}",
    coalesce(d.numeric_precision, 0) as "{nameof(Result.NumericPrecision)}",
    coalesce(d.numeric_precision_radix, 0) as "{nameof(Result.NumericPrecisionRadix)}",
    coalesce(d.numeric_scale, 0) as "{nameof(Result.NumericScale)}",
    d.collation_name as "{nameof(Result.CollationName)}",
    lbl.labels as "{nameof(Result.EnumLabels)}"
from pg_catalog.pg_type t
inner join pg_catalog.pg_namespace n on n.oid = t.typnamespace
left join pg_catalog.pg_class cls on cls.oid = t.typrelid
-- a domain is described by the type it is defined over, which information_schema.domains reports
-- with its declared length and precision already decoded out of the type modifier
left join information_schema.domains d on d.domain_schema = n.nspname and d.domain_name = t.typname
-- a range type is instead described by its subtype, which only pg_range names
left join pg_catalog.pg_range r on r.rngtypid = t.oid
left join pg_catalog.pg_type bt on bt.oid = r.rngsubtype
left join pg_catalog.pg_namespace bt_ns on bt_ns.oid = bt.typnamespace
left join lateral (
    select array_agg(en.enumlabel::text order by en.enumsortorder) as labels
    from pg_catalog.pg_enum en
    where en.enumtypid = t.oid
) lbl on true
where n.nspname not in ('pg_catalog', 'information_schema', 'pg_toast')
    and t.typtype in ('d', 'e', 'c', 'r')
    -- every table, view and sequence also owns a composite type describing its row; only a
    -- standalone composite type, whose relkind is 'c', is a type a user declared
    and (t.typtype <> 'c' or cls.relkind = 'c')
order by n.nspname, t.typname
""";
}
