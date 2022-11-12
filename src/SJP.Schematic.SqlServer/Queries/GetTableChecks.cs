namespace SJP.Schematic.SqlServer.Queries;

internal static class GetTableChecks
{
    internal sealed record Query
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string ConstraintName { get; init; }

        public required string Definition { get; init; }

        public required bool IsDisabled { get; init; }
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