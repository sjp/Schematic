using System;
using System.Collections.Generic;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql;

/// <summary>
/// Builds column type metadata out of the type information the catalog reports for a column.
/// </summary>
/// <remarks>
/// <c>information_schema</c> reports a column's type as one of the built-in type names, or as
/// <c>ARRAY</c> or <c>USER-DEFINED</c> when the type is not one of them. Neither of the latter two
/// names a type, so the type is identified by <c>udt_name</c> instead and classified by the
/// <c>pg_type.typtype</c> of that type. The same shape of information is reported for a table, a
/// view and a materialized view, so all of them are described here.
/// </remarks>
internal static class PostgreSqlColumnTypeMetadata
{
    private const string PgCatalog = "pg_catalog";
    private const string ArrayDataType = "ARRAY";
    private const string UserDefinedDataType = "USER-DEFINED";

    private const string EnumTypeKind = "e";
    private const string CompositeTypeKind = "c";
    private const string RangeTypeKind = "r";
    private const string MultirangeTypeKind = "m";

    /// <summary>
    /// Describes a column's type, resolving the array, enum, composite, range and domain types that
    /// <c>information_schema.data_type</c> does not name.
    /// </summary>
    /// <param name="typeProvider">The type provider used to describe an element or base type.</param>
    /// <param name="source">The type information the catalog reported for the column.</param>
    /// <param name="collation">The column's collation, if any.</param>
    /// <param name="maxLength">The column's maximum length.</param>
    /// <param name="numericPrecision">The column's numeric precision, if any.</param>
    /// <param name="fractionalSecondsPrecision">The column's fractional seconds precision, for a temporal type; otherwise none.</param>
    /// <returns>Column type metadata.</returns>
    public static ColumnTypeMetadata Create(
        IDbTypeProvider typeProvider,
        CatalogTypeInfo source,
        Option<Identifier> collation,
        int maxLength,
        Option<INumericPrecision> numericPrecision,
        Option<int> fractionalSecondsPrecision
    )
    {
        ArgumentNullException.ThrowIfNull(typeProvider);

        var isArray = string.Equals(source.DataType, ArrayDataType, StringComparison.OrdinalIgnoreCase);
        var isUserDefined = string.Equals(source.DataType, UserDefinedDataType, StringComparison.OrdinalIgnoreCase);
        var enumValues = source.EnumLabels ?? [];

        var metadata = new ColumnTypeMetadata
        {
            Collation = collation,
            MaxLength = maxLength,
            NumericPrecision = numericPrecision,
            FractionalSecondsPrecision = fractionalSecondsPrecision,
        };

        if (isArray)
        {
            metadata.TypeName = GetUserTypeName(source);
            metadata.DataType = DataType.Array;
            metadata.ElementType = CreateElementType(typeProvider, source, enumValues);
            return metadata;
        }

        if (isUserDefined)
        {
            metadata.TypeName = GetUserTypeName(source);
            metadata.DataType = GetUserDefinedDataType(source.TypeKind);
            if (metadata.DataType == DataType.Enum)
                metadata.EnumValues = enumValues;
            return metadata;
        }

        // a domain is reported by the type it is defined over, so the domain names the column's type
        // while the type reported for it becomes the base type
        if (!source.DomainName.IsNullOrWhiteSpace())
        {
            metadata.TypeName = Identifier.CreateQualifiedIdentifier(source.DomainSchema, source.DomainName);
            metadata.BaseType = Option<IDbType>.Some(typeProvider.CreateColumnType(new ColumnTypeMetadata
            {
                TypeName = Identifier.CreateQualifiedIdentifier(PgCatalog, source.DataType),
                MaxLength = maxLength,
                NumericPrecision = numericPrecision,
                FractionalSecondsPrecision = fractionalSecondsPrecision,
            }));
            // the column stores values of the underlying type, so the class of data is the same
            metadata.DataType = metadata.BaseType.MatchUnsafe(static t => t.DataType, static () => DataType.Unknown);
            metadata.ClrType = metadata.BaseType.MatchUnsafe(static t => t.ClrType, static () => null);
            return metadata;
        }

        metadata.TypeName = Identifier.CreateQualifiedIdentifier(PgCatalog, source.DataType);
        return metadata;
    }

    /// <summary>
    /// Determines the length declared by a column's type. Character and bit string types declare a
    /// maximum length, while the numeric types instead declare a precision.
    /// </summary>
    /// <param name="characterMaximumLength">The declared maximum length of a character or bit string type, otherwise zero.</param>
    /// <param name="numericPrecision">The declared precision of a numeric type, otherwise zero.</param>
    /// <param name="numericPrecisionRadix">The base <paramref name="numericPrecision"/> is expressed in, otherwise zero.</param>
    /// <returns>A length in characters for a character type, or in decimal digits for a numeric one.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Any of the arguments is less than zero.</exception>
    public static int CreateMaxLength(int characterMaximumLength, int numericPrecision, int numericPrecisionRadix)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(characterMaximumLength);
        ArgumentOutOfRangeException.ThrowIfNegative(numericPrecision);
        ArgumentOutOfRangeException.ThrowIfNegative(numericPrecisionRadix);

