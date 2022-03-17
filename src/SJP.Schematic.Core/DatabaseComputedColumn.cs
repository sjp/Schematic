﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using LanguageExt;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// A database column that stores information about a column whose values are generated by the database.
/// </summary>
/// <seealso cref="DatabaseColumn" />
/// <seealso cref="IDatabaseComputedColumn" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class DatabaseComputedColumn : DatabaseColumn, IDatabaseComputedColumn
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseComputedColumn"/> class.
    /// </summary>
    /// <param name="columnName">The name of the column. Only the local name is kept.</param>
    /// <param name="type">The column data type.</param>
    /// <param name="isNullable">Whether the column can hold <c>null</c> values.</param>
    /// <param name="defaultValue">The default value expression, if available.</param>
    /// <param name="definition">The computed column definition, if available.</param>
    /// <exception cref="ArgumentNullException"><paramref name="columnName"/> or <paramref name="type"/> is <c>null</c></exception>
    public DatabaseComputedColumn(
        Identifier columnName,
        IDbType type,
        bool isNullable,
        Option<string> defaultValue,
        Option<string> definition
    ) : base(columnName, type, isNullable, defaultValue, Option<IAutoIncrement>.None)
    {
        Definition = definition;
    }

    /// <summary>
    /// The definition of the computed column. Optional as some providers (e.g. Oracle) allow the definition to be missing.
    /// </summary>
    public Option<string> Definition { get; }

    /// <summary>
    /// Determines whether the values of this column are generated by the database.
    /// </summary>
    /// <value><c>true</c> if the values within this column are computed by the database; otherwise, <c>false</c>.</value>
    /// <remarks>Always <c>true</c> unless a derived type overrides this behaviour.</remarks>
    public override bool IsComputed { get; } = true;

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

            builder.Append("Computed Column: ")
                .Append(Name.LocalName);

            return builder.GetStringAndRelease();
        }
    }
}
