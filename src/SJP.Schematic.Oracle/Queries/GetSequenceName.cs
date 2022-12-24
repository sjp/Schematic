using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Queries;

internal static class GetSequenceName
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string SequenceName { get; init; }
    }

    internal sealed record Result
    {
        public required string SchemaName { get; init; }

        public required string SequenceName { get; init; }
    }

    internal const string Sql = @$"
select s.SEQUENCE_OWNER as ""{nameof(Result.SchemaName)}"", s.SEQUENCE_NAME as ""{nameof(Result.SequenceName)}""
from SYS.ALL_SEQUENCES s
inner join SYS.ALL_OBJECTS o on s.SEQUENCE_OWNER = o.OWNER and s.SEQUENCE_NAME = o.OBJECT_NAME
where s.SEQUENCE_OWNER = :{nameof(Query.SchemaName)} and s.SEQUENCE_NAME = :{nameof(Query.SequenceName)} and o.ORACLE_MAINTAINED <> 'Y'";
}