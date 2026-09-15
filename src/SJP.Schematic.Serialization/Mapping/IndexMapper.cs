using System.Linq;
using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a database index between its core and serialized representations.
/// </summary>
public class IndexMapper
    : IImmutableMapper<Dto.DatabaseIndex, IDatabaseIndex>
    , IImmutableMapper<IDatabaseIndex, Dto.DatabaseIndex>
{
    /// <summary>
    /// Maps a serialized index to its core representation.
    /// </summary>
    /// <param name="source">A serialized index.</param>
    /// <returns>An index.</returns>
    public IDatabaseIndex Map(Dto.DatabaseIndex source) => Map(source, ColumnLookup.Empty);

    internal IDatabaseIndex Map(Dto.DatabaseIndex source, ColumnLookup columns)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var indexColumnMapper = (DatabaseIndexColumnMapper)MapperRegistry.GetMapper<Dto.DatabaseIndexColumn, IDatabaseIndexColumn>();
        var optionMapper = MapperRegistry.GetMapper<string?, Option<string>>();
        var intOptionMapper = MapperRegistry.GetMapper<int?, Option<int>>();

        var indexName = identifierMapper.Map(source.IndexName);
        var indexColumns = source.Columns.Select(column => indexColumnMapper.Map(column, columns)).ToList();
        var includedColumns = columns.ResolveList(source.IncludedColumns);
        var filterDefinition = optionMapper.Map(source.FilterDefinition);

        return new DatabaseIndex(
            indexName,
            source.IsUnique,
            indexColumns,
            includedColumns,
            source.IsEnabled,
            filterDefinition,
            source.IndexType,
            intOptionMapper.Map(source.FillFactor),
            source.IsValid,
            source.IsVisible
        );
    }

    /// <summary>
    /// Maps an index to its serialized representation.
    /// </summary>
    /// <param name="source">An index.</param>
    /// <returns>A serialized index.</returns>
    public Dto.DatabaseIndex Map(IDatabaseIndex source) => Map(source, new SerializedObjectCache());

    internal Dto.DatabaseIndex Map(IDatabaseIndex source, SerializedObjectCache cache)
    {
        if (cache.Indexes.TryGetValue(source, out var cached))
            return cached;

        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var indexColumnMapper = (DatabaseIndexColumnMapper)MapperRegistry.GetMapper<IDatabaseIndexColumn, Dto.DatabaseIndexColumn>();
        var columnMapper = (DatabaseColumnMapper)MapperRegistry.GetMapper<IDatabaseColumn, Dto.DatabaseColumn>();
        var optionMapper = MapperRegistry.GetMapper<Option<string>, string?>();
        var intOptionMapper = MapperRegistry.GetMapper<Option<int>, int?>();

        var indexName = identifierMapper.Map(source.Name);
        var indexColumns = source.Columns.Select(column => indexColumnMapper.Map(column, cache)).ToList();
        var includedColumns = source.IncludedColumns.Select(column => columnMapper.Map(column, cache)).ToList();
        var filterDefinition = optionMapper.Map(source.FilterDefinition);

        var result = new Dto.DatabaseIndex
        {
            IndexName = indexName,
            Columns = indexColumns,
            IncludedColumns = includedColumns,
            IsEnabled = source.IsEnabled,
            IsUnique = source.IsUnique,
            FilterDefinition = filterDefinition,
            IndexType = source.IndexType,
            FillFactor = intOptionMapper.Map(source.FillFactor),
            IsValid = source.IsValid,
            IsVisible = source.IsVisible,
        };

        cache.Indexes.Add(source, result);
        return result;
    }
}