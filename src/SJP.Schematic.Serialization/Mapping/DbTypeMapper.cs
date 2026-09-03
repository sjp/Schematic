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
    /// <returns>
    /// A column data type. Its <see cref="IDbType.ClrType"/> is <see cref="object"/> when the serialized
    /// type does not name one, or names one that no loaded assembly declares; either way
    /// <see cref="IDbType.ClrTypeName"/> is what the document said, so a document read in a process
    /// missing the assembly can be written back out unchanged.
    /// </returns>
    public IDbType Map(Dto.DbType source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var numericPrecisionMapper = MapperRegistry.GetMapper<Dto.NumericPrecision?, Option<INumericPrecision>>();
        var collationMapper = MapperRegistry.GetMapper<Dto.Identifier?, Option<Identifier>>();
        var fractionalSecondsMapper = MapperRegistry.GetMapper<int?, Option<int>>();

        // an absent name means the source database did not know a CLR type for the column, and an
        // unresolvable one means this process cannot name the type the source database knew
        var clrTypeName = string.IsNullOrWhiteSpace(source.ClrTypeName) ? null : source.ClrTypeName;
        var clrType = clrTypeName == null
            ? typeof(object)
            : ClrTypeResolver.Resolve(clrTypeName) ?? typeof(object);

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
            source.IsUnsigned,
            clrTypeName,
            fractionalSecondsMapper.Map(source.FractionalSecondsPrecision)
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
        var fractionalSecondsMapper = MapperRegistry.GetMapper<Option<int>, int?>();

        return new Dto.DbType
        {
            TypeName = identifierMapper.Map(source.TypeName),
            DataType = source.DataType,
            Definition = source.Definition,
            // deliberately not an assembly-qualified name, which would pin an assembly version in the
            // document; resolution searches loaded assemblies for the name instead
            ClrTypeName = source.ClrTypeName,
            IsFixedLength = source.IsFixedLength,
            MaxLength = source.MaxLength,
            NumericPrecision = numericPrecisionMapper.Map(source.NumericPrecision),
            Collation = collationMapper.Map(source.Collation),
            FractionalSecondsPrecision = fractionalSecondsMapper.Map(source.FractionalSecondsPrecision),
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
