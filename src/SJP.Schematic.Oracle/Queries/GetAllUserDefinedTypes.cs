namespace SJP.Schematic.Oracle.Queries;

internal static class GetAllUserDefinedTypes
{
    internal sealed record Result : IUserDefinedTypeDefinitionRow
    {
        public required string? SchemaName { get; init; }

        public required string? TypeName { get; init; }

        /// <summary>
        /// <c>ALL_TYPES.TYPECODE</c>, i.e. <c>OBJECT</c> or <c>COLLECTION</c>.
        /// </summary>
        public required string? TypeCode { get; init; }

        public required string? ElementTypeSchema { get; init; }

        public required string? ElementTypeName { get; init; }

        public required int ElementLength { get; init; }

        public required int ElementPrecision { get; init; }

        public required int ElementScale { get; init; }

        public required string? ElementCollation { get; init; }
    }

    internal const string Sql = $"""

select
    t.OWNER as "{nameof(Result.SchemaName)}",
    t.TYPE_NAME as "{nameof(Result.TypeName)}",
    t.TYPECODE as "{nameof(Result.TypeCode)}",
    ct.ELEM_TYPE_OWNER as "{nameof(Result.ElementTypeSchema)}",
    ct.ELEM_TYPE_NAME as "{nameof(Result.ElementTypeName)}",
    coalesce(ct.LENGTH, 0) as "{nameof(Result.ElementLength)}",
    coalesce(ct.PRECISION, 0) as "{nameof(Result.ElementPrecision)}",
    coalesce(ct.SCALE, 0) as "{nameof(Result.ElementScale)}",
    ct.CHARACTER_SET_NAME as "{nameof(Result.ElementCollation)}"
from SYS.ALL_TYPES t
-- a collection type is described by the element it holds, which only ALL_COLL_TYPES reports
left join SYS.ALL_COLL_TYPES ct on ct.OWNER = t.OWNER and ct.TYPE_NAME = t.TYPE_NAME
inner join SYS.ALL_OBJECTS o on o.OWNER = t.OWNER and o.OBJECT_NAME = t.TYPE_NAME
where o.OBJECT_TYPE = 'TYPE' and o.ORACLE_MAINTAINED <> 'Y'
order by t.OWNER, t.TYPE_NAME
""";
}
