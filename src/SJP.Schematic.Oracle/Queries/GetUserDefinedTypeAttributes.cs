using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

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

        public required string? AttributeTypeSchema { get; init; }

        public required string? AttributeTypeName { get; init; }

        public required int DataLength { get; init; }

        public required int Precision { get; init; }

        public required int Scale { get; init; }

        public required string? Collation { get; init; }
    }

    internal const string Sql = $"""

select
    a.ATTR_NAME as "{nameof(Result.AttributeName)}",
    a.ATTR_TYPE_OWNER as "{nameof(Result.AttributeTypeSchema)}",
    a.ATTR_TYPE_NAME as "{nameof(Result.AttributeTypeName)}",
    coalesce(a.LENGTH, 0) as "{nameof(Result.DataLength)}",
    coalesce(a.PRECISION, 0) as "{nameof(Result.Precision)}",
    coalesce(a.SCALE, 0) as "{nameof(Result.Scale)}",
    a.CHARACTER_SET_NAME as "{nameof(Result.Collation)}"
from SYS.ALL_TYPE_ATTRS a
where a.OWNER = :{nameof(Query.SchemaName)} and a.TYPE_NAME = :{nameof(Query.TypeName)}
order by a.ATTR_NO
""";
}
