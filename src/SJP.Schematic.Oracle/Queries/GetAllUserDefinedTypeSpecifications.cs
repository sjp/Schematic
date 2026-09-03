namespace SJP.Schematic.Oracle.Queries;

internal static class GetAllUserDefinedTypeSpecifications
{
    internal sealed record Result
    {
        public required string? SchemaName { get; init; }

        public required string? TypeName { get; init; }

        public required int LineNumber { get; init; }

        public required string? Definition { get; init; }
    }

    internal const string Sql = $"""

select
    s.OWNER as "{nameof(Result.SchemaName)}",
    s.NAME as "{nameof(Result.TypeName)}",
    s.LINE as "{nameof(Result.LineNumber)}",
    s.TEXT as "{nameof(Result.Definition)}"
from SYS.ALL_SOURCE s
inner join SYS.ALL_OBJECTS o on o.OWNER = s.OWNER and o.OBJECT_NAME = s.NAME
where s.TYPE = 'TYPE' and o.OBJECT_TYPE = 'TYPE' and o.ORACLE_MAINTAINED <> 'Y'
order by s.OWNER, s.NAME, s.LINE
""";
}