        return characterMaximumLength > 0
            ? characterMaximumLength
            : ToDecimalDigits(numericPrecision, numericPrecisionRadix);
    }

    /// <summary>
    /// Describes the precision and scale of a numeric column, in decimal digits.
    /// </summary>
    /// <param name="numericPrecision">The declared precision of a numeric type, otherwise zero.</param>
    /// <param name="numericScale">The declared scale of an exact numeric type, otherwise zero.</param>
    /// <param name="numericPrecisionRadix">The base the precision and scale are expressed in, or zero when the column's type is not numeric.</param>
    /// <returns>A numeric precision, or <see cref="Option{A}.None"/> when the column's type is not a numeric one.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Any of the arguments is less than zero.</exception>
    public static Option<INumericPrecision> CreateNumericPrecision(int numericPrecision, int numericScale, int numericPrecisionRadix)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(numericPrecision);
        ArgumentOutOfRangeException.ThrowIfNegative(numericScale);
        ArgumentOutOfRangeException.ThrowIfNegative(numericPrecisionRadix);

        // only a numeric type has a radix, so its absence means there is no precision to report
        if (numericPrecisionRadix == 0)
            return Option<INumericPrecision>.None;

        return Option<INumericPrecision>.Some(new NumericPrecision(
            ToDecimalDigits(numericPrecision, numericPrecisionRadix),
            ToDecimalDigits(numericScale, numericPrecisionRadix)));
    }

    // information_schema expresses a numeric type's precision and scale in the base given by
    // numeric_precision_radix, which is 2 for the floating point types and 10 for numeric and
    // decimal. A precision of n digits in base b spans values up to b^n - 1, so a binary precision
    // is reported as the number of decimal digits that range requires, e.g. float8's 53 binary
    // digits become 16 decimal ones.
    private static int ToDecimalDigits(int digits, int radix)
    {
        if (digits == 0 || radix == 10 || radix < 2)
            return digits;

        return (int)Math.Ceiling(digits * Math.Log10(radix));
    }

    private static Identifier GetUserTypeName(CatalogTypeInfo source)
    {
        return source.UdtName.IsNullOrWhiteSpace()
            ? Identifier.CreateQualifiedIdentifier(PgCatalog, source.DataType)
            : Identifier.CreateQualifiedIdentifier(source.UdtSchema, source.UdtName);
    }

    private static Option<IDbType> CreateElementType(IDbTypeProvider typeProvider, CatalogTypeInfo source, IReadOnlyList<string> enumValues)
    {
        if (source.ElementTypeName.IsNullOrWhiteSpace())
            return Option<IDbType>.None;

        var elementMetadata = new ColumnTypeMetadata
        {
            TypeName = Identifier.CreateQualifiedIdentifier(source.ElementTypeSchema, source.ElementTypeName),
            DataType = GetUserDefinedDataType(source.ElementTypeKind),
        };

        // an element type that pg_catalog names is a built-in one, which the type provider classifies
        // from its name; only a user-defined element carries its own class and values
        if (string.Equals(source.ElementTypeSchema, PgCatalog, StringComparison.Ordinal))
            elementMetadata.DataType = DataType.Unknown;
        else if (elementMetadata.DataType == DataType.Enum)
            elementMetadata.EnumValues = enumValues;

        return Option<IDbType>.Some(typeProvider.CreateColumnType(elementMetadata));
    }

    private static DataType GetUserDefinedDataType(string? typeKind)
    {
        return typeKind switch
        {
            EnumTypeKind => DataType.Enum,
            CompositeTypeKind => DataType.Composite,
            RangeTypeKind or MultirangeTypeKind => DataType.Range,
            // a base or pseudo type that is not in pg_catalog is an extension type, e.g. a vector or
            // a geometry, which cannot be classified any further from the catalog alone
            _ => DataType.Other,
        };
    }

    /// <summary>
    /// The type information the catalog reports for a single column.
    /// </summary>
    /// <param name="DataType">The <c>information_schema</c> data type name, or <c>ARRAY</c>/<c>USER-DEFINED</c>.</param>
    /// <param name="UdtSchema">The schema of the underlying type.</param>
    /// <param name="UdtName">The name of the underlying type.</param>
    /// <param name="DomainSchema">The schema of the domain the column is defined over, if any.</param>
    /// <param name="DomainName">The name of the domain the column is defined over, if any.</param>
    /// <param name="TypeKind">The <c>pg_type.typtype</c> of the underlying type.</param>
    /// <param name="ElementTypeSchema">The schema of the element type of an array type.</param>
    /// <param name="ElementTypeName">The name of the element type of an array type.</param>
    /// <param name="ElementTypeKind">The <c>pg_type.typtype</c> of the element type of an array type.</param>
    /// <param name="EnumLabels">The labels of whichever of the type or its element type is an enum.</param>
    internal sealed record CatalogTypeInfo(
        string? DataType,
        string? UdtSchema,
        string? UdtName,
        string? DomainSchema,
        string? DomainName,
        string? TypeKind,
        string? ElementTypeSchema,
        string? ElementTypeName,
        string? ElementTypeKind,
        string[]? EnumLabels
    );
}
