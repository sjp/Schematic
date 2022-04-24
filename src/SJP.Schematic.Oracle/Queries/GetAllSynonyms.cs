namespace SJP.Schematic.Oracle.Queries;

internal static class GetAllSynonyms
{
    internal sealed record Result
    {
        public string? SchemaName { get; init; }

        public string? SynonymName { get; init; }

        public string? TargetDatabaseName { get; init; }

        public string? TargetSchemaName { get; init; }

        public string? TargetObjectName { get; init; }
    }

    internal const string Sql = @$"
select distinct
    s.OWNER as ""{ nameof(Result.SchemaName) }"",
    s.SYNONYM_NAME as ""{ nameof(Result.SynonymName) }"",
    s.DB_LINK as ""{ nameof(Result.TargetDatabaseName) }"",
    s.TABLE_OWNER as ""{ nameof(Result.TargetSchemaName) }"",
    s.TABLE_NAME as ""{ nameof(Result.TargetObjectName) }""
from SYS.ALL_SYNONYMS s
inner join SYS.ALL_OBJECTS o on s.OWNER = o.OWNER and s.SYNONYM_NAME = o.OBJECT_NAME
where o.ORACLE_MAINTAINED <> 'Y'
order by s.DB_LINK, s.OWNER, s.SYNONYM_NAME";
}