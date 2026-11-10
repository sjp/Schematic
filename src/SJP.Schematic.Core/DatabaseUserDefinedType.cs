using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using LanguageExt;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// A type declared by a user within a database, e.g. a PostgreSQL enum or a SQL Server table type.
/// </summary>
/// <remarks>
/// <para>
/// Every kind of type has a <see cref="Name"/>, which is available directly. Switch on the type to
/// reach what only that kind of type is built from, such as the <see cref="Enum.Values"/> of an
/// enumeration or the <see cref="Range.Subtype"/> of a range.
/// </para>
/// <para>
/// A <see langword="default"/> value holds no type, and every member throws when read from it.
/// </para>
/// </remarks>
/// <seealso cref="IDatabaseEntity" />
[DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
public union DatabaseUserDefinedType(
    DatabaseUserDefinedType.Unknown,
    DatabaseUserDefinedType.Alias,
    DatabaseUserDefinedType.Domain,
    DatabaseUserDefinedType.Enum,
    DatabaseUserDefinedType.Composite,
    DatabaseUserDefinedType.Range,
    DatabaseUserDefinedType.Table,
    DatabaseUserDefinedType.Clr,
    DatabaseUserDefinedType.Collection
) : IDatabaseEntity
{
    /// <summary>
    /// The name of the user-defined type.
    /// </summary>
    /// <value>A type name.</value>
    public Identifier Name => this switch
    {
        Unknown type => type.Name,
        Alias type => type.Name,
        Domain type => type.Name,
        Enum type => type.Name,
        Composite type => type.Name,
        Range type => type.Name,
        Table type => type.Name,
        Clr type => type.Name,
        Collection type => type.Name,
    };

    /// <summary>
    /// Returns a string that provides a basic string representation of this object.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => Value?.ToString() ?? string.Empty;

    /// <summary>
    /// A type whose kind the dialect could not determine.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    public sealed class Unknown : IDatabaseEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Unknown"/> class.
        /// </summary>
        /// <param name="name">A type name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null" />.</exception>
        public Unknown(Identifier name)
            : this(name, Option<string>.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Unknown"/> class.
        /// </summary>
        /// <param name="name">A type name.</param>
        /// <param name="definition">The textual definition of the type, if the database records one.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null" />.</exception>
        public Unknown(Identifier name, Option<string> definition)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Definition = definition;
        }

        /// <summary>
        /// The name of the user-defined type.
        /// </summary>
        /// <value>A type name.</value>
        public Identifier Name { get; }

        /// <summary>
        /// The textual definition of the type, e.g. an Oracle type's specification.
        /// </summary>
        /// <value>A definition, if the database records one.</value>
        public Option<string> Definition { get; }

        /// <summary>
        /// Returns a string that provides a basic string representation of this object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => UserDefinedTypeArguments.Describe(Name);
    }

    /// <summary>
    /// An alias for a built-in type, e.g. a SQL Server type created by <c>create type ... from</c>.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    public sealed class Alias : IDatabaseEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Alias"/> class, describing a nullable alias
        /// with no default.
        /// </summary>
        /// <param name="name">A type name.</param>
        /// <param name="aliasedType">The type that this type is an alias for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="aliasedType"/> is <see langword="null" />.</exception>
        public Alias(Identifier name, IDbType aliasedType)
            : this(name, aliasedType, true, Option<string>.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Alias"/> class.
        /// </summary>
        /// <param name="name">A type name.</param>
        /// <param name="aliasedType">The type that this type is an alias for.</param>
        /// <param name="isNullable">Whether a value of this type can be <see langword="null" />.</param>
        /// <param name="defaultValue">A default value bound to the type, if any.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="aliasedType"/> is <see langword="null" />.</exception>
        public Alias(Identifier name, IDbType aliasedType, bool isNullable, Option<string> defaultValue)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            AliasedType = aliasedType ?? throw new ArgumentNullException(nameof(aliasedType));
            IsNullable = isNullable;
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// The name of the user-defined type.
        /// </summary>
        /// <value>A type name.</value>
        public Identifier Name { get; }

        /// <summary>
        /// The type that this type is an alias for.
        /// </summary>
        /// <value>A database type.</value>
        public IDbType AliasedType { get; }

        /// <summary>
        /// Determines whether a value of this type can be <see langword="null" />.
        /// </summary>
        /// <value><see langword="true" /> if a value of this type can be <see langword="null" />; otherwise, <see langword="false" />.</value>
        public bool IsNullable { get; }

        /// <summary>
        /// An expression that creates a default value for a column of this type when one is omitted.
        /// </summary>
        /// <value>The default value for the type, if one is bound to it.</value>
        public Option<string> DefaultValue { get; }

        /// <summary>
        /// Returns a string that provides a basic string representation of this object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => UserDefinedTypeArguments.Describe(Name);
    }

    /// <summary>
    /// A built-in type constrained by check constraints, a nullability rule and a default, e.g. a
    /// PostgreSQL domain.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    public sealed class Domain : IDatabaseEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Domain"/> class, describing a nullable domain
        /// with no default and no check constraints.
        /// </summary>
        /// <param name="name">A type name.</param>
        /// <param name="aliasedType">The type that the domain constrains.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="aliasedType"/> is <see langword="null" />.</exception>
        public Domain(Identifier name, IDbType aliasedType)
            : this(name, aliasedType, true, Option<string>.None, [])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Domain"/> class.
        /// </summary>
        /// <param name="name">A type name.</param>
        /// <param name="aliasedType">The type that the domain constrains.</param>
        /// <param name="isNullable">Whether a value of this type can be <see langword="null" />.</param>
        /// <param name="defaultValue">A default value declared for the type, if any.</param>
        /// <param name="checks">The check constraints a value of this type must satisfy, empty when the domain declares none.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/>, <paramref name="aliasedType"/> or <paramref name="checks"/> is <see langword="null" />, or <paramref name="checks"/> contains a <see langword="null" /> element.</exception>
        public Domain(
            Identifier name,
            IDbType aliasedType,
            bool isNullable,
            Option<string> defaultValue,
            IReadOnlyCollection<IDatabaseCheckConstraint> checks
        )
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            AliasedType = aliasedType ?? throw new ArgumentNullException(nameof(aliasedType));
            Checks = checks.ToDefensiveCopy(nameof(checks));
            IsNullable = isNullable;
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// The name of the user-defined type.
        /// </summary>
        /// <value>A type name.</value>
        public Identifier Name { get; }

        /// <summary>
        /// The type that the domain constrains.
        /// </summary>
        /// <value>A database type.</value>
        public IDbType AliasedType { get; }

        /// <summary>
        /// Determines whether a value of this type can be <see langword="null" />.
        /// </summary>
        /// <value><see langword="true" /> if a value of this type can be <see langword="null" />; otherwise, <see langword="false" />.</value>
        public bool IsNullable { get; }

        /// <summary>
        /// An expression that creates a default value for a column of this type when one is omitted.
        /// </summary>
        /// <value>The default value for the type, if one is declared.</value>
        public Option<string> DefaultValue { get; }

        /// <summary>
        /// The check constraints that a value of this type must satisfy.
        /// </summary>
        /// <value>A collection of check constraints, empty when the domain declares none.</value>
        public IReadOnlyCollection<IDatabaseCheckConstraint> Checks { get; }

        /// <summary>
        /// Returns a string that provides a basic string representation of this object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => UserDefinedTypeArguments.Describe(Name);
    }

    /// <summary>
    /// A type whose values are restricted to a named set of labels, e.g. a PostgreSQL enum.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    public sealed class Enum : IDatabaseEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Enum"/> class.
        /// </summary>
        /// <param name="name">A type name.</param>
        /// <param name="values">The values a value of this type is restricted to, in their declared order.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="values"/> is <see langword="null" />, or <paramref name="values"/> contains a <see langword="null" /> element.</exception>
        public Enum(Identifier name, IReadOnlyList<string> values)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Values = values.ToDefensiveCopy(nameof(values));
        }

        /// <summary>
        /// The name of the user-defined type.
        /// </summary>
        /// <value>A type name.</value>
        public Identifier Name { get; }

        /// <summary>
        /// The values that a value of this type is restricted to, in their declared order.
        /// </summary>
        /// <value>The permitted values.</value>
        public IReadOnlyList<string> Values { get; }

        /// <summary>
        /// Returns a string that provides a basic string representation of this object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => UserDefinedTypeArguments.Describe(Name);
    }

    /// <summary>
    /// A type composed of named attributes, e.g. a PostgreSQL composite type or an Oracle object type.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    public sealed class Composite : IDatabaseEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Composite"/> class.
        /// </summary>
        /// <param name="name">A type name.</param>
        /// <param name="attributes">The named attributes that comprise this type.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="attributes"/> is <see langword="null" />, or <paramref name="attributes"/> contains a <see langword="null" /> element.</exception>
        public Composite(Identifier name, IReadOnlyList<IDatabaseColumn> attributes)
            : this(name, attributes, Option<string>.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Composite"/> class.
        /// </summary>
        /// <param name="name">A type name.</param>
        /// <param name="attributes">The named attributes that comprise this type.</param>
        /// <param name="definition">The textual definition of the type, if the database records one.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="attributes"/> is <see langword="null" />, or <paramref name="attributes"/> contains a <see langword="null" /> element.</exception>
        public Composite(Identifier name, IReadOnlyList<IDatabaseColumn> attributes, Option<string> definition)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Attributes = attributes.ToDefensiveCopy(nameof(attributes));
            Definition = definition;
        }

        /// <summary>
        /// The name of the user-defined type.
        /// </summary>
        /// <value>A type name.</value>
        public Identifier Name { get; }

        /// <summary>
        /// The named attributes that comprise this type.
        /// </summary>
        /// <value>A collection of attributes.</value>
        public IReadOnlyList<IDatabaseColumn> Attributes { get; }

        /// <summary>
        /// The textual definition of the type, e.g. an Oracle object type's specification.
        /// </summary>
        /// <value>A definition, if the database records one.</value>
        public Option<string> Definition { get; }

        /// <summary>
        /// Returns a string that provides a basic string representation of this object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => UserDefinedTypeArguments.Describe(Name);
    }

    /// <summary>
    /// A type describing a range of values of a subtype, e.g. a PostgreSQL range type.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    public sealed class Range : IDatabaseEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Range"/> class.
        /// </summary>
        /// <param name="name">A type name.</param>
        /// <param name="subtype">The type of the values that the range spans.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="subtype"/> is <see langword="null" />.</exception>
        public Range(Identifier name, IDbType subtype)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Subtype = subtype ?? throw new ArgumentNullException(nameof(subtype));
        }

        /// <summary>
        /// The name of the user-defined type.
        /// </summary>
        /// <value>A type name.</value>
        public Identifier Name { get; }

        /// <summary>
        /// The type of the values that the range spans.
        /// </summary>
        /// <value>A database type.</value>
        public IDbType Subtype { get; }

        /// <summary>
        /// Returns a string that provides a basic string representation of this object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => UserDefinedTypeArguments.Describe(Name);
    }

    /// <summary>
    /// A type describing a table, e.g. a SQL Server table type used for table-valued parameters.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    public sealed class Table : IDatabaseEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class, describing a table type with
        /// no check constraints.
        /// </summary>
        /// <param name="name">A type name.</param>
        /// <param name="columns">The columns of the table that this type describes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="columns"/> is <see langword="null" />, or <paramref name="columns"/> contains a <see langword="null" /> element.</exception>
        public Table(Identifier name, IReadOnlyList<IDatabaseColumn> columns)
            : this(name, columns, [])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class.
        /// </summary>
        /// <param name="name">A type name.</param>
        /// <param name="columns">The columns of the table that this type describes.</param>
        /// <param name="checks">The check constraints declared on the table, empty when it declares none.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/>, <paramref name="columns"/> or <paramref name="checks"/> is <see langword="null" />, or one of the collections contains a <see langword="null" /> element.</exception>
        public Table(Identifier name, IReadOnlyList<IDatabaseColumn> columns, IReadOnlyCollection<IDatabaseCheckConstraint> checks)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Columns = columns.ToDefensiveCopy(nameof(columns));
            Checks = checks.ToDefensiveCopy(nameof(checks));
        }

        /// <summary>
        /// The name of the user-defined type.
        /// </summary>
        /// <value>A type name.</value>
        public Identifier Name { get; }

        /// <summary>
        /// The columns of the table that this type describes.
        /// </summary>
        /// <value>A collection of columns.</value>
        public IReadOnlyList<IDatabaseColumn> Columns { get; }

        /// <summary>
        /// The check constraints declared on the table that this type describes.
        /// </summary>
        /// <value>A collection of check constraints, empty when the table declares none.</value>
        public IReadOnlyCollection<IDatabaseCheckConstraint> Checks { get; }

        /// <summary>
        /// Returns a string that provides a basic string representation of this object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => UserDefinedTypeArguments.Describe(Name);
    }

    /// <summary>
    /// A type implemented by managed code hosted in the database, e.g. a SQL Server assembly type.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    public sealed class Clr : IDatabaseEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Clr"/> class, whose implementing code is not
        /// reported.
        /// </summary>
        /// <param name="name">A type name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null" />.</exception>
        public Clr(Identifier name)
            : this(name, Option<string>.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Clr"/> class.
        /// </summary>
        /// <param name="name">A type name.</param>
        /// <param name="externalName">The assembly and class that implement the type, if the database reports them.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null" />.</exception>
        public Clr(Identifier name, Option<string> externalName)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ExternalName = externalName;
        }

        /// <summary>
        /// The name of the user-defined type.
        /// </summary>
        /// <value>A type name.</value>
        public Identifier Name { get; }

        /// <summary>
        /// The assembly and class that implement the type, as SQL Server's
        /// <c>create type ... external name</c> names them, e.g. <c>MyAssembly.MyNamespace.MyType</c>.
        /// </summary>
        /// <value>An assembly-qualified class name, if the database reports one.</value>
        public Option<string> ExternalName { get; }

        /// <summary>
        /// Returns a string that provides a basic string representation of this object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => UserDefinedTypeArguments.Describe(Name);
    }

    /// <summary>
    /// A type describing a collection of values of an element type, e.g. an Oracle varray or nested table.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    public sealed class Collection : IDatabaseEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Collection"/> class.
        /// </summary>
        /// <param name="name">A type name.</param>
        /// <param name="elementType">The type of the values that the collection holds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="elementType"/> is <see langword="null" />.</exception>
        public Collection(Identifier name, IDbType elementType)
            : this(name, elementType, Option<string>.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Collection"/> class.
        /// </summary>
        /// <param name="name">A type name.</param>
        /// <param name="elementType">The type of the values that the collection holds.</param>
        /// <param name="definition">The textual definition of the type, if the database records one.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="elementType"/> is <see langword="null" />.</exception>
        public Collection(Identifier name, IDbType elementType, Option<string> definition)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
            Definition = definition;
        }

        /// <summary>
        /// The name of the user-defined type.
        /// </summary>
        /// <value>A type name.</value>
        public Identifier Name { get; }

        /// <summary>
        /// The type of the values that the collection holds.
        /// </summary>
        /// <value>A database type.</value>
        public IDbType ElementType { get; }

        /// <summary>
        /// The textual definition of the type, e.g. an Oracle collection type's specification.
        /// </summary>
        /// <value>A definition, if the database records one.</value>
        public Option<string> Definition { get; }

        /// <summary>
        /// Returns a string that provides a basic string representation of this object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => UserDefinedTypeArguments.Describe(Name);
    }
}

/// <summary>
/// Argument handling shared by the user-defined type cases.
/// </summary>
file static class UserDefinedTypeArguments
{
    public static string Describe(Identifier name)
    {
        var builder = StringBuilderCache.Acquire();

        builder.Append("Type: ");

        if (!name.Schema.IsNullOrWhiteSpace())
            builder.Append(name.Schema).Append('.');

        builder.Append(name.LocalName);

        return builder.GetStringAndRelease();
    }
}
