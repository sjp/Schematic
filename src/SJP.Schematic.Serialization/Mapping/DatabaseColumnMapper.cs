using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a column between its core and serialized representations.
/// </summary>
public class DatabaseColumnMapper
    : IImmutableMapper<Dto.DatabaseColumn, IDatabaseColumn>
    , IImmutableMapper<IDatabaseColumn, Dto.DatabaseColumn>
    , IImmutableMapper<IDatabaseComputedColumn, Dto.DatabaseColumn>
{
    /// <summary>
    /// Maps a serialized column to its core representation.
    /// </summary>
    /// <param name="source">A serialized column.</param>
    /// <returns>A column. A computed column is returned when the serialized column is marked as computed.</returns>
    public IDatabaseColumn Map(Dto.DatabaseColumn source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var dbTypeMapper = MapperRegistry.GetMapper<Dto.DbType, IDbType>();
        var optionalMapper = MapperRegistry.GetMapper<string?, Option<string>>();

        if (source.IsComputed)
        {
            return new DatabaseComputedColumn(
                identifierMapper.Map(source.ColumnName),
                dbTypeMapper.Map(source.Type),
                source.IsNullable,
                optionalMapper.Map(source.DefaultValue),
                optionalMapper.Map(source.Definition)
            );
        }

        var autoIncrMapper = MapperRegistry.GetMapper<Dto.AutoIncrement?, Option<IAutoIncrement>>();

        return new DatabaseColumn(
            identifierMapper.Map(source.ColumnName),
            dbTypeMapper.Map(source.Type),
            source.IsNullable,
            optionalMapper.Map(source.DefaultValue),
            autoIncrMapper.Map(source.AutoIncrement)
        );
    }

    /// <summary>
    /// Maps a column to its serialized representation.
    /// </summary>
    /// <param name="source">A column.</param>
    /// <returns>A serialized column. A computed column is dispatched on its runtime type so that its definition is preserved.</returns>
    public Dto.DatabaseColumn Map(IDatabaseColumn source)
    {
        // overload resolution is static, so computed columns must be routed at runtime
        // otherwise their definitions are silently dropped when serializing a column collection
        if (source is IDatabaseComputedColumn computedColumn)
            return Map(computedColumn);

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
            AutoIncrement = autoIncrMapper.Map(source.AutoIncrement),
            IsComputed = source.IsComputed,
        };
    }

    /// <summary>
    /// Maps a computed column to its serialized representation.
    /// </summary>
    /// <param name="source">A computed column.</param>
    /// <returns>A serialized column.</returns>
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
            IsComputed = source.IsComputed,
        };
    }
}