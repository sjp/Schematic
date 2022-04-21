namespace SJP.Schematic.MySql.Queries;

internal static class GetTableCheckConstraints
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string ConstraintName { get; init; } = default!;

        public string Definition { get; init; } = default!;

        public string Enforced { get; init; } = default!;
    }

    internal const string Sql = @$"
select
    cc.constraint_name as `{ nameof(Result.ConstraintName) }`,
    cc.check_clause as `{ nameof(Result.Definition) }`,
    tc.enforced as `{ nameof(Result.Enforced) }`
from information_schema.table_constraints tc
inner join information_schema.check_constraints cc on tc.table_schema = cc.constraint_schema and tc.constraint_name = cc.constraint_name
where tc.table_schema = @{ nameof(Query.SchemaName) } and tc.table_name = @{ nameof(Query.TableName) } and tc.constraint_type = 'CHECK'";
}