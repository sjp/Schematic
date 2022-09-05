namespace SJP.Schematic.Oracle.Queries;

internal static class GetUserSynonymDefinition
{
    internal sealed record Query
    {
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
    s.DB_LINK as ""{nameof(Result.TargetDatabaseName)}"",
    s.TABLE_OWNER as ""{nameof(Result.TargetSchemaName)}"",
    s.TABLE_NAME as ""{nameof(Result.TargetObjectName)}""
from SYS.USER_SYNONYMS s
inner join SYS.ALL_OBJECTS o on s.SYNONYM_NAME = o.OBJECT_NAME
where s.SYNONYM_NAME = :{nameof(Query.SynonymName)} and o.OWNER = SYS_CONTEXT('USERENV', 'CURRENT_USER') and o.ORACLE_MAINTAINED <> 'Y'";
}