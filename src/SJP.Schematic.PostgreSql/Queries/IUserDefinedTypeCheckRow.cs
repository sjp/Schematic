namespace SJP.Schematic.PostgreSql.Queries;

/// <summary>
/// The shape of a <c>pg_constraint</c> row describing a check declared on a domain. The 'all types'
/// and 'single type' queries project the same columns, so the provider maps both through this.
/// </summary>
internal interface IUserDefinedTypeCheckRow
{
    string? ConstraintName { get; }

    string? Definition { get; }

    /// <summary>
    /// Whether the existing values have been verified against the constraint. <see langword="false" />
    /// for a constraint declared or left <c>NOT VALID</c>.
    /// </summary>
    bool IsValidated { get; }
}
