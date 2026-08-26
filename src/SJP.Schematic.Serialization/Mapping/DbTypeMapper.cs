using System;
using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class DbTypeMapper
    : IImmutableMapper<Dto.DbType, IDbType>
    , IImmutableMapper<IDbType, Dto.DbType>
{
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
            collationMapper.Map(source.Collation)
        );
    }

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
        };
    }
}