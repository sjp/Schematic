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

    // Index column name: the raw column name, unlike pg_get_indexdef which quotes it where required;
    // expression columns are stored as attribute number 0, which matches no attribute and so gives null.
    //
    // Descending: the property is null for included columns and for access methods without an ordering.
    //
    // Collation: the default collation is not worth reporting, it is simply the one the column already has.
    internal const string Sql = $"""
select
    i.relname as "{nameof(GetTableIndexes.Result.IndexName)}",
    idx.indisunique as "{nameof(GetTableIndexes.Result.IsUnique)}",
    idx.indisprimary as "{nameof(GetTableIndexes.Result.IsPrimary)}",
    pg_catalog.pg_get_expr(idx.indpred, idx.indrelid) as "{nameof(GetTableIndexes.Result.FilterDefinition)}",
    idx.indnkeyatts as "{nameof(GetTableIndexes.Result.KeyColumnCount)}",
    k.k as "{nameof(GetTableIndexes.Result.IndexColumnId)}",
    pg_catalog.pg_get_indexdef(idx.indexrelid, k.k + 1, true) as "{nameof(GetTableIndexes.Result.IndexColumnExpression)}",
    a.attname::text as "{nameof(GetTableIndexes.Result.IndexColumnName)}",
    coalesce(pg_catalog.pg_index_column_has_property(idx.indexrelid, k.k + 1, 'desc'), false) as "{nameof(GetTableIndexes.Result.IsDescending)}",
    coalesce(pg_catalog.pg_index_column_has_property(idx.indexrelid, k.k + 1, 'nulls_first'), false) as "{nameof(GetTableIndexes.Result.IsNullsFirst)}",
    nullif(coll.collname, 'default') as "{nameof(GetTableIndexes.Result.IndexColumnCollation)}",
    am.amname as "{nameof(GetTableIndexes.Result.IndexMethod)}",
    idx.indisvalid as "{nameof(GetTableIndexes.Result.IsValid)}"
from pg_catalog.pg_index idx
    inner join pg_catalog.pg_class i on i.oid = idx.indexrelid
    inner join pg_catalog.pg_am am on am.oid = i.relam
    cross join lateral pg_catalog.generate_subscripts(idx.indkey, 1) as k(k)
    left join pg_catalog.pg_attribute a on a.attrelid = idx.indrelid and a.attnum = idx.indkey[k.k]
    left join pg_catalog.pg_collation coll on coll.oid = idx.indcollation[k.k]
where idx.indrelid = (
    select t.oid
    from pg_catalog.pg_class t
    inner join pg_catalog.pg_namespace ns on ns.oid = t.relnamespace
    where t.relkind = 'm'
        and t.relname = @{nameof(Query.ViewName)}
        and ns.nspname = @{nameof(Query.SchemaName)}
)
order by i.relname, "{nameof(GetTableIndexes.Result.IndexColumnId)}"
""";
}
