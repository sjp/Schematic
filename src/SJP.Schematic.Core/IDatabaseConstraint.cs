namespace SJP.Schematic.Core;

/// <summary>
/// Defines a database constraint, i.e. a rule the database applies to a table's rows.
/// </summary>
/// <seealso cref="IDatabaseOptional" />
public interface IDatabaseConstraint : IDatabaseOptional
{
    /// <summary>
    /// <para>Indicates whether the database has verified that the existing rows satisfy the constraint.</para>
    /// <para>
    /// A constraint that is enabled but not validated is enforced for new data only; the database
    /// cannot rely upon it when planning queries. Databases that always validate a constraint's
    /// existing data report <see langword="true" />.
    /// </para>
    /// </summary>
    /// <value><see langword="true" /> if the existing rows are known to satisfy the constraint; otherwise, <see langword="false" />.</value>
    bool IsValidated { get; }

    /// <summary>
    /// Describes when the database checks the constraint. Databases without deferrable constraints
    /// report <see cref="ConstraintDeferrability.NotDeferrable"/>.
    /// </summary>
    /// <value>A deferrability value.</value>
    ConstraintDeferrability Deferrability { get; }
}
