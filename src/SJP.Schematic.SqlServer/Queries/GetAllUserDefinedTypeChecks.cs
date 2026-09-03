namespace SJP.Schematic.SqlServer.Queries;

internal static class GetAllUserDefinedTypeChecks
{
    internal sealed record Result : IUserDefinedTypeCheckRow
    {
        public required string SchemaName { get; init; }

        public required string TypeName { get; init; }

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
    schema_name(tt.schema_id) as [{nameof(Result.SchemaName)}],
    tt.name as [{nameof(Result.TypeName)}],
    cc.name as [{nameof(Result.ConstraintName)}],
    cc.definition as [{nameof(Result.Definition)}],
    cc.is_disabled as [{nameof(Result.IsDisabled)}],
    cc.is_not_trusted as [{nameof(Result.IsNotTrusted)}]
from sys.table_types tt
inner join sys.check_constraints cc on tt.type_table_object_id = cc.parent_object_id
where tt.is_user_defined = 1";
}
