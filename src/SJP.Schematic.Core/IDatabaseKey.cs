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
}