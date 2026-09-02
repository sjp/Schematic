using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Serialization.Mapping;
using SJP.Schematic.Serialization.Mapping.Comments;

namespace SJP.Schematic.Serialization;

/// <summary>
/// A poor man's DI container.
/// Using this as we know how to map between internal types and those are the only ones supported.
/// Allows us to more easily map nested objects with singleton objects.
/// </summary>
internal static class MapperRegistry
{
    private sealed record TypePair(Type SourceType, Type TargetType);

    private static readonly FrozenDictionary<TypePair, object> _cache = BuildMappers();

    private static FrozenDictionary<TypePair, object> BuildMappers()
    {
        var mappers = new Dictionary<TypePair, object>();

        RegisterMapper<Dto.AutoIncrement?, Option<IAutoIncrement>>(mappers, () => new AutoIncrementMapper());
        RegisterMapper<Option<IAutoIncrement>, Dto.AutoIncrement?>(mappers, () => new AutoIncrementMapper());

        RegisterMapper<Dto.DatabaseCheckConstraint, IDatabaseCheckConstraint>(mappers, () => new DatabaseCheckMapper());
        RegisterMapper<IDatabaseCheckConstraint, Dto.DatabaseCheckConstraint>(mappers, () => new DatabaseCheckMapper());

        RegisterMapper<Dto.DatabaseColumn, IDatabaseColumn>(mappers, () => new DatabaseColumnMapper());
        // computed columns are handled by the IDatabaseColumn mapping, which dispatches on the runtime type
        RegisterMapper<IDatabaseColumn, Dto.DatabaseColumn>(mappers, () => new DatabaseColumnMapper());

        RegisterMapper<Dto.DatabaseIndexColumn, IDatabaseIndexColumn>(mappers, () => new DatabaseIndexColumnMapper());
        RegisterMapper<IDatabaseIndexColumn, Dto.DatabaseIndexColumn>(mappers, () => new DatabaseIndexColumnMapper());

        RegisterMapper<Dto.DatabaseKey, IDatabaseKey>(mappers, () => new DatabaseKeyMapper());
        RegisterMapper<IDatabaseKey, Dto.DatabaseKey>(mappers, () => new DatabaseKeyMapper());
        RegisterMapper<Dto.DatabaseKey?, Option<IDatabaseKey>>(mappers, () => new DatabaseKeyMapper());
        RegisterMapper<Option<IDatabaseKey>, Dto.DatabaseKey?>(mappers, () => new DatabaseKeyMapper());

        RegisterMapper<Dto.DatabaseRelationalKey, IDatabaseRelationalKey>(mappers, () => new DatabaseRelationalKeyMapper());
        RegisterMapper<IDatabaseRelationalKey, Dto.DatabaseRelationalKey>(mappers, () => new DatabaseRelationalKeyMapper());

        RegisterMapper<Dto.DatabaseRoutine, IDatabaseRoutine>(mappers, () => new DatabaseRoutineMapper());
        RegisterMapper<IDatabaseRoutine, Dto.DatabaseRoutine>(mappers, () => new DatabaseRoutineMapper());

        RegisterMapper<Dto.DatabaseRoutineOverload, IDatabaseRoutineOverload>(mappers, () => new DatabaseRoutineOverloadMapper());
        RegisterMapper<IDatabaseRoutineOverload, Dto.DatabaseRoutineOverload>(mappers, () => new DatabaseRoutineOverloadMapper());

        RegisterMapper<Dto.DatabaseRoutineParameter, IDatabaseRoutineParameter>(mappers, () => new DatabaseRoutineParameterMapper());
        RegisterMapper<IDatabaseRoutineParameter, Dto.DatabaseRoutineParameter>(mappers, () => new DatabaseRoutineParameterMapper());

        RegisterMapper<Dto.DatabaseSequence, IDatabaseSequence>(mappers, () => new DatabaseSequenceMapper());
        RegisterMapper<IDatabaseSequence, Dto.DatabaseSequence>(mappers, () => new DatabaseSequenceMapper());

        RegisterMapper<Dto.DatabaseSynonym, IDatabaseSynonym>(mappers, () => new DatabaseSynonymMapper());
        RegisterMapper<IDatabaseSynonym, Dto.DatabaseSynonym>(mappers, () => new DatabaseSynonymMapper());

        RegisterMapper<Dto.DatabaseTrigger, IDatabaseTrigger>(mappers, () => new DatabaseTriggerMapper());
        RegisterMapper<IDatabaseTrigger, Dto.DatabaseTrigger>(mappers, () => new DatabaseTriggerMapper());

        RegisterMapper<Dto.DatabaseView, IDatabaseView>(mappers, () => new DatabaseViewMapper());
        RegisterMapper<IDatabaseView, Dto.DatabaseView>(mappers, () => new DatabaseViewMapper());

        RegisterMapper<Dto.DbType, IDbType>(mappers, () => new DbTypeMapper());
        RegisterMapper<IDbType, Dto.DbType>(mappers, () => new DbTypeMapper());
        RegisterMapper<Dto.DbType?, Option<IDbType>>(mappers, () => new DbTypeMapper());
        RegisterMapper<Option<IDbType>, Dto.DbType?>(mappers, () => new DbTypeMapper());

        RegisterMapper<Dto.IdentifierDefaults, IIdentifierDefaults>(mappers, () => new IdentifierDefaultsMapper());
        RegisterMapper<IIdentifierDefaults, Dto.IdentifierDefaults>(mappers, () => new IdentifierDefaultsMapper());

        RegisterMapper<Dto.Identifier?, Option<Identifier>>(mappers, () => new IdentifierMapper());
        RegisterMapper<Option<Identifier>, Dto.Identifier?>(mappers, () => new IdentifierMapper());
        RegisterMapper<Identifier, Dto.Identifier>(mappers, () => new IdentifierMapper());
        RegisterMapper<Dto.Identifier, Identifier>(mappers, () => new IdentifierMapper());

        RegisterMapper<Dto.DatabaseIndex, IDatabaseIndex>(mappers, () => new IndexMapper());
        RegisterMapper<IDatabaseIndex, Dto.DatabaseIndex>(mappers, () => new IndexMapper());

        RegisterMapper<Dto.NumericPrecision?, Option<INumericPrecision>>(mappers, () => new NumericPrecisionMapper());
        RegisterMapper<Option<INumericPrecision>, Dto.NumericPrecision?>(mappers, () => new NumericPrecisionMapper());

        RegisterMapper<string?, Option<string>>(mappers, () => new OptionMapper());
        RegisterMapper<Option<string>, string?>(mappers, () => new OptionMapper());
        RegisterMapper<decimal?, Option<decimal>>(mappers, () => new OptionMapper());
        RegisterMapper<Option<decimal>, decimal?>(mappers, () => new OptionMapper());

        RegisterMapper<Dto.RelationalDatabaseTable, IRelationalDatabaseTable>(mappers, () => new RelationalDatabaseTableMapper());
        RegisterMapper<IRelationalDatabaseTable, Dto.RelationalDatabaseTable>(mappers, () => new RelationalDatabaseTableMapper());

        // Comments
        RegisterMapper<Dto.Comments.DatabaseRoutineComments, IDatabaseRoutineComments>(mappers, () => new DatabaseRoutineCommentsMapper());
        RegisterMapper<IDatabaseRoutineComments, Dto.Comments.DatabaseRoutineComments>(mappers, () => new DatabaseRoutineCommentsMapper());

        RegisterMapper<Dto.Comments.DatabaseSequenceComments, IDatabaseSequenceComments>(mappers, () => new DatabaseSequenceCommentsMapper());
        RegisterMapper<IDatabaseSequenceComments, Dto.Comments.DatabaseSequenceComments>(mappers, () => new DatabaseSequenceCommentsMapper());

        RegisterMapper<Dto.Comments.DatabaseSynonymComments, IDatabaseSynonymComments>(mappers, () => new DatabaseSynonymCommentsMapper());
        RegisterMapper<IDatabaseSynonymComments, Dto.Comments.DatabaseSynonymComments>(mappers, () => new DatabaseSynonymCommentsMapper());

        RegisterMapper<Dto.Comments.DatabaseTableComments, IRelationalDatabaseTableComments>(mappers, () => new DatabaseTableCommentsMapper());
        RegisterMapper<IRelationalDatabaseTableComments, Dto.Comments.DatabaseTableComments>(mappers, () => new DatabaseTableCommentsMapper());

        RegisterMapper<Dto.Comments.DatabaseViewComments, IDatabaseViewComments>(mappers, () => new DatabaseViewCommentsMapper());
        RegisterMapper<IDatabaseViewComments, Dto.Comments.DatabaseViewComments>(mappers, () => new DatabaseViewCommentsMapper());

        return mappers.ToFrozenDictionary();
    }

    private static void RegisterMapper<TSource, TDestination>(Dictionary<TypePair, object> mappers, Func<IImmutableMapper<TSource, TDestination>> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var typePair = new TypePair(typeof(TSource), typeof(TDestination));
        mappers[typePair] = factory.Invoke();
    }

    public static IImmutableMapper<TSource, TDestination> GetMapper<TSource, TDestination>()
    {
        var key = new TypePair(typeof(TSource), typeof(TDestination));
        if (!_cache.TryGetValue(key, out var mapper))
            throw new KeyNotFoundException($"Cannot map {typeof(TSource).FullName} to {typeof(TDestination).FullName}. A mapper has not been registered for this projection.");

        if (mapper is not IImmutableMapper<TSource, TDestination> resultMapper)
            throw new InvalidOperationException($"The mapper registered for the projection {typeof(TSource).FullName} to {typeof(TDestination).FullName} is not an {typeof(IImmutableMapper<,>).FullName} instance.");

        return resultMapper;
    }
}