using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines a schema, i.e. the namespace that database objects are declared within.
/// </summary>
/// <remarks>
/// The schema name is held in <see cref="Identifier.LocalName"/> of <see cref="IDatabaseEntity.Name"/>.
/// A schema is not itself qualified by another schema.
/// </remarks>
/// <seealso cref="IDatabaseEntity" />
public interface IDatabaseSchema : IDatabaseEntity
{
    /// <summary>
    /// The name of the principal that owns the schema, when the database records one.
    /// </summary>
    /// <value>The owner of the schema, if available.</value>
    Option<string> Owner { get; }

    /// <summary>
    /// Whether this is the schema that unqualified object names resolve to, i.e. whether it is
    /// the schema given by <see cref="IIdentifierDefaults.Schema"/>.
    /// </summary>
    /// <value><see langword="true" /> if this is the default schema; otherwise <see langword="false" />.</value>
    bool IsDefault { get; }

    /// <summary>
    /// Whether the schema is one that the database itself declares, e.g. <c>INFORMATION_SCHEMA</c>
    /// or <c>pg_catalog</c>, rather than one declared by a user.
    /// </summary>
    /// <value><see langword="true" /> if this is a system schema; otherwise <see langword="false" />.</value>
    bool IsSystem { get; }
}
