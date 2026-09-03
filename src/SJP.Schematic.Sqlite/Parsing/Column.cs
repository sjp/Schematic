using System;
using EnumsNET;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Parsing;

/// <summary>
/// The parsed definition of a table column in a SQLite <c>CREATE TABLE</c> definition.
/// </summary>
public class Column
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Column"/> class.
    /// </summary>
    /// <param name="columnName">The column name.</param>
    /// <param name="typeDefinition">The type definition.</param>
    /// <param name="nullable">If set to <see langword="true" /> indicates the column is nullable.</param>
    /// <param name="autoIncrement">If set to <see langword="true" /> the column automatically increments.</param>
    /// <param name="collation">The column collation.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <param name="defaultValueKind">What the default value expression evaluates to.</param>
    /// <param name="computedDefinition">The computed definition.</param>
    /// <param name="computedColumnType">The computed column type.</param>
    /// <exception cref="ArgumentNullException"><paramref name="columnName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="columnName"/> is empty or whitespace, or <paramref name="collation"/>, <paramref name="defaultValueKind"/> or <paramref name="computedColumnType"/> is not a valid enum.</exception>
    public Column(
        string columnName,
        string typeDefinition,
        bool nullable,
        bool autoIncrement,
        SqliteCollation collation,
        string defaultValue,
        DefaultValueKind defaultValueKind,
        string computedDefinition,
        SqliteGeneratedColumnType computedColumnType
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);
        if (!collation.IsValid())
            throw new ArgumentException($"The {nameof(SqliteCollation)} provided must be a valid enum.", nameof(collation));
        if (!defaultValueKind.IsValid())
            throw new ArgumentException($"The {nameof(DefaultValueKind)} provided must be a valid enum.", nameof(defaultValueKind));
        if (!computedColumnType.IsValid())
            throw new ArgumentException($"The {nameof(SqliteGeneratedColumnType)} provided must be a valid enum.", nameof(computedColumnType));

        Name = columnName;
        TypeDefinition = typeDefinition ?? string.Empty;
        Nullable = nullable;
        IsAutoIncrement = autoIncrement;
        Collation = collation;
        DefaultValue = defaultValue ?? string.Empty;
        DefaultValueKind = defaultValueKind;
        ComputedDefinition = computedDefinition ?? string.Empty;
        ComputedColumnType = computedColumnType;
    }

    /// <summary>
    /// The column name.
    /// </summary>
    /// <value>A column name.</value>
    public string Name { get; }

    /// <summary>
    /// A type definition for the column type.
    /// </summary>
    /// <value>The column type definition.</value>
    public string TypeDefinition { get; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="Column"/> is nullable.
    /// </summary>
    /// <value><see langword="true" /> if nullable; otherwise, <see langword="false" />.</value>
    public bool Nullable { get; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="Column"/> has an automatic increment applied to it.
    /// </summary>
    /// <value><see langword="true" /> if this instance is automatically incrementing; otherwise, <see langword="false" />.</value>
    public bool IsAutoIncrement { get; }

    /// <summary>
    /// The collation used to compare column values.
    /// </summary>
    /// <value>The column collation.
    /// </value>
    public SqliteCollation Collation { get; }

    /// <summary>
    /// Gets the default value.
    /// </summary>
    /// <value>The default value definition.</value>
    public string DefaultValue { get; }

    /// <summary>
    /// Describes what the default value expression evaluates to, taken from the token the parser
    /// matched after <c>DEFAULT</c>.
    /// </summary>
    /// <value>A default value classification, or <see cref="Core.DefaultValueKind.Unknown"/> when the column has no default.</value>
    public DefaultValueKind DefaultValueKind { get; }

    /// <summary>
    /// Gets the computed definition.
    /// </summary>
    /// <value>The computed column definition.</value>
    public string ComputedDefinition { get; }

    /// <summary>
    /// Determines how the column value is stored.
    /// </summary>
    /// <value>A value which indicates how the value of the column is generated.</value>
    public SqliteGeneratedColumnType ComputedColumnType { get; }
}
