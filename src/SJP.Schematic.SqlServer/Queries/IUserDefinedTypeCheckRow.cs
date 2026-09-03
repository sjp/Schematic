namespace SJP.Schematic.SqlServer.Queries;

/// <summary>
/// The shape of a <c>sys.check_constraints</c> row that describes a check declared on a table type.
/// The 'all types' and 'single type' queries project the same columns, so the provider maps both
/// through this.
/// </summary>
internal interface IUserDefinedTypeCheckRow
{
    string ConstraintName { get; }

    string Definition { get; }

    bool IsDisabled { get; }

    bool IsNotTrusted { get; }
}
