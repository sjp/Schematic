using System;
using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class DbTypeProfile
    : IImmutableMapper<Dto.DbType, IDbType>
    , IImmutableMapper<IDbType, Dto.DbType>
{
    public IDbType Map(Dto.DbType source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var numericPrecisionMapper = MapperRegistry.GetMapper<Dto.NumericPrecision?, Option<INumericPrecision>>();
        var collationMapper = MapperRegistry.GetMapper<Dto.Identifier?, Option<Identifier>>();

        return new ColumnDataType(
            identifierMapper.Map(source.TypeName!),
            source.DataType,
            source.Definition!,
            Type.GetType(source.ClrTypeName ?? "System.Object")!,
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
            ClrTypeName = source.ClrType.ToString(),
            IsFixedLength = source.IsFixedLength,
            MaxLength = source.MaxLength,
            NumericPrecision = numericPrecisionMapper.Map(source.NumericPrecision),
            Collation = collationMapper.Map(source.Collation)
        };
    }
}
