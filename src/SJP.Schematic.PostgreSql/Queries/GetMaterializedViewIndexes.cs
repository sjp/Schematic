using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetMaterializedViewIndexes
{
    // The shape of an index is the same whether it is attached to a table or a materialized view, so
    // the rows are described by GetTableIndexes.Result and mapped by the same code.
    internal sealed record Query : ISqlQuery<GetTableIndexes.Result>
    {
        public required string SchemaName { get; init; }

        public required string ViewName { get; init; }
    }

    internal const string Sql = $"""

select
    i.relname as "{nameof(GetTableIndexes.Result.IndexName)}",
    idx.indisunique as "{nameof(GetTableIndexes.Result.IsUnique)}",
    idx.indisprimary as "{nameof(GetTableIndexes.Result.IsPrimary)}",
    pg_catalog.pg_get_expr(idx.indpred, idx.indrelid) as "{nameof(GetTableIndexes.Result.FilterDefinition)}",
    idx.indnkeyatts as "{nameof(GetTableIndexes.Result.KeyColumnCount)}",
    pg_catalog.generate_subscripts(idx.indkey, 1) as "{nameof(GetTableIndexes.Result.IndexColumnId)}",
    pg_catalog.unnest(array(
        select pg_catalog.pg_get_indexdef(idx.indexrelid, k + 1, true)
        from pg_catalog.generate_subscripts(idx.indkey, 1) k
        order by k
    )) as "{nameof(GetTableIndexes.Result.IndexColumnExpression)}",
    pg_catalog.unnest(array(
        -- the property is null for included columns and for access methods without an ordering
        select coalesce(pg_catalog.pg_index_column_has_property(idx.indexrelid, k + 1, 'desc'), false)
        from pg_catalog.generate_subscripts(idx.indkey, 1) k
        order by k
    )) as "{nameof(GetTableIndexes.Result.IsDescending)}",
    pg_catalog.unnest(array(
        select coalesce(pg_catalog.pg_index_column_has_property(idx.indexrelid, k + 1, 'nulls_first'), false)
        from pg_catalog.generate_subscripts(idx.indkey, 1) k
        order by k
    )) as "{nameof(GetTableIndexes.Result.IsNullsFirst)}",
    pg_catalog.unnest(array(
        -- the default collation is not worth reporting, it is simply the one the column already has
        select nullif(coll.collname, 'default')
        from pg_catalog.generate_subscripts(idx.indkey, 1) k
        left join pg_catalog.pg_collation coll on coll.oid = idx.indcollation[k]
        order by k
    )) as "{nameof(GetTableIndexes.Result.IndexColumnCollation)}",
    (idx.indexprs is not null) or (idx.indkey::int[] @> array[0]) as "{nameof(GetTableIndexes.Result.IsFunctional)}",
    am.amname as "{nameof(GetTableIndexes.Result.IndexMethod)}",
    idx.indisvalid as "{nameof(GetTableIndexes.Result.IsValid)}"
from pg_catalog.pg_index idx
    inner join pg_catalog.pg_class t on idx.indrelid = t.oid
    inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
    inner join pg_catalog.pg_class i on i.oid = idx.indexrelid
    inner join pg_catalog.pg_am am on am.oid = i.relam
where
    t.relkind = 'm'
    and t.relname = @{nameof(Query.ViewName)}
    and ns.nspname = @{nameof(Query.SchemaName)}
order by i.relname, "{nameof(GetTableIndexes.Result.IndexColumnId)}"
""";
}
