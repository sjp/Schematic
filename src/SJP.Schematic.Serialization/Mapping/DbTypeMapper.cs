using System;
using System.Collections.Generic;
using System.Linq;
using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a column data type between its core and serialized representations.
/// </summary>
public class DbTypeMapper
    : IImmutableMapper<Dto.DbType, IDbType>
    , IImmutableMapper<IDbType, Dto.DbType>
    , IImmutableMapper<Dto.DbType?, Option<IDbType>>
    , IImmutableMapper<Option<IDbType>, Dto.DbType?>
{
    /// <summary>
    /// Maps a serialized column data type to its core representation.
    /// </summary>
    /// <param name="source">A serialized column data type.</param>
    /// <returns>A column data type. Its CLR type is <see cref="object"/> when the serialized type does not name one.</returns>
    /// <exception cref="InvalidOperationException">The serialized type names a CLR type that no loaded assembly declares.</exception>
    public IDbType Map(Dto.DbType source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var numericPrecisionMapper = MapperRegistry.GetMapper<Dto.NumericPrecision?, Option<INumericPrecision>>();
        var collationMapper = MapperRegistry.GetMapper<Dto.Identifier?, Option<Identifier>>();

        // an absent name means the source database did not know a CLR type for the column
        var clrType = string.IsNullOrWhiteSpace(source.ClrTypeName)
            ? typeof(object)
            : ClrTypeResolver.Resolve(source.ClrTypeName)
                ?? throw new InvalidOperationException($"Unable to resolve the CLR type '{source.ClrTypeName}' given for the column type '{source.TypeName.LocalName}'. Types are only resolved from assemblies that are already loaded.");

        return new ColumnDataType(
            identifierMapper.Map(source.TypeName),
            source.DataType,
            source.Definition,
            clrType,
            source.IsFixedLength,
            source.MaxLength,
            numericPrecisionMapper.Map(source.NumericPrecision),
            collationMapper.Map(source.Collation),
            source.ElementType == null ? Option<IDbType>.None : Option<IDbType>.Some(Map(source.ElementType)),
            source.EnumValues?.ToList() ?? (IReadOnlyList<string>)[],
            source.BaseType == null ? Option<IDbType>.None : Option<IDbType>.Some(Map(source.BaseType)),
            source.IsUnsigned
        );
    }

    /// <summary>
    /// Maps a column data type to its serialized representation.
    /// </summary>
    /// <param name="source">A column data type.</param>
    /// <returns>A serialized column data type. Its CLR type is named without any assembly information.</returns>
    public Dto.DbType Map(IDbType source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var numericPrecisionMapper = MapperRegistry.GetMapper<Option<INumericPrecision>, Dto.NumericPrecision?>();
        var collationMapper = MapperRegistry.GetMapper<Option<Identifier>, Dto.Identifier?>();

        return new Dto.DbType
        {
            TypeName = identifierMapper.Map(source.TypeName),
            DataType = source.DataType,
            Definition = source.Definition,
            // deliberately not an assembly-qualified name, which would pin an assembly version in the
            // document; resolution searches loaded assemblies for the name instead
            ClrTypeName = source.ClrType.ToString(),
            IsFixedLength = source.IsFixedLength,
            MaxLength = source.MaxLength,
            NumericPrecision = numericPrecisionMapper.Map(source.NumericPrecision),
            Collation = collationMapper.Map(source.Collation),
            ElementType = source.ElementType.MatchUnsafe(Map, static () => (Dto.DbType?)null),
            // an empty collection is left out of the document rather than written as an empty array
            EnumValues = source.EnumValues.Count > 0 ? source.EnumValues.ToList() : null,
            BaseType = source.BaseType.MatchUnsafe(Map, static () => (Dto.DbType?)null),
            IsUnsigned = source.IsUnsigned,
        };
    }

    // an explicit implementation because the nullable annotation alone does not distinguish this
    // overload from the one taking a serialized type that is known to be present
    Option<IDbType> IImmutableMapper<Dto.DbType?, Option<IDbType>>.Map(Dto.DbType? source)
    {
        return source == null
            ? Option<IDbType>.None
            : Option<IDbType>.Some(Map(source));
    }

    /// <summary>
    /// Maps an optional column data type to its serialized representation.
    /// </summary>
    /// <param name="source">A column data type, if one is available.</param>
    /// <returns>A serialized column data type, or <see langword="null"/> when no type is available.</returns>
    public Dto.DbType? Map(Option<IDbType> source)
    {
        return source.MatchUnsafe(Map, static () => (Dto.DbType?)null);
    }
}
