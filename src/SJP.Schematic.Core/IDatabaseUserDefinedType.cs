using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines a type declared by a user within a database, e.g. a PostgreSQL enum or a SQL Server table type.
/// </summary>
/// <seealso cref="IDatabaseEntity" />
public interface IDatabaseUserDefinedType : IDatabaseEntity
{
    /// <summary>
    /// The kind of type that has been declared.
    /// </summary>
    /// <value>A user-defined type kind.</value>
    UserDefinedTypeKind Kind { get; }

    /// <summary>
    /// The type that this type is defined in terms of, if available. This is the aliased type for
    /// <see cref="UserDefinedTypeKind.Alias"/> and <see cref="UserDefinedTypeKind.Domain"/>, the
    /// subtype for <see cref="UserDefinedTypeKind.Range"/>, and the element type for
    /// <see cref="UserDefinedTypeKind.Collection"/>.
    /// </summary>
    /// <value>A base type, if available.</value>
    Option<IDbType> BaseType { get; }

    /// <summary>
    /// The values that a value of this type is restricted to.
    /// </summary>
    /// <value>The permitted values, for a <see cref="UserDefinedTypeKind.Enum"/> type; otherwise empty.</value>
    IReadOnlyList<string> EnumValues { get; }

    /// <summary>
    /// The named attributes that comprise this type.
    /// </summary>
    /// <value>A collection of attributes, for a <see cref="UserDefinedTypeKind.Composite"/> or
    /// <see cref="UserDefinedTypeKind.Table"/> type; otherwise empty.</value>
    IReadOnlyList<IDatabaseColumn> Attributes { get; }

    /// <summary>
    /// The check constraints that a value of this type must satisfy.
    /// </summary>
    /// <value>A collection of check constraints, empty when the type declares none.</value>
    IReadOnlyCollection<IDatabaseCheckConstraint> Checks { get; }

    /// <summary>
    /// Determines whether a value of this type can be <see langword="null" />.
    /// </summary>
    /// <value><see langword="true" /> if a value of this type can be <see langword="null" />; otherwise, <see langword="false" />.</value>
    bool IsNullable { get; }

    /// <summary>
    /// An expression that creates a default value for a column of this type when one is omitted.
    /// </summary>
    /// <value>The default value for the type, if available.</value>
    Option<string> DefaultValue { get; }

    /// <summary>
    /// The textual definition of the type, e.g. an Oracle object type's specification or the name
    /// of the assembly implementing a <see cref="UserDefinedTypeKind.Clr"/> type.
    /// </summary>
    /// <value>A definition, if available.</value>
    Option<string> Definition { get; }
}
