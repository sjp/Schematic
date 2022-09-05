namespace SJP.Schematic.SqlServer.Queries;

internal static class GetTableChecks
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

        public bool IsDisabled { get; init; }
    }

    internal const string Sql = @$"
select
    cc.name as [{nameof(Result.ConstraintName)}],
    cc.definition as [{nameof(Result.Definition)}],
    cc.is_disabled as [{nameof(Result.IsDisabled)}]
from sys.tables t
inner join sys.check_constraints cc on t.object_id = cc.parent_object_id
where schema_name(t.schema_id) = @{nameof(Query.SchemaName)} and t.name = @{nameof(Query.TableName)} and t.is_ms_shipped = 0";
}