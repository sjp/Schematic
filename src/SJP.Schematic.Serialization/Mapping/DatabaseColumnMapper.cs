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
{
    /// <summary>
    /// Maps a serialized column to its core representation.
    /// </summary>
    /// <param name="source">A serialized column.</param>
    /// <returns>A column.</returns>
    public IDatabaseColumn Map(Dto.DatabaseColumn source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var optionalIdentifierMapper = MapperRegistry.GetMapper<Dto.Identifier?, Option<Identifier>>();
        var dbTypeMapper = MapperRegistry.GetMapper<Dto.DbType, IDbType>();
        var optionalMapper = MapperRegistry.GetMapper<string?, Option<string>>();
        var autoIncrMapper = MapperRegistry.GetMapper<Dto.AutoIncrement?, Option<IAutoIncrement>>();

        var defaultValue = optionalMapper.Map(source.DefaultValue)
            .Map(def => (IDatabaseDefaultValue)new DatabaseDefaultValue(
                def,
                source.DefaultValueKind,
                optionalIdentifierMapper.Map(source.DefaultConstraintName),
                optionalIdentifierMapper.Map(source.DefaultSequenceName)
            ));

        return new DatabaseColumn(
            identifierMapper.Map(source.ColumnName),
            dbTypeMapper.Map(source.Type),
            source.IsNullable,
            defaultValue,
            autoIncrMapper.Map(source.AutoIncrement),
            source.IsComputed,
            optionalMapper.Map(source.Definition),
            source.ComputedStorage
        );
    }

    /// <summary>
    /// Maps a column to its serialized representation.
    /// </summary>
    /// <param name="source">A column.</param>
    /// <returns>A serialized column.</returns>
    public Dto.DatabaseColumn Map(IDatabaseColumn source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var optionalIdentifierMapper = MapperRegistry.GetMapper<Option<Identifier>, Dto.Identifier?>();
        var dbTypeMapper = MapperRegistry.GetMapper<IDbType, Dto.DbType>();
        var optionalMapper = MapperRegistry.GetMapper<Option<string>, string?>();
        var autoIncrMapper = MapperRegistry.GetMapper<Option<IAutoIncrement>, Dto.AutoIncrement?>();

        return new Dto.DatabaseColumn
        {
            ColumnName = identifierMapper.Map(source.Name),
            Type = dbTypeMapper.Map(source.Type),
            IsNullable = source.IsNullable,
            DefaultValue = optionalMapper.Map(source.Default.Map(static def => def.Definition)),
            DefaultConstraintName = optionalIdentifierMapper.Map(source.Default.Bind(static def => def.ConstraintName)),
            DefaultValueKind = source.Default.Match(static def => def.Kind, static () => DefaultValueKind.Unknown),
            DefaultSequenceName = optionalIdentifierMapper.Map(source.Default.Bind(static def => def.SequenceName)),
            AutoIncrement = autoIncrMapper.Map(source.AutoIncrement),
            IsComputed = source.IsComputed,
            Definition = optionalMapper.Map(source.ComputedDefinition),
            ComputedStorage = source.ComputedStorage,
        };
    }
}
