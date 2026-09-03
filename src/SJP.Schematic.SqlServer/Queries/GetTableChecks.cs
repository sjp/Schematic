using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Queries;

internal static class GetTableChecks
{
    internal sealed record Query : ISqlQuery<Result>
    {
        public required string SchemaName { get; init; }

        public required string TableName { get; init; }
    }

    internal sealed record Result
    {
        public required string ConstraintName { get; init; }

        public required string Definition { get; init; }

        public required bool IsDisabled { get; init; }

        /// <summary>
        /// Set when the constraint was created or re-enabled <c>WITH NOCHECK</c>, i.e. SQL Server has
        /// not verified the existing rows against it.
        /// </summary>
        public required bool IsNotTrusted { get; init; }
    }

    internal const string Sql = @$"
select
    cc.name as [{nameof(Result.ConstraintName)}],
    cc.definition as [{nameof(Result.Definition)}],
    cc.is_disabled as [{nameof(Result.IsDisabled)}],
    cc.is_not_trusted as [{nameof(Result.IsNotTrusted)}]
from sys.tables t
inner join sys.check_constraints cc on t.object_id = cc.parent_object_id
where t.schema_id = schema_id(@{nameof(Query.SchemaName)}) and t.name = @{nameof(Query.TableName)} and t.is_ms_shipped = 0";
}