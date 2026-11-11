using System;
using System.ComponentModel;
using System.Diagnostics;
using LanguageExt;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// A database column that stores information about a column in a table or a view.
/// </summary>
/// <seealso cref="IDatabaseColumn" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class DatabaseColumn : IDatabaseColumn
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseColumn"/> class.
    /// </summary>
    /// <param name="columnName">The name of the column. Only the local name is kept.</param>
    /// <param name="type">The column data type.</param>
    /// <param name="isNullable">Whether the column can hold <see langword="null" /> values.</param>
    /// <param name="defaultValue">The default applied when an <c>INSERT</c> omits the column, if available.</param>
    /// <param name="valueSource">Where the values of the column come from.</param>
    /// <exception cref="ArgumentNullException"><paramref name="columnName"/> or <paramref name="type"/> is <see langword="null" />, <paramref name="defaultValue"/> is <c>Some</c> of a <see langword="default"/> <see cref="DatabaseDefaultValue"/>, or <paramref name="valueSource"/> is a <see langword="default"/> <see cref="ColumnValueSource"/>.</exception>
    public DatabaseColumn(
        Identifier columnName,
        IDbType type,
        bool isNullable,
        Option<DatabaseDefaultValue> defaultValue,
        ColumnValueSource valueSource
    ) : this(columnName, type, isNullable, defaultValue, valueSource, false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseColumn"/> class.
    /// </summary>
    /// <param name="columnName">The name of the column. Only the local name is kept.</param>
    /// <param name="type">The column data type.</param>
    /// <param name="isNullable">Whether the column can hold <see langword="null" /> values.</param>
    /// <param name="defaultValue">The default applied when an <c>INSERT</c> omits the column, if available.</param>
    /// <param name="valueSource">Where the values of the column come from.</param>
    /// <param name="isHidden">Whether the column is omitted from the expansion of <c>SELECT *</c>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="columnName"/> or <paramref name="type"/> is <see langword="null" />, <paramref name="defaultValue"/> is <c>Some</c> of a <see langword="default"/> <see cref="DatabaseDefaultValue"/>, or <paramref name="valueSource"/> is a <see langword="default"/> <see cref="ColumnValueSource"/>.</exception>
    public DatabaseColumn(
        Identifier columnName,
        IDbType type,
        bool isNullable,
        Option<DatabaseDefaultValue> defaultValue,
        ColumnValueSource valueSource,
        bool isHidden
    )
    {
        ArgumentNullException.ThrowIfNull(columnName);
        if (defaultValue.Exists(static def => def.Value is null))
            throw new ArgumentNullException(nameof(defaultValue), "The default value provided holds no default.");
        if (valueSource.Value is null)
            throw new ArgumentNullException(nameof(valueSource), "The value source provided holds no value source.");

        Name = LocalIdentifier.From(columnName);
        Type = type ?? throw new ArgumentNullException(nameof(type));
        IsNullable = isNullable;
        Default = defaultValue;
        ValueSource = valueSource;
        IsHidden = isHidden;
    }

    /// <summary>
    /// The default applied to a column when omitted on an <c>INSERT</c> statement.
    /// </summary>
    /// <value>The default for a column, if available.</value>
    public Option<DatabaseDefaultValue> Default { get; }

    /// <summary>
    /// An expression that creates a default value for a column when omitted on an <c>INSERT</c> statement.
    /// </summary>
    /// <value>The default value for a column, if available.</value>
    public Option<string> DefaultValue => Default.Map(static def => def.Definition);

    /// <summary>
    /// Where the values of this column come from.
    /// </summary>
    /// <value>The source of the column's values.</value>
    public ColumnValueSource ValueSource { get; }

    /// <summary>
    /// Determines whether the column is omitted from the expansion of <c>SELECT *</c>.
    /// </summary>
    /// <value><see langword="true" /> if this column is hidden from <c>SELECT *</c>; otherwise, <see langword="false" />.</value>
    public bool IsHidden { get; }

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
    /// Determines whether a column can store <see langword="null" /> values.
    /// </summary>
    /// <value><see langword="true" /> if this column can store <see langword="null" /> values; otherwise, <see langword="false" />.</value>
    public bool IsNullable { get; }

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

            builder.Append(ValueSource is ColumnValueSource.Computed ? "Computed Column: " : "Column: ")
                .Append(Name.LocalName);

            return builder.GetStringAndRelease();
        }
    }
}