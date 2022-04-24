namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetAllSequenceComments
{
    internal sealed record Result
    {
        public string? SchemaName { get; init; }

        public string? SequenceName { get; init; }

        public string? Comment { get; init; }
    }

    internal const string Sql = @$"
select
    nc.nspname as ""{ nameof(Result.SchemaName) }"",
    c.relname as ""{ nameof(Result.SequenceName) }"",
    d.description as ""{ nameof(Result.Comment) }""
from pg_catalog.pg_namespace nc
inner join pg_catalog.pg_class c on c.relnamespace = nc.oid
left join pg_catalog.pg_description d on d.objoid = c.oid
where nc.nspname not in ('pg_catalog', 'information_schema') and c.relkind = 'S'
order by nc.nspname, c.relname";
}