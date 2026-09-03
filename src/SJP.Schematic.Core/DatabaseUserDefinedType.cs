using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// Contains information about a type declared by a user within a database.
/// </summary>
/// <seealso cref="IDatabaseUserDefinedType" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class DatabaseUserDefinedType : IDatabaseUserDefinedType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseUserDefinedType"/> class, describing a
    /// type that declares no attributes, values or constraints.
    /// </summary>
    /// <param name="typeName">A type name.</param>
    /// <param name="kind">The kind of type.</param>
    /// <param name="baseType">The type that this type is defined in terms of, if available.</param>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="kind"/> is not a valid enum value.</exception>
    public DatabaseUserDefinedType(Identifier typeName, UserDefinedTypeKind kind, Option<IDbType> baseType)
        : this(typeName, kind, baseType, [], [], [], true, Option<string>.None, Option<string>.None)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseUserDefinedType"/> class.
    /// </summary>
    /// <param name="typeName">A type name.</param>
    /// <param name="kind">The kind of type.</param>
    /// <param name="baseType">The type that this type is defined in terms of, if available.</param>
    /// <param name="enumValues">The values a value of this type is restricted to, empty when the type is not an enumeration.</param>
    /// <param name="attributes">The named attributes that comprise this type, empty when the type declares none.</param>
    /// <param name="checks">The check constraints a value of this type must satisfy, empty when the type declares none.</param>
    /// <param name="isNullable">Whether a value of this type can be <see langword="null" />.</param>
    /// <param name="defaultValue">A default value for the type, if available.</param>
    /// <param name="definition">A textual definition of the type, if available.</param>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/>, <paramref name="enumValues"/>, <paramref name="attributes"/> or <paramref name="checks"/> is <see langword="null" />, or one of the collections contains a <see langword="null" /> element.</exception>
    /// <exception cref="ArgumentException"><paramref name="kind"/> is not a valid enum value.</exception>
    public DatabaseUserDefinedType(
        Identifier typeName,
        UserDefinedTypeKind kind,
        Option<IDbType> baseType,
        IReadOnlyList<string> enumValues,
        IReadOnlyList<IDatabaseColumn> attributes,
        IReadOnlyCollection<IDatabaseCheckConstraint> checks,
        bool isNullable,
        Option<string> defaultValue,
        Option<string> definition
    )
    {
        if (!kind.IsValid())
            throw new ArgumentException($"The {nameof(UserDefinedTypeKind)} provided must be a valid enum.", nameof(kind));
        if (enumValues.NullOrAnyNull())
            throw new ArgumentNullException(nameof(enumValues));
        if (attributes.NullOrAnyNull())
            throw new ArgumentNullException(nameof(attributes));
        if (checks.NullOrAnyNull())
            throw new ArgumentNullException(nameof(checks));

        Name = typeName ?? throw new ArgumentNullException(nameof(typeName));
        Kind = kind;
        BaseType = baseType;
        EnumValues = enumValues;
        Attributes = attributes;
        Checks = checks;
        IsNullable = isNullable;
        DefaultValue = defaultValue;
        Definition = definition;
    }

    /// <summary>
    /// The name of the user-defined type.
    /// </summary>
    public Identifier Name { get; }

    /// <summary>
    /// The kind of type that has been declared.
    /// </summary>
    /// <value>A user-defined type kind.</value>
    public UserDefinedTypeKind Kind { get; }

    /// <summary>
    /// The type that this type is defined in terms of, if available.
    /// </summary>
    /// <value>A base type, if available.</value>
    public Option<IDbType> BaseType { get; }

    /// <summary>
    /// The values that a value of this type is restricted to.
    /// </summary>
    /// <value>The permitted values, empty when the type is not an enumeration.</value>
    public IReadOnlyList<string> EnumValues { get; }

    /// <summary>
    /// The named attributes that comprise this type.
    /// </summary>
    /// <value>A collection of attributes, empty when the type declares none.</value>
    public IReadOnlyList<IDatabaseColumn> Attributes { get; }

    /// <summary>
    /// The check constraints that a value of this type must satisfy.
    /// </summary>
    /// <value>A collection of check constraints, empty when the type declares none.</value>
    public IReadOnlyCollection<IDatabaseCheckConstraint> Checks { get; }

    /// <summary>
    /// Determines whether a value of this type can be <see langword="null" />.
    /// </summary>
    /// <value><see langword="true" /> if a value of this type can be <see langword="null" />; otherwise, <see langword="false" />.</value>
    public bool IsNullable { get; }

    /// <summary>
    /// An expression that creates a default value for a column of this type when one is omitted.
    /// </summary>
    /// <value>The default value for the type, if available.</value>
    public Option<string> DefaultValue { get; }

    /// <summary>
    /// The textual definition of the type, if available.
    /// </summary>
    /// <value>A definition, if available.</value>
    public Option<string> Definition { get; }

    /// <summary>
    /// Returns a string that provides a basic string representation of this object.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => DebuggerDisplay;

    private string DebuggerDisplay
    {
        get
        {
            var builder = StringBuilderCache.Acquire();

            builder.Append("Type: ");

            if (!Name.Schema.IsNullOrWhiteSpace())
                builder.Append(Name.Schema).Append('.');

            builder.Append(Name.LocalName);

            return builder.GetStringAndRelease();
        }
    }
}
