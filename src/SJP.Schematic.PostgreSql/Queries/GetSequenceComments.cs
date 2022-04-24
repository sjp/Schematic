﻿namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetSequenceComments
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string SequenceName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string? Comment { get; init; }
    }

    internal const string Sql = @$"
select
    d.description as ""{ nameof(Result.Comment) }""
from pg_catalog.pg_namespace nc
inner join pg_catalog.pg_class c on c.relnamespace = nc.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where nc.nspname = @{ nameof(Query.SchemaName) } and c.relname = @{ nameof(Query.SequenceName) }
    and nc.nspname not in ('pg_catalog', 'information_schema')
    and c.relkind = 'S'
";
}