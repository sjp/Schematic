using System.Collections.Generic;

namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized type declared by a user within a database, e.g. a PostgreSQL enum or a SQL Server
/// table type.
/// </summary>
public sealed record DatabaseUserDefinedType
{
    /// <summary>
    /// The name of the type.
    /// </summary>
    public required Identifier TypeName { get; init; }

    /// <summary>
    /// The kind of type that was declared.
    /// </summary>
    public UserDefinedTypeKind Kind { get; init; }

    /// <summary>
    /// The type this type is defined in terms of: the aliased type of an alias or domain, the subtype
    /// of a range, or the element type of a collection. Absent for every other kind of type.
    /// </summary>
    public DbType? BaseType { get; init; }

    /// <summary>
    /// The values a value of this type is restricted to. Empty unless the type is an enumeration.
    /// </summary>
    public IEnumerable<string> EnumValues { get; init; } = [];

    /// <summary>
    /// The attributes of a composite type, or the columns of a table type. Empty for every other kind
    /// of type.
    /// </summary>
    public IEnumerable<DatabaseColumn> Attributes { get; init; } = [];

    /// <summary>
    /// The check constraints of a domain or table type. Empty for every other kind of type.
    /// </summary>
    public IEnumerable<DatabaseCheckConstraint> Checks { get; init; } = [];

    /// <summary>
    /// Whether a value of the type can be <see langword="null" />. Only an alias or domain declares a
    /// nullability rule; every other kind of type is written as nullable.
    /// </summary>
    public bool IsNullable { get; init; }

    /// <summary>
    /// The default value declared for an alias or domain, if any.
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// The textual definition of the type, if the source database records one, or the external name
    /// of a CLR type.
    /// </summary>
    public string? Definition { get; init; }
}
