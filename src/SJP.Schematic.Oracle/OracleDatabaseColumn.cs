﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Oracle;

/// <summary>
/// A database column specific to Oracle.
/// </summary>
/// <seealso cref="IDatabaseColumn" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class OracleDatabaseColumn : IDatabaseColumn
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OracleDatabaseColumn"/> class.
    /// </summary>
    /// <param name="columnName">A column name.</param>
    /// <param name="type">A column type.</param>
    /// <param name="isNullable">If set to <c>true</c> the column is nullable.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <exception cref="ArgumentNullException"><paramref name="columnName"/> or <paramref name="type"/> is <c>null</c>.</exception>
    public OracleDatabaseColumn(Identifier columnName, IDbType type, bool isNullable, Option<string> defaultValue)
    {
        if (columnName == null)
            throw new ArgumentNullException(nameof(columnName));

        Name = columnName.LocalName;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        IsNullable = isNullable;
        DefaultValue = defaultValue;
    }

    /// <summary>
    /// An expression that creates a default value for a column when omitted on an <c>INSERT</c> statement.
    /// </summary>
    /// <value>The default value for a column, if available.</value>
    public Option<string> DefaultValue { get; }

    /// <summary>
    /// Determines whether the values of this column are generated by the database.
    /// </summary>
    /// <value>Always <c>false</c> unless overridden.</value>
    public virtual bool IsComputed { get; }

    /// <summary>
    /// The name of a column within a table or view.
    /// </summary>
    /// <value>A column name.</value>
    public Identifier Name { get; }

    /// <summary>
    /// The database column data type.
    /// </summary>
    /// <value>A column data type.</value>
    public IDbType Type { get; }

    /// <summary>
    /// Determines whether a column can store <c>NULL</c> values.
    /// </summary>
    /// <value><c>true</c> if this column can store <c>null</c> values; otherwise, <c>false</c>.</value>
    public bool IsNullable { get; }

    /// <summary>
    /// Retrieves the auto-increment parameters applies to this column, always 'none'.
    /// </summary>
    /// <value>An automatic increment definition that is always 'none'.</value>
    public Option<IAutoIncrement> AutoIncrement { get; } = Option<IAutoIncrement>.None;

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

            builder.Append("Column: ")
                .Append(Name.LocalName);

            return builder.GetStringAndRelease();
        }
    }
}