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

        // No database lets a column be both computed and an identity, so a document that says both
        // was not written from one. The computed expression is what produces the column's values,
        // so it is kept, and the identity is dropped.
        ColumnValueSource valueSource = source.IsComputed
            ? new ColumnValueSource.Computed(optionalMapper.Map(source.Definition), source.ComputedStorage)
            : autoIncrMapper.Map(source.AutoIncrement).Match<ColumnValueSource>(
                static identity => new ColumnValueSource.Generated(identity),
                static () => new ColumnValueSource.Stored()
            );

        return new DatabaseColumn(
            identifierMapper.Map(source.ColumnName),
            dbTypeMapper.Map(source.Type),
            source.IsNullable,
            defaultValue,
            valueSource,
            source.IsHidden
        );
    }

    private static DatabaseDefaultValue MapDefaultValue(Dto.DatabaseColumn source, string definition, IImmutableMapper<Dto.Identifier?, Option<Identifier>> identifierMapper)
    {
        var constraintName = identifierMapper.Map(source.DefaultConstraintName);

        return source.DefaultValueKind switch
        {
            Dto.DefaultValueKind.Unknown => new DatabaseDefaultValue.Unknown(definition, constraintName),
            Dto.DefaultValueKind.Literal => new DatabaseDefaultValue.Literal(definition, constraintName),
            Dto.DefaultValueKind.Null => new DatabaseDefaultValue.Null(definition, constraintName),
            Dto.DefaultValueKind.Expression => new DatabaseDefaultValue.Expression(definition, constraintName),
            Dto.DefaultValueKind.SequenceNextValue => new DatabaseDefaultValue.SequenceNextValue(definition, constraintName, identifierMapper.Map(source.DefaultSequenceName)),
            _ => throw new ArgumentException($"The {nameof(Dto.DefaultValueKind)} provided must be a valid enum.", nameof(source)),
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
            static () => (Dto.DefaultValueKind.Unknown, Option<Identifier>.None)
        );

        // The document keeps the members it has always had, which a stored column leaves empty:
        // not computed, with no definition, an unknown storage and no identity.
        var (isComputed, computedDefinition, computedStorage, autoIncrement) = source.ValueSource switch
        {
            ColumnValueSource.Stored => (false, Option<string>.None, ComputedColumnStorage.Unknown, Option<IAutoIncrement>.None),
            ColumnValueSource.Computed computed => (true, computed.Definition, computed.Storage, Option<IAutoIncrement>.None),
            ColumnValueSource.Generated generated => (false, Option<string>.None, ComputedColumnStorage.Unknown, Option<IAutoIncrement>.Some(generated.Identity)),
        };

        var result = new Dto.DatabaseColumn
        {
            ColumnName = identifierMapper.Map(source.Name),
            Type = type,
            IsNullable = source.IsNullable,
            DefaultValue = optionalMapper.Map(source.Default.Map(static def => def.Definition)),
            DefaultConstraintName = optionalIdentifierMapper.Map(source.Default.Bind(static def => def.ConstraintName)),
            DefaultValueKind = defaultValueKind,
            DefaultSequenceName = optionalIdentifierMapper.Map(defaultSequenceName),
            AutoIncrement = autoIncrMapper.Map(autoIncrement),
            IsComputed = isComputed,
            IsHidden = source.IsHidden,
            Definition = optionalMapper.Map(computedDefinition),
            ComputedStorage = computedStorage,
        };

        cache.Columns.Add(source, result);
        return result;
    }

    private static (Dto.DefaultValueKind Kind, Option<Identifier> SequenceName) MapDefaultValueKind(DatabaseDefaultValue defaultValue) => defaultValue switch
    {
        DatabaseDefaultValue.Unknown => (Dto.DefaultValueKind.Unknown, Option<Identifier>.None),
        DatabaseDefaultValue.Literal => (Dto.DefaultValueKind.Literal, Option<Identifier>.None),
        DatabaseDefaultValue.Null => (Dto.DefaultValueKind.Null, Option<Identifier>.None),
        DatabaseDefaultValue.Expression => (Dto.DefaultValueKind.Expression, Option<Identifier>.None),
        DatabaseDefaultValue.SequenceNextValue sequenceDefault => (Dto.DefaultValueKind.SequenceNextValue, sequenceDefault.SequenceName),
    };
}
