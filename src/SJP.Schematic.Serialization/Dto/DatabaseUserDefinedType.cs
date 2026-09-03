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
    public Core.UserDefinedTypeKind Kind { get; init; }

    /// <summary>
    /// The type this type is defined in terms of. Absent when it is not defined over another type.
    /// </summary>
    public DbType? BaseType { get; init; }

    /// <summary>
    /// The values a value of this type is restricted to. Empty when the type is not an enumeration.
    /// </summary>
    public IEnumerable<string> EnumValues { get; init; } = [];

    /// <summary>
    /// The named attributes that comprise the type. Empty when the type declares none.
    /// </summary>
    public IEnumerable<DatabaseColumn> Attributes { get; init; } = [];

    /// <summary>
    /// The check constraints a value of the type must satisfy. Empty when the type declares none.
    /// </summary>
    public IEnumerable<DatabaseCheckConstraint> Checks { get; init; } = [];

    /// <summary>
    /// Whether a value of the type can be <see langword="null" />.
    /// </summary>
    public bool IsNullable { get; init; }

    /// <summary>
    /// The default value declared for the type, if any.
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// The textual definition of the type, if the source database records one.
    /// </summary>
    public string? Definition { get; init; }
}
