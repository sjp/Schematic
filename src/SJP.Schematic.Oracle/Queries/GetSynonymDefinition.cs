namespace SJP.Schematic.Oracle.Queries;

internal static class GetSynonymDefinition
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string SynonymName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string? TargetDatabaseName { get; init; }

        public string? TargetSchemaName { get; init; }

        public string? TargetObjectName { get; init; }
    }

    internal const string Sql = @$"
select distinct
    s.DB_LINK as ""{ nameof(Result.TargetDatabaseName) }"",
    s.TABLE_OWNER as ""{ nameof(Result.TargetSchemaName) }"",
    s.TABLE_NAME as ""{ nameof(Result.TargetObjectName) }""
from SYS.ALL_SYNONYMS s
inner join SYS.ALL_OBJECTS o on s.OWNER = o.OWNER and s.SYNONYM_NAME = o.OBJECT_NAME
where s.OWNER = :{ nameof(Query.SchemaName) } and s.SYNONYM_NAME = :{ nameof(Query.SynonymName) } and o.ORACLE_MAINTAINED <> 'Y'";
}