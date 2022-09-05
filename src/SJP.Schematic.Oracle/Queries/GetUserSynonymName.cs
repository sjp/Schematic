namespace SJP.Schematic.Oracle.Queries;

internal static class GetUserSynonymName
{
    internal sealed record Query
    {
        public string SynonymName { get; init; } = default!;
    }

    internal const string Sql = @$"
select s.SYNONYM_NAME
from SYS.USER_SYNONYMS s
inner join SYS.ALL_OBJECTS o on s.SYNONYM_NAME = o.OBJECT_NAME
where o.OWNER = SYS_CONTEXT('USERENV', 'CURRENT_USER') and s.SYNONYM_NAME = :{nameof(Query.SynonymName)} and o.ORACLE_MAINTAINED <> 'Y'";
}