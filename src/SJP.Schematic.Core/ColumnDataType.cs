using System;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core;

/// <summary>
/// A definition of column data type information.
/// </summary>
/// <seealso cref="IDbType" />
public class ColumnDataType : IDbType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnDataType"/> class.
    /// </summary>
    /// <param name="typeName">A type name.</param>
    /// <param name="dataType">The data type.</param>
    /// <param name="definition">The type definition in string form.</param>
    /// <param name="clrType">The .NET data type that the column maps to.</param>
    /// <param name="isFixedLength">Whether the type is a fixed length, <c>true</c> if fixed length; otherwise <c>false</c>.</param>
    /// <param name="maxLength">The maximum length the column can store.</param>
    /// <param name="numericPrecision">The numeric precision.</param>
    /// <param name="collation">The collation.</param>
    /// <exception cref="ArgumentException"><paramref name="dataType"/> is not a valid enum.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <c>null</c>, empty or whitespace. Alternatively if <paramref name="typeName"/> or <paramref name="clrType"/> is <c>null</c>.</exception>
    public ColumnDataType(
        Identifier typeName,
        DataType dataType,
        string definition,
        Type clrType,
        bool isFixedLength,
        int maxLength,
        Option<INumericPrecision> numericPrecision,
        Option<Identifier> collation
    )
    {
        if (!dataType.IsValid())
            throw new ArgumentException($"The {nameof(DataType)} provided must be a valid enum.", nameof(dataType));
        if (definition.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(definition));

        TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
        DataType = dataType;
        Definition = definition;
        ClrType = clrType ?? throw new ArgumentNullException(nameof(clrType));
        IsFixedLength = isFixedLength;
        MaxLength = maxLength;
        NumericPrecision = numericPrecision;
        Collation = collation;
    }

    /// <summary>
    /// Gets the name of the column data type.
    /// </summary>
    /// <value>The name of the type.</value>
    public Identifier TypeName { get; }

    /// <summary>
    /// Gets the class of data type.
    /// </summary>
    /// <value>The data type.</value>
    public DataType DataType { get; }

    /// <summary>
    /// Gets the definition.
    /// </summary>
    /// <value>The definition.</value>
    public string Definition { get; }

    /// <summary>
    /// Gets a value indicating whether this data type has fixed length.
    /// </summary>
    /// <value><c>true</c> if this instance has a fixed length; otherwise, <c>false</c>.</value>
    public bool IsFixedLength { get; }

    /// <summary>
    /// The maximum length the column can hold.
    /// </summary>
    /// <value>The maximum length.</value>
    public int MaxLength { get; }

    /// <summary>
    /// The CLR data type used to store column data.
    /// </summary>
    /// <value>A CLR type.</value>
    public Type ClrType { get; }

    /// <summary>
    /// The numeric precision, if available.
    /// </summary>
    /// <value>The numeric precision.</value>
    public Option<INumericPrecision> NumericPrecision { get; }

    /// <summary>
    /// The collation, if available.
    /// </summary>
    /// <value>The collation.</value>
    public Option<Identifier> Collation { get; }
}