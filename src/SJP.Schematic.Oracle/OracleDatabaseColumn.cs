using System;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
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
    /// <param name="isNullable">If set to <see langword="true" /> the column is nullable.</param>
    /// <param name="defaultValue">The default applied when an <c>INSERT</c> omits the column, if available.</param>
    /// <exception cref="ArgumentNullException"><paramref name="columnName"/> or <paramref name="type"/> is <see langword="null" />.</exception>
    public OracleDatabaseColumn(Identifier columnName, IDbType type, bool isNullable, Option<IDatabaseDefaultValue> defaultValue)
        : this(columnName, type, isNullable, defaultValue, Option<IAutoIncrement>.None)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OracleDatabaseColumn"/> class.
    /// </summary>
    /// <param name="columnName">A column name.</param>
    /// <param name="type">A column type.</param>
    /// <param name="isNullable">If set to <see langword="true" /> the column is nullable.</param>
    /// <param name="defaultValue">The default applied when an <c>INSERT</c> omits the column, if available.</param>
    /// <param name="autoIncrement">The identity definition applied to the column, if it has one.</param>
    /// <exception cref="ArgumentNullException"><paramref name="columnName"/> or <paramref name="type"/> is <see langword="null" />.</exception>
    public OracleDatabaseColumn(Identifier columnName, IDbType type, bool isNullable, Option<IDatabaseDefaultValue> defaultValue, Option<IAutoIncrement> autoIncrement)
        : this(columnName, type, isNullable, defaultValue, autoIncrement, false, Option<string>.None, ComputedColumnStorage.Unknown)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OracleDatabaseColumn"/> class.
    /// </summary>
    /// <param name="columnName">A column name.</param>
    /// <param name="type">A column type.</param>
    /// <param name="isNullable">If set to <see langword="true" /> the column is nullable.</param>
    /// <param name="defaultValue">The default applied when an <c>INSERT</c> omits the column, if available.</param>
    /// <param name="autoIncrement">The identity definition applied to the column, if it has one.</param>
    /// <param name="isComputed">Whether the values of the column are computed by the database.</param>
    /// <param name="computedDefinition">The expression that computes the column's values, if available.</param>
    /// <param name="computedStorage">Whether the computed values are stored or evaluated when read.</param>
    /// <exception cref="ArgumentNullException"><paramref name="columnName"/> or <paramref name="type"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="computedStorage"/> is not a valid enum value.</exception>
    public OracleDatabaseColumn(
        Identifier columnName,
        IDbType type,
        bool isNullable,
        Option<IDatabaseDefaultValue> defaultValue,
        Option<IAutoIncrement> autoIncrement,
        bool isComputed,
        Option<string> computedDefinition,
        ComputedColumnStorage computedStorage
    ) : this(columnName, type, isNullable, defaultValue, autoIncrement, isComputed, computedDefinition, computedStorage, false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OracleDatabaseColumn"/> class.
    /// </summary>
    /// <param name="columnName">A column name.</param>
    /// <param name="type">A column type.</param>
    /// <param name="isNullable">If set to <see langword="true" /> the column is nullable.</param>
    /// <param name="defaultValue">The default applied when an <c>INSERT</c> omits the column, if available.</param>
    /// <param name="autoIncrement">The identity definition applied to the column, if it has one.</param>
    /// <param name="isComputed">Whether the values of the column are computed by the database.</param>
    /// <param name="computedDefinition">The expression that computes the column's values, if available.</param>
    /// <param name="computedStorage">Whether the computed values are stored or evaluated when read.</param>
    /// <param name="isHidden">Whether the column was declared <c>INVISIBLE</c>, and is therefore omitted from the expansion of <c>SELECT *</c>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="columnName"/> or <paramref name="type"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="computedStorage"/> is not a valid enum value.</exception>
    public OracleDatabaseColumn(
        Identifier columnName,
        IDbType type,
        bool isNullable,
        Option<IDatabaseDefaultValue> defaultValue,
        Option<IAutoIncrement> autoIncrement,
        bool isComputed,
        Option<string> computedDefinition,
        ComputedColumnStorage computedStorage,
        bool isHidden
    )
    {
        ArgumentNullException.ThrowIfNull(columnName);
        if (!computedStorage.IsValid())
            throw new ArgumentException($"The {nameof(ComputedColumnStorage)} provided must be a valid enum.", nameof(computedStorage));

        Name = columnName.LocalName;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        IsNullable = isNullable;
        Default = defaultValue;
        AutoIncrement = autoIncrement;
        IsComputed = isComputed;
        ComputedDefinition = isComputed ? computedDefinition : Option<string>.None;
        ComputedStorage = isComputed ? computedStorage : ComputedColumnStorage.Unknown;
        IsHidden = isHidden;
    }

    /// <summary>
    /// The default applied to a column when omitted on an <c>INSERT</c> statement.
    /// </summary>
    /// <value>The default for a column, if available.</value>
    public Option<IDatabaseDefaultValue> Default { get; }

    /// <summary>
    /// An expression that creates a default value for a column when omitted on an <c>INSERT</c> statement.
    /// </summary>
    /// <value>The default value for a column, if available.</value>
    public Option<string> DefaultValue => Default.Map(static def => def.Definition);

    /// <summary>
    /// Determines whether the values of this column are generated by the database.
    /// </summary>
    /// <value><see langword="true" /> if the values within this column are computed by the database; otherwise, <see langword="false" />.</value>
    public bool IsComputed { get; }

    /// <summary>
    /// The expression that computes the values of this column. Optional as Oracle allows the definition to be missing.
    /// </summary>
    /// <value>A computed column definition, if available.</value>
    public Option<string> ComputedDefinition { get; }

    /// <summary>
    /// Determines whether the computed values of this column are stored in the table or evaluated when the column is read.
    /// </summary>
    /// <value>The storage applied to a computed column. Oracle virtual columns are never stored.</value>
    public ComputedColumnStorage ComputedStorage { get; }

    /// <summary>
    /// Determines whether the column was declared <c>INVISIBLE</c>, and is therefore omitted from the expansion of <c>SELECT *</c>.
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
    /// Retrieves the identity definition applied to this column, if it is an identity column.
    /// </summary>
    /// <value>An automatic increment definition, if available.</value>
    public Option<IAutoIncrement> AutoIncrement { get; }

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

            builder.Append(IsComputed ? "Computed Column: " : "Column: ")
                .Append(Name.LocalName);

            return builder.GetStringAndRelease();
        }
    }
}