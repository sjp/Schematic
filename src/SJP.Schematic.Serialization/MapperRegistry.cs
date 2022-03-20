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
        RegisterMapper<Dto.AutoIncrement?, Option<IAutoIncrement>>(() => new AutoIncrementProfile());
        RegisterMapper<Option<IAutoIncrement>, Dto.AutoIncrement?>(() => new AutoIncrementProfile());

        RegisterMapper<Dto.DatabaseCheckConstraint, IDatabaseCheckConstraint>(() => new DatabaseCheckProfile());
        RegisterMapper<IDatabaseCheckConstraint, Dto.DatabaseCheckConstraint>(() => new DatabaseCheckProfile());

        RegisterMapper<Dto.DatabaseColumn, IDatabaseColumn>(() => new DatabaseColumnProfile());
        RegisterMapper<IDatabaseColumn, Dto.DatabaseColumn>(() => new DatabaseColumnProfile());
        RegisterMapper<IDatabaseComputedColumn, Dto.DatabaseColumn>(() => new DatabaseColumnProfile());

        RegisterMapper<Dto.DatabaseIndexColumn, IDatabaseIndexColumn>(() => new DatabaseIndexColumnProfile());
        RegisterMapper<IDatabaseIndexColumn, Dto.DatabaseIndexColumn>(() => new DatabaseIndexColumnProfile());

        RegisterMapper<Dto.DatabaseKey, IDatabaseKey>(() => new DatabaseKeyProfile());
        RegisterMapper<IDatabaseKey, Dto.DatabaseKey>(() => new DatabaseKeyProfile());
        RegisterMapper<Dto.DatabaseKey?, Option<IDatabaseKey>>(() => new DatabaseKeyProfile());
        RegisterMapper<Option<IDatabaseKey>, Dto.DatabaseKey?>(() => new DatabaseKeyProfile());

        RegisterMapper<Dto.DatabaseRelationalKey, IDatabaseRelationalKey>(() => new DatabaseRelationalKeyProfile());
        RegisterMapper<IDatabaseRelationalKey, Dto.DatabaseRelationalKey>(() => new DatabaseRelationalKeyProfile());

        RegisterMapper<Dto.DatabaseRoutine, IDatabaseRoutine>(() => new DatabaseRoutineProfile());
        RegisterMapper<IDatabaseRoutine, Dto.DatabaseRoutine>(() => new DatabaseRoutineProfile());

        RegisterMapper<Dto.DatabaseSequence, IDatabaseSequence>(() => new DatabaseSequenceProfile());
        RegisterMapper<IDatabaseSequence, Dto.DatabaseSequence>(() => new DatabaseSequenceProfile());

        RegisterMapper<Dto.DatabaseSynonym, IDatabaseSynonym>(() => new DatabaseSynonymProfile());
        RegisterMapper<IDatabaseSynonym, Dto.DatabaseSynonym>(() => new DatabaseSynonymProfile());

        RegisterMapper<Dto.DatabaseTrigger, IDatabaseTrigger>(() => new DatabaseTriggerProfile());
        RegisterMapper<IDatabaseTrigger, Dto.DatabaseTrigger>(() => new DatabaseTriggerProfile());

        RegisterMapper<Dto.DatabaseView, IDatabaseView>(() => new DatabaseViewProfile());
        RegisterMapper<IDatabaseView, Dto.DatabaseView>(() => new DatabaseViewProfile());

        RegisterMapper<Dto.DbType, IDbType>(() => new DbTypeProfile());
        RegisterMapper<IDbType, Dto.DbType>(() => new DbTypeProfile());

        RegisterMapper<Dto.IdentifierDefaults, IIdentifierDefaults>(() => new IdentifierDefaultsProfile());
        RegisterMapper<IIdentifierDefaults, Dto.IdentifierDefaults>(() => new IdentifierDefaultsProfile());

        RegisterMapper<Dto.Identifier?, Option<Identifier>>(() => new IdentifierProfile());
        RegisterMapper<Option<Identifier>, Dto.Identifier>(() => new IdentifierProfile());
        RegisterMapper<Identifier, Dto.Identifier>(() => new IdentifierProfile());
        RegisterMapper<Dto.Identifier, Identifier>(() => new IdentifierProfile());

        RegisterMapper<Dto.DatabaseIndex, IDatabaseIndex>(() => new IndexProfile());
        RegisterMapper<IDatabaseIndex, Dto.DatabaseIndex>(() => new IndexProfile());

        RegisterMapper<Dto.NumericPrecision?, Option<INumericPrecision>>(() => new NumericPrecisionProfile());
        RegisterMapper<Option<INumericPrecision>, Dto.NumericPrecision?>(() => new NumericPrecisionProfile());

        RegisterMapper<string?, Option<string>>(() => new OptionProfile());
        RegisterMapper<Option<string>, string?>(() => new OptionProfile());
        RegisterMapper<decimal?, Option<decimal>>(() => new OptionProfile());
        RegisterMapper<Option<decimal>, decimal?>(() => new OptionProfile());

        RegisterMapper(() => new RelationalDatabaseProfile());

        RegisterMapper<Dto.RelationalDatabaseTable, IRelationalDatabaseTable>(() => new RelationalDatabaseTableProfile());
        RegisterMapper<IRelationalDatabaseTable, Dto.RelationalDatabaseTable>(() => new RelationalDatabaseTableProfile());

        // Comments
        RegisterMapper(() => new DatabaseCommentProviderProfile());

        RegisterMapper<Dto.Comments.DatabaseRoutineComments, IDatabaseRoutineComments>(() => new DatabaseRoutineCommentsProfile());
        RegisterMapper<IDatabaseRoutineComments, Dto.Comments.DatabaseRoutineComments>(() => new DatabaseRoutineCommentsProfile());

        RegisterMapper<Dto.Comments.DatabaseSequenceComments, IDatabaseSequenceComments>(() => new DatabaseSequenceCommentsProfile());
        RegisterMapper<IDatabaseSequenceComments, Dto.Comments.DatabaseSequenceComments>(() => new DatabaseSequenceCommentsProfile());

        RegisterMapper<Dto.Comments.DatabaseSynonymComments, IDatabaseSynonymComments>(() => new DatabaseSynonymCommentsProfile());
        RegisterMapper<IDatabaseSynonymComments, Dto.Comments.DatabaseSynonymComments>(() => new DatabaseSynonymCommentsProfile());

        RegisterMapper<Dto.Comments.DatabaseTableComments, IRelationalDatabaseTableComments>(() => new DatabaseTableCommentsProfile());
        RegisterMapper<IRelationalDatabaseTableComments, Dto.Comments.DatabaseTableComments>(() => new DatabaseTableCommentsProfile());

        RegisterMapper<Dto.Comments.DatabaseViewComments, IDatabaseViewComments>(() => new DatabaseViewCommentsProfile());
        RegisterMapper<IDatabaseViewComments, Dto.Comments.DatabaseViewComments>(() => new DatabaseViewCommentsProfile());
    }

    private static void RegisterMapper<TSource, TDestination>(Func<IImmutableMapper<TSource, TDestination>> factory)
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

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
