using System;
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

    private static readonly Dictionary<TypePair, object> _cache = new();

    static MapperRegistry()
    {
        RegisterMappers();
    }

    private static void RegisterMappers()
    {
        RegisterMapper<Dto.AutoIncrement?, Option<IAutoIncrement>>(() => new AutoIncrementMapper());
        RegisterMapper<Option<IAutoIncrement>, Dto.AutoIncrement?>(() => new AutoIncrementMapper());

        RegisterMapper<Dto.DatabaseCheckConstraint, IDatabaseCheckConstraint>(() => new DatabaseCheckMapper());
        RegisterMapper<IDatabaseCheckConstraint, Dto.DatabaseCheckConstraint>(() => new DatabaseCheckMapper());

        RegisterMapper<Dto.DatabaseColumn, IDatabaseColumn>(() => new DatabaseColumnMapper());
        RegisterMapper<IDatabaseColumn, Dto.DatabaseColumn>(() => new DatabaseColumnMapper());
        RegisterMapper<IDatabaseComputedColumn, Dto.DatabaseColumn>(() => new DatabaseColumnMapper());

        RegisterMapper<Dto.DatabaseIndexColumn, IDatabaseIndexColumn>(() => new DatabaseIndexColumnMapper());
        RegisterMapper<IDatabaseIndexColumn, Dto.DatabaseIndexColumn>(() => new DatabaseIndexColumnMapper());

        RegisterMapper<Dto.DatabaseKey, IDatabaseKey>(() => new DatabaseKeyMapper());
        RegisterMapper<IDatabaseKey, Dto.DatabaseKey>(() => new DatabaseKeyMapper());
        RegisterMapper<Dto.DatabaseKey?, Option<IDatabaseKey>>(() => new DatabaseKeyMapper());
        RegisterMapper<Option<IDatabaseKey>, Dto.DatabaseKey?>(() => new DatabaseKeyMapper());

        RegisterMapper<Dto.DatabaseRelationalKey, IDatabaseRelationalKey>(() => new DatabaseRelationalKeyMapper());
        RegisterMapper<IDatabaseRelationalKey, Dto.DatabaseRelationalKey>(() => new DatabaseRelationalKeyMapper());

        RegisterMapper<Dto.DatabaseRoutine, IDatabaseRoutine>(() => new DatabaseRoutineMapper());
        RegisterMapper<IDatabaseRoutine, Dto.DatabaseRoutine>(() => new DatabaseRoutineMapper());

        RegisterMapper<Dto.DatabaseSequence, IDatabaseSequence>(() => new DatabaseSequenceMapper());
        RegisterMapper<IDatabaseSequence, Dto.DatabaseSequence>(() => new DatabaseSequenceMapper());

        RegisterMapper<Dto.DatabaseSynonym, IDatabaseSynonym>(() => new DatabaseSynonymMapper());
        RegisterMapper<IDatabaseSynonym, Dto.DatabaseSynonym>(() => new DatabaseSynonymMapper());

        RegisterMapper<Dto.DatabaseTrigger, IDatabaseTrigger>(() => new DatabaseTriggerMapper());
        RegisterMapper<IDatabaseTrigger, Dto.DatabaseTrigger>(() => new DatabaseTriggerMapper());

        RegisterMapper<Dto.DatabaseView, IDatabaseView>(() => new DatabaseViewMapper());
        RegisterMapper<IDatabaseView, Dto.DatabaseView>(() => new DatabaseViewMapper());

        RegisterMapper<Dto.DbType, IDbType>(() => new DbTypeMapper());
        RegisterMapper<IDbType, Dto.DbType>(() => new DbTypeMapper());

        RegisterMapper<Dto.IdentifierDefaults, IIdentifierDefaults>(() => new IdentifierDefaultsMapper());
        RegisterMapper<IIdentifierDefaults, Dto.IdentifierDefaults>(() => new IdentifierDefaultsMapper());

        RegisterMapper<Dto.Identifier?, Option<Identifier>>(() => new IdentifierMapper());
        RegisterMapper<Option<Identifier>, Dto.Identifier>(() => new IdentifierMapper());
        RegisterMapper<Identifier, Dto.Identifier>(() => new IdentifierMapper());
        RegisterMapper<Dto.Identifier, Identifier>(() => new IdentifierMapper());

        RegisterMapper<Dto.DatabaseIndex, IDatabaseIndex>(() => new IndexMapper());
        RegisterMapper<IDatabaseIndex, Dto.DatabaseIndex>(() => new IndexMapper());

        RegisterMapper<Dto.NumericPrecision?, Option<INumericPrecision>>(() => new NumericPrecisionMapper());
        RegisterMapper<Option<INumericPrecision>, Dto.NumericPrecision?>(() => new NumericPrecisionMapper());

        RegisterMapper<string?, Option<string>>(() => new OptionMapper());
        RegisterMapper<Option<string>, string?>(() => new OptionMapper());
        RegisterMapper<decimal?, Option<decimal>>(() => new OptionMapper());
        RegisterMapper<Option<decimal>, decimal?>(() => new OptionMapper());

        RegisterMapper(() => new RelationalDatabaseMapper());

        RegisterMapper<Dto.RelationalDatabaseTable, IRelationalDatabaseTable>(() => new RelationalDatabaseTableMapper());
        RegisterMapper<IRelationalDatabaseTable, Dto.RelationalDatabaseTable>(() => new RelationalDatabaseTableMapper());

        // Comments
        RegisterMapper(() => new DatabaseCommentProviderMapper());

        RegisterMapper<Dto.Comments.DatabaseRoutineComments, IDatabaseRoutineComments>(() => new DatabaseRoutineCommentsMapper());
        RegisterMapper<IDatabaseRoutineComments, Dto.Comments.DatabaseRoutineComments>(() => new DatabaseRoutineCommentsMapper());

        RegisterMapper<Dto.Comments.DatabaseSequenceComments, IDatabaseSequenceComments>(() => new DatabaseSequenceCommentsMapper());
        RegisterMapper<IDatabaseSequenceComments, Dto.Comments.DatabaseSequenceComments>(() => new DatabaseSequenceCommentsMapper());

        RegisterMapper<Dto.Comments.DatabaseSynonymComments, IDatabaseSynonymComments>(() => new DatabaseSynonymCommentsMapper());
        RegisterMapper<IDatabaseSynonymComments, Dto.Comments.DatabaseSynonymComments>(() => new DatabaseSynonymCommentsMapper());

        RegisterMapper<Dto.Comments.DatabaseTableComments, IRelationalDatabaseTableComments>(() => new DatabaseTableCommentsMapper());
        RegisterMapper<IRelationalDatabaseTableComments, Dto.Comments.DatabaseTableComments>(() => new DatabaseTableCommentsMapper());

        RegisterMapper<Dto.Comments.DatabaseViewComments, IDatabaseViewComments>(() => new DatabaseViewCommentsMapper());
        RegisterMapper<IDatabaseViewComments, Dto.Comments.DatabaseViewComments>(() => new DatabaseViewCommentsMapper());
    }

    private static void RegisterMapper<TSource, TDestination>(Func<IImmutableMapper<TSource, TDestination>> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var typePair = new TypePair(typeof(TSource), typeof(TDestination));
        _cache[typePair] = factory.Invoke();
    }

    public static IImmutableMapper<TSource, TDestination> GetMapper<TSource, TDestination>()
    {
        var key = new TypePair(typeof(TSource), typeof(TDestination));
        if (!_cache.TryGetValue(key, out var mapper))
            throw new KeyNotFoundException($"Cannot map { typeof(TSource).FullName } to { typeof(TDestination).FullName }. A mapper has not been registered for this projection.");

        if (mapper is not IImmutableMapper<TSource, TDestination> resultMapper)
            throw new InvalidOperationException($"The mapper registered for the projection { typeof(TSource).FullName } to { typeof(TDestination).FullName } is not an { typeof(IImmutableMapper<,>).FullName } instance.");

        return resultMapper;
    }
}