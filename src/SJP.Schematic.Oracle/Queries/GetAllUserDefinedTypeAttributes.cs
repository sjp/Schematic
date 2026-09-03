namespace SJP.Schematic.Oracle.Queries;

internal static class GetAllUserDefinedTypeAttributes
{
    internal sealed record Result : IUserDefinedTypeAttributeRow
    {
        public required string? SchemaName { get; init; }

        public required string? TypeName { get; init; }

        public required string? AttributeName { get; init; }

        public required string? AttributeTypeSchema { get; init; }

        public required string? AttributeTypeName { get; init; }

        public required int DataLength { get; init; }

        public required int Precision { get; init; }

        public required int Scale { get; init; }

        public required string? Collation { get; init; }
    }

    internal const string Sql = $"""

select
    a.OWNER as "{nameof(Result.SchemaName)}",
    a.TYPE_NAME as "{nameof(Result.TypeName)}",
    a.ATTR_NAME as "{nameof(Result.AttributeName)}",
    a.ATTR_TYPE_OWNER as "{nameof(Result.AttributeTypeSchema)}",
    a.ATTR_TYPE_NAME as "{nameof(Result.AttributeTypeName)}",
    coalesce(a.LENGTH, 0) as "{nameof(Result.DataLength)}",
    coalesce(a.PRECISION, 0) as "{nameof(Result.Precision)}",
    coalesce(a.SCALE, 0) as "{nameof(Result.Scale)}",
    a.CHARACTER_SET_NAME as "{nameof(Result.Collation)}"
from SYS.ALL_TYPE_ATTRS a
inner join SYS.ALL_OBJECTS o on o.OWNER = a.OWNER and o.OBJECT_NAME = a.TYPE_NAME
where o.OBJECT_TYPE = 'TYPE' and o.ORACLE_MAINTAINED <> 'Y'
order by a.OWNER, a.TYPE_NAME, a.ATTR_NO
""";
}
