﻿using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Represents a database column present on a table or view.
/// </summary>
public interface IDatabaseColumn
{
    /// <summary>
    /// The name of a column within a table or view.
    /// </summary>
    /// <value>A column name.</value>
    Identifier Name { get; }

    /// <summary>
    /// Determines whether a column can store <c>NULL</c> values.
    /// </summary>
    /// <value><c>true</c> if this column can store <c>null</c> values; otherwise, <c>false</c>.</value>
    bool IsNullable { get; }

    /// <summary>
    /// Determines whether the values of this column are generated by the database.
    /// </summary>
    /// <value><c>true</c> if the values within this column are computed by the database; otherwise, <c>false</c>.</value>
    bool IsComputed { get; }

    /// <summary>
    /// An expression that creates a default value for a column when omitted on an <c>INSERT</c> statement.
    /// </summary>
    /// <value>The default value for a column, if available.</value>
    Option<string> DefaultValue { get; }

    /// <summary>
    /// The database column data type.
    /// </summary>
    /// <value>A column data type.
    /// </value>
    IDbType Type { get; }

    /// <summary>
    /// Retrieves the auto-increment parameters applies to this column, if available.
    /// </summary>
    /// <value>An automatic increment definition, if available.
    /// </value>
    Option<IAutoIncrement> AutoIncrement { get; }
}