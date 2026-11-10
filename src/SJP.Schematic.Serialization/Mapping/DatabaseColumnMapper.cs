using System;
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
            .Map(def => MapDefaultValue(source, def, optionalIdentifierMapper));

        return new DatabaseColumn(
            identifierMapper.Map(source.ColumnName),
            dbTypeMapper.Map(source.Type),
            source.IsNullable,
            defaultValue,
            autoIncrMapper.Map(source.AutoIncrement),
            source.IsComputed,
            optionalMapper.Map(source.Definition),
            source.ComputedStorage,
            source.IsHidden
        );
    }

    private static DatabaseDefaultValue MapDefaultValue(Dto.DatabaseColumn source, string definition, IImmutableMapper<Dto.Identifier?, Option<Identifier>> identifierMapper)
    {
        var constraintName = identifierMapper.Map(source.DefaultConstraintName);

        return source.DefaultValueKind switch
        {
            DefaultValueKind.Unknown => new DatabaseDefaultValue.Unknown(definition, constraintName),
            DefaultValueKind.Literal => new DatabaseDefaultValue.Literal(definition, constraintName),
            DefaultValueKind.Null => new DatabaseDefaultValue.Null(definition, constraintName),
            DefaultValueKind.Expression => new DatabaseDefaultValue.Expression(definition, constraintName),
            DefaultValueKind.SequenceNextValue => new DatabaseDefaultValue.SequenceNextValue(definition, constraintName, identifierMapper.Map(source.DefaultSequenceName)),
            _ => throw new ArgumentException($"The {nameof(DefaultValueKind)} provided must be a valid enum.", nameof(source)),
        };
    }

    /// <summary>
    /// Maps a column to its serialized representation.
    /// </summary>
    /// <param name="source">A column.</param>
    /// <returns>A serialized column.</returns>
    public Dto.DatabaseColumn Map(IDatabaseColumn source) => Map(source, new SerializedObjectCache());

    internal Dto.DatabaseColumn Map(IDatabaseColumn source, SerializedObjectCache cache)
    {
        if (cache.Columns.TryGetValue(source, out var cached))
            return cached;

        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var optionalIdentifierMapper = MapperRegistry.GetMapper<Option<Identifier>, Dto.Identifier?>();
        var dbTypeMapper = MapperRegistry.GetMapper<IDbType, Dto.DbType>();
        var optionalMapper = MapperRegistry.GetMapper<Option<string>, string?>();
        var autoIncrMapper = MapperRegistry.GetMapper<Option<IAutoIncrement>, Dto.AutoIncrement?>();

        if (!cache.Types.TryGetValue(source.Type, out var type))
        {
            type = dbTypeMapper.Map(source.Type);
            cache.Types.Add(source.Type, type);
        }

        var (defaultValueKind, defaultSequenceName) = source.Default.Match(
            MapDefaultValueKind,
            static () => (DefaultValueKind.Unknown, Option<Identifier>.None)
        );

        var result = new Dto.DatabaseColumn
        {
            ColumnName = identifierMapper.Map(source.Name),
            Type = type,
            IsNullable = source.IsNullable,
            DefaultValue = optionalMapper.Map(source.Default.Map(static def => def.Definition)),
            DefaultConstraintName = optionalIdentifierMapper.Map(source.Default.Bind(static def => def.ConstraintName)),
            DefaultValueKind = defaultValueKind,
            DefaultSequenceName = optionalIdentifierMapper.Map(defaultSequenceName),
            AutoIncrement = autoIncrMapper.Map(source.AutoIncrement),
            IsComputed = source.IsComputed,
            IsHidden = source.IsHidden,
            Definition = optionalMapper.Map(source.ComputedDefinition),
            ComputedStorage = source.ComputedStorage,
        };

        cache.Columns.Add(source, result);
        return result;
    }

    private static (DefaultValueKind Kind, Option<Identifier> SequenceName) MapDefaultValueKind(DatabaseDefaultValue defaultValue) => defaultValue switch
    {
        DatabaseDefaultValue.Unknown => (DefaultValueKind.Unknown, Option<Identifier>.None),
        DatabaseDefaultValue.Literal => (DefaultValueKind.Literal, Option<Identifier>.None),
        DatabaseDefaultValue.Null => (DefaultValueKind.Null, Option<Identifier>.None),
        DatabaseDefaultValue.Expression => (DefaultValueKind.Expression, Option<Identifier>.None),
        DatabaseDefaultValue.SequenceNextValue sequenceDefault => (DefaultValueKind.SequenceNextValue, sequenceDefault.SequenceName),
    };
}
