namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetV11TableIndexes
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string? IndexName { get; init; }

        public bool IsUnique { get; init; }

        public bool IsPrimary { get; init; }

        public string? FilterDefinition { get; init; }

        public int KeyColumnCount { get; init; }

        public int IndexColumnId { get; init; }

        public string? IndexColumnExpression { get; init; }

        public bool IsDescending { get; init; }

        public bool IsFunctional { get; init; }
    }

    internal const string Sql = @$"
select
    i.relname as ""{nameof(Result.IndexName)}"",
    idx.indisunique as ""{nameof(Result.IsUnique)}"",
    idx.indisprimary as ""{nameof(Result.IsPrimary)}"",
    pg_catalog.pg_get_expr(idx.indpred, idx.indrelid) as ""{nameof(Result.FilterDefinition)}"",
    idx.indnkeyatts as ""{nameof(Result.KeyColumnCount)}"",
    pg_catalog.generate_subscripts(idx.indkey, 1) as ""{nameof(Result.IndexColumnId)}"",
    pg_catalog.unnest(array(
        select pg_catalog.pg_get_indexdef(idx.indexrelid, k + 1, true)
        from pg_catalog.generate_subscripts(idx.indkey, 1) k
        order by k
    )) as ""{nameof(Result.IndexColumnExpression)}"",
    pg_catalog.unnest(array(
        select pg_catalog.pg_index_column_has_property(idx.indexrelid, k + 1, 'desc')
        from pg_catalog.generate_subscripts(idx.indkey, 1) k
        order by k
    )) as ""{nameof(Result.IsDescending)}"",
    (idx.indexprs is not null) or (idx.indkey::int[] @> array[0]) as ""{nameof(Result.IsFunctional)}""
from pg_catalog.pg_index idx
    inner join pg_catalog.pg_class t on idx.indrelid = t.oid
    inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
    inner join pg_catalog.pg_class i on i.oid = idx.indexrelid
where
    t.relkind = 'r'
    and t.relname = @{nameof(Query.TableName)}
    and ns.nspname = @{nameof(Query.SchemaName)}";
}