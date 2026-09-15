using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Boxed.Mapping;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a database definition between its core and serialized representations.
/// </summary>
public class RelationalDatabaseMapper
    : IAsyncImmutableMapper<IRelationalDatabase, Dto.RelationalDatabase>
{
    /// <summary>
    /// Maps a serialized database definition to a database.
    /// </summary>
    /// <param name="source">A serialized database definition.</param>
    /// <param name="identifierResolver">An identifier resolver used by the resulting database to look up objects.</param>
    /// <returns>A database.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="identifierResolver"/> is <c>null</c>.</exception>
    public IRelationalDatabase Map(Dto.RelationalDatabase source, IIdentifierResolutionStrategy identifierResolver)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(identifierResolver);

        var identifierDefaultsMapper = MapperRegistry.GetMapper<Dto.IdentifierDefaults, IIdentifierDefaults>();
        var viewMapper = MapperRegistry.GetMapper<Dto.DatabaseView, IDatabaseView>();
        var sequenceMapper = MapperRegistry.GetMapper<Dto.DatabaseSequence, IDatabaseSequence>();
        var synonymMapper = MapperRegistry.GetMapper<Dto.DatabaseSynonym, IDatabaseSynonym>();
        var routineMapper = MapperRegistry.GetMapper<Dto.DatabaseRoutine, IDatabaseRoutine>();
        var userDefinedTypeMapper = MapperRegistry.GetMapper<Dto.DatabaseUserDefinedType, IDatabaseUserDefinedType>();
        var schemaMapper = MapperRegistry.GetMapper<Dto.DatabaseSchema, IDatabaseSchema>();

        return new RelationalDatabase(
            identifierDefaultsMapper.Map(source.IdentifierDefaults),
            identifierResolver,
            MapTables(source.Tables),
            viewMapper.MapList(source.Views),
            sequenceMapper.MapList(source.Sequences),
            synonymMapper.MapList(source.Synonyms),
            routineMapper.MapList(source.Routines),
            userDefinedTypeMapper.MapList(source.UserDefinedTypes),
            schemaMapper.MapList(source.Schemas)
        );
    }

    // Every table's columns are mapped before any table is built, so that both sides of a foreign key
    // can be read as the columns of the tables they belong to rather than as copies. A table name that
    // appears more than once is ambiguous, so foreign keys naming it keep their own copies.
    private static List<IRelationalDatabaseTable> MapTables(IEnumerable<Dto.RelationalDatabaseTable> source)
    {
        var columnMapper = MapperRegistry.GetMapper<Dto.DatabaseColumn, IDatabaseColumn>();
        var tableMapper = (RelationalDatabaseTableMapper)MapperRegistry.GetMapper<Dto.RelationalDatabaseTable, IRelationalDatabaseTable>();

        var tables = source.ToList();
        var tableColumns = tables.ConvertAll(table => new ColumnLookup(columnMapper.MapList(table.Columns)));

        var columnsByTableName = new Dictionary<Dto.Identifier, ColumnLookup>(tables.Count);
        var duplicateTableNames = new HashSet<Dto.Identifier>();
        for (var i = 0; i < tables.Count; i++)
        {
            if (!columnsByTableName.TryAdd(tables[i].TableName, tableColumns[i]))
                duplicateTableNames.Add(tables[i].TableName);
        }
        foreach (var tableName in duplicateTableNames)
            columnsByTableName.Remove(tableName);

        var result = new List<IRelationalDatabaseTable>(tables.Count);
        for (var i = 0; i < tables.Count; i++)
            result.Add(tableMapper.Map(tables[i], tableColumns[i], columnsByTableName));

        return result;
    }

    /// <summary>
    /// Maps a database to a serialized database definition.
    /// </summary>
    /// <param name="source">A database.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A serialized database definition.</returns>
    public async Task<Dto.RelationalDatabase> MapAsync(IRelationalDatabase source, CancellationToken cancellationToken)
    {
        var tableMapper = (RelationalDatabaseTableMapper)MapperRegistry.GetMapper<IRelationalDatabaseTable, Dto.RelationalDatabaseTable>();
        var viewMapper = (DatabaseViewMapper)MapperRegistry.GetMapper<IDatabaseView, Dto.DatabaseView>();
        var sequenceMapper = MapperRegistry.GetMapper<IDatabaseSequence, Dto.DatabaseSequence>();
        var synonymMapper = MapperRegistry.GetMapper<IDatabaseSynonym, Dto.DatabaseSynonym>();
        var routineMapper = MapperRegistry.GetMapper<IDatabaseRoutine, Dto.DatabaseRoutine>();
        var userDefinedTypeMapper = MapperRegistry.GetMapper<IDatabaseUserDefinedType, Dto.DatabaseUserDefinedType>();
        var schemaMapper = MapperRegistry.GetMapper<IDatabaseSchema, Dto.DatabaseSchema>();

        var (
            tables,
            views,
            sequences,
            synonyms,
            routines,
            userDefinedTypes,
            schemas
        ) = await (
            source.GetAllTables(cancellationToken),
            source.GetAllViews(cancellationToken),
            source.GetAllSequences(cancellationToken),
            source.GetAllSynonyms(cancellationToken),
            source.GetAllRoutines(cancellationToken),
            source.GetAllUserDefinedTypes(cancellationToken),
            source.GetAllSchemas(cancellationToken)
        ).WhenAll();

        var identifierDefaultsMapper = MapperRegistry.GetMapper<IIdentifierDefaults, Dto.IdentifierDefaults>();

        // shared across every table and view, so that an object referenced from several places,
        // such as a primary key that foreign keys in other tables point at, is mapped once
        var cache = new SerializedObjectCache();

        return new Dto.RelationalDatabase
        {
            IdentifierDefaults = identifierDefaultsMapper.Map(source.IdentifierDefaults),
            Tables = tables.Select(table => tableMapper.Map(table, cache)).ToList(),
            Views = views.Select(view => viewMapper.Map(view, cache)).ToList(),
            Sequences = sequenceMapper.MapList(sequences),
            Synonyms = synonymMapper.MapList(synonyms),
            Routines = routineMapper.MapList(routines),
            UserDefinedTypes = userDefinedTypeMapper.MapList(userDefinedTypes),
            Schemas = schemaMapper.MapList(schemas),
        };
    }
}