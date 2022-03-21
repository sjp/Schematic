using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class DatabaseColumnMapper
    : IImmutableMapper<Dto.DatabaseColumn, IDatabaseColumn>
    , IImmutableMapper<IDatabaseColumn, Dto.DatabaseColumn>
    , IImmutableMapper<IDatabaseComputedColumn, Dto.DatabaseColumn>
{
    public IDatabaseColumn Map(Dto.DatabaseColumn source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var dbTypeMapper = MapperRegistry.GetMapper<Dto.DbType, IDbType>();
        var optionalMapper = MapperRegistry.GetMapper<string?, Option<string>>();

        if (source.IsComputed)
        {
            return new DatabaseComputedColumn(
                identifierMapper.Map(source.ColumnName!),
                dbTypeMapper.Map(source.Type!),
                source.IsNullable,
                optionalMapper.Map(source.DefaultValue),
                optionalMapper.Map(source.Definition)
            );
        }

        var autoIncrMapper = MapperRegistry.GetMapper<Dto.AutoIncrement?, Option<IAutoIncrement>>();

        return new DatabaseColumn(
            identifierMapper.Map(source.ColumnName!),
            dbTypeMapper.Map(source.Type!),
            source.IsNullable,
            optionalMapper.Map(source.DefaultValue),
            autoIncrMapper.Map(source.AutoIncrement)
        );
    }

    public Dto.DatabaseColumn Map(IDatabaseColumn source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var dbTypeMapper = MapperRegistry.GetMapper<IDbType, Dto.DbType>();
        var optionalMapper = MapperRegistry.GetMapper<Option<string>, string?>();
        var autoIncrMapper = MapperRegistry.GetMapper<Option<IAutoIncrement>, Dto.AutoIncrement?>();

        return new Dto.DatabaseColumn
        {
            ColumnName = identifierMapper.Map(source.Name),
            Type = dbTypeMapper.Map(source.Type),
            IsNullable = source.IsNullable,
            DefaultValue = optionalMapper.Map(source.DefaultValue),
            AutoIncrement = autoIncrMapper.Map(source.AutoIncrement)
        };
    }

    public Dto.DatabaseColumn Map(IDatabaseComputedColumn source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var dbTypeMapper = MapperRegistry.GetMapper<IDbType, Dto.DbType>();
        var optionalMapper = MapperRegistry.GetMapper<Option<string>, string?>();

        return new Dto.DatabaseColumn
        {
            ColumnName = identifierMapper.Map(source.Name),
            Type = dbTypeMapper.Map(source.Type),
            IsNullable = source.IsNullable,
            DefaultValue = optionalMapper.Map(source.DefaultValue),
            Definition = optionalMapper.Map(source.Definition),
            IsComputed = source.IsComputed
        };
    }
}