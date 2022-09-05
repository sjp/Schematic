namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetTableChecks
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string? ConstraintName { get; init; }

        public string? Definition { get; init; }
    }

    internal const string Sql = @$"
select
    c.conname as ""{nameof(Result.ConstraintName)}"",
    c.consrc as ""{nameof(Result.Definition)}""
from pg_catalog.pg_namespace ns
inner join pg_catalog.pg_class t on ns.oid = t.relnamespace
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
where
    c.contype = 'c'
    and t.relname = @{nameof(Query.TableName)}
    and ns.nspname = @{nameof(Query.SchemaName)}";
}