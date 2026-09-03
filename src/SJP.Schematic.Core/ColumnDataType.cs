using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using LanguageExt;

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
    /// <param name="isFixedLength">Whether the type is a fixed length, <see langword="true" /> if fixed length; otherwise <see langword="false" />.</param>
    /// <param name="maxLength">The maximum length the column can store.</param>
    /// <param name="numericPrecision">The numeric precision.</param>
    /// <param name="collation">The collation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/>, <paramref name="definition"/> or <paramref name="clrType"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace, or <paramref name="dataType"/> is not a valid enum.</exception>
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
        : this(typeName, dataType, definition, clrType, isFixedLength, maxLength, numericPrecision, collation, Option<IDbType>.None, [], Option<IDbType>.None, false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnDataType"/> class.
    /// </summary>
    /// <param name="typeName">A type name.</param>
    /// <param name="dataType">The data type.</param>
    /// <param name="definition">The type definition in string form.</param>
    /// <param name="clrType">The .NET data type that the column maps to.</param>
    /// <param name="isFixedLength">Whether the type is a fixed length, <see langword="true" /> if fixed length; otherwise <see langword="false" />.</param>
    /// <param name="maxLength">The maximum length the column can store.</param>
    /// <param name="numericPrecision">The numeric precision.</param>
    /// <param name="collation">The collation.</param>
    /// <param name="elementType">The type of the elements of a collection type, if any.</param>
    /// <param name="enumValues">The values the type is restricted to, empty when it is not restricted.</param>
    /// <param name="baseType">The type that this type is defined in terms of, if any.</param>
    /// <param name="isUnsigned">Whether the type stores only non-negative values.</param>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/>, <paramref name="definition"/>, <paramref name="clrType"/> or <paramref name="enumValues"/> is <see langword="null" />, or <paramref name="enumValues"/> contains a <see langword="null" /> value.</exception>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace, or <paramref name="dataType"/> is not a valid enum.</exception>
    public ColumnDataType(
        Identifier typeName,
        DataType dataType,
        string definition,
        Type clrType,
        bool isFixedLength,
        int maxLength,
        Option<INumericPrecision> numericPrecision,
        Option<Identifier> collation,
        Option<IDbType> elementType,
        IReadOnlyList<string> enumValues,
        Option<IDbType> baseType,
        bool isUnsigned
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definition);
        ArgumentNullException.ThrowIfNull(enumValues);
        if (enumValues.Any(static v => v == null))
            throw new ArgumentNullException(nameof(enumValues), "An enum value was null. A null enum value is not supported.");

        if (!dataType.IsValid())
            throw new ArgumentException($"The {nameof(DataType)} provided must be a valid enum.", nameof(dataType));

        TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
        DataType = dataType;
        Definition = definition;
        ClrType = clrType ?? throw new ArgumentNullException(nameof(clrType));
        IsFixedLength = isFixedLength;
        MaxLength = maxLength;
        NumericPrecision = numericPrecision;
        Collation = collation;
        ElementType = elementType;
        EnumValues = enumValues;
        BaseType = baseType;
        IsUnsigned = isUnsigned;
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
    /// <value><see langword="true" /> if this instance has a fixed length; otherwise, <see langword="false" />.</value>
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

    /// <summary>
    /// The type of the elements stored by a collection type, if available.
    /// </summary>
    /// <value>The element type, for a <see cref="Core.DataType.Array"/> or <see cref="Core.DataType.Range"/> type; otherwise none.</value>
    public Option<IDbType> ElementType { get; }

    /// <summary>
    /// The values a value of this type is restricted to.
    /// </summary>
    /// <value>The permitted values, for a <see cref="Core.DataType.Enum"/> or <see cref="Core.DataType.Set"/> type; otherwise empty.</value>
    public IReadOnlyList<string> EnumValues { get; }

    /// <summary>
    /// The type that this type is defined in terms of, if available.
    /// </summary>
    /// <value>The base type, for a domain or alias type; otherwise none.</value>
    public Option<IDbType> BaseType { get; }

    /// <summary>
    /// Gets a value indicating whether the type stores only non-negative values.
    /// </summary>
    /// <value><see langword="true" /> if this instance is unsigned; otherwise, <see langword="false" />.</value>
    public bool IsUnsigned { get; }
}