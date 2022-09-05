namespace SJP.Schematic.Oracle.Queries;

internal static class GetSynonymName
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string SynonymName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string SynonymName { get; init; } = default!;
    }

    internal const string Sql = @$"
select s.OWNER as ""{nameof(Result.SchemaName)}"", s.SYNONYM_NAME as ""{nameof(Result.SynonymName)}""
from SYS.ALL_SYNONYMS s
inner join SYS.ALL_OBJECTS o on s.OWNER = o.OWNER and s.SYNONYM_NAME = o.OBJECT_NAME
where s.OWNER = :{nameof(Query.SchemaName)} and s.SYNONYM_NAME = :{nameof(Query.SynonymName)} and o.ORACLE_MAINTAINED <> 'Y'";
}