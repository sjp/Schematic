using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Queries;

internal static class GetV12TableChecks
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string? ConstraintName { get; init; }

        public required string? Definition { get; init; }
    }

    internal const string Sql = $"""

select
    c.conname as "{nameof(Result.ConstraintName)}",
    pg_catalog.pg_get_constraintdef(c.oid) as "{nameof(Result.Definition)}"
from pg_catalog.pg_namespace ns
inner join pg_catalog.pg_class t on ns.oid = t.relnamespace
inner join pg_catalog.pg_constraint c on c.conrelid = t.oid
where
    c.contype = 'c'
    and t.relname = @{nameof(Query.TableName)}
    and ns.nspname = @{nameof(Query.SchemaName)}
""";
}