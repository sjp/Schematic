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
    public IDatabaseIndex Map(Dto.DatabaseIndex source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var indexColumnMapper = MapperRegistry.GetMapper<Dto.DatabaseIndexColumn, IDatabaseIndexColumn>();
        var columnMapper = MapperRegistry.GetMapper<Dto.DatabaseColumn, IDatabaseColumn>();
        var optionMapper = MapperRegistry.GetMapper<string?, Option<string>>();

        var indexName = identifierMapper.Map(source.IndexName);
        var indexColumns = indexColumnMapper.MapList(source.Columns);
        var includedColumns = columnMapper.MapList(source.IncludedColumns);
        var filterDefinition = optionMapper.Map(source.FilterDefinition);

        return new DatabaseIndex(
            indexName,
            source.IsUnique,
            indexColumns,
            includedColumns,
            source.IsEnabled,
            filterDefinition
        );
    }

    /// <summary>
    /// Maps an index to its serialized representation.
    /// </summary>
    /// <param name="source">An index.</param>
    /// <returns>A serialized index.</returns>
    public Dto.DatabaseIndex Map(IDatabaseIndex source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var indexColumnMapper = MapperRegistry.GetMapper<IDatabaseIndexColumn, Dto.DatabaseIndexColumn>();
        var columnMapper = MapperRegistry.GetMapper<IDatabaseColumn, Dto.DatabaseColumn>();
        var optionMapper = MapperRegistry.GetMapper<Option<string>, string?>();

        var indexName = identifierMapper.Map(source.Name);
        var indexColumns = indexColumnMapper.MapList(source.Columns);
        var includedColumns = columnMapper.MapList(source.IncludedColumns);
        var filterDefinition = optionMapper.Map(source.FilterDefinition);

        return new Dto.DatabaseIndex
        {
            IndexName = indexName,
            Columns = indexColumns,
            IncludedColumns = includedColumns,
            IsEnabled = source.IsEnabled,
            IsUnique = source.IsUnique,
            FilterDefinition = filterDefinition,
        };
    }
}