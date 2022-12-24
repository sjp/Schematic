using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetTableIndexes
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string? IndexName { get; init; }

        public required bool IsUnique { get; init; }

        public required bool IsPrimary { get; init; }

        public required string? FilterDefinition { get; init; }

        public required int KeyColumnCount { get; init; }

        public required int IndexColumnId { get; init; }

        public required string? IndexColumnExpression { get; init; }

        public required bool IsDescending { get; init; }

        public required bool IsFunctional { get; init; }
    }

    internal const string Sql = @$"
select
    i.relname as ""{nameof(Result.IndexName)}"",
    idx.indisunique as ""{nameof(Result.IsUnique)}"",
    idx.indisprimary as ""{nameof(Result.IsPrimary)}"",
    pg_catalog.pg_get_expr(idx.indpred, idx.indrelid) as ""{nameof(Result.FilterDefinition)}"",
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