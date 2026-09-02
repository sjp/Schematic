using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines a database key constraint type.
/// </summary>
/// <seealso cref="IDatabaseOptional" />
public interface IDatabaseKey : IDatabaseOptional
{
    /// <summary>
    /// The name of the key constraint, if available.
    /// </summary>
    /// <value>A constraint name, if available.</value>
    Option<Identifier> Name { get; }

    /// <summary>
    /// The columns that defines the key constraint.
    /// </summary>
    /// <value>A collection of database columns.</value>
    IReadOnlyCollection<IDatabaseColumn> Columns { get; }

    /// <summary>
    /// The type of key constraint, e.g. primary, unique, foreign.
    /// </summary>
    /// <value>A key constraint type.</value>
    DatabaseKeyType KeyType { get; }

    /// <summary>
    /// The index that the database uses to enforce the key constraint.
    /// </summary>
    /// <remarks>
    /// Only primary and unique keys are backed by an index, and only when the database exposes it.
    /// The backing index is not repeated in <see cref="IRelationalDatabaseTable.Indexes"/>.
    /// </remarks>
    /// <value>An index, if the database reports one for the constraint.</value>
    Option<IDatabaseIndex> BackingIndex { get; }
}