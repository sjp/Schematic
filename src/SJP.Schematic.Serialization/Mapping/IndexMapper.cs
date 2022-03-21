using Boxed.Mapping;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class IndexMapper
    : IImmutableMapper<Dto.DatabaseIndex, IDatabaseIndex>
    , IImmutableMapper<IDatabaseIndex, Dto.DatabaseIndex>
{
    public IDatabaseIndex Map(Dto.DatabaseIndex source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier?, Identifier>();
        var indexColumnMapper = MapperRegistry.GetMapper<Dto.DatabaseIndexColumn, IDatabaseIndexColumn>();
        var columnMapper = MapperRegistry.GetMapper<Dto.DatabaseColumn, IDatabaseColumn>();

        var indexName = identifierMapper.Map(source.IndexName);
        var indexColumns = indexColumnMapper.MapList(source.Columns);
        var includedColumns = columnMapper.MapList(source.IncludedColumns);

        return new DatabaseIndex(
            indexName,
            source.IsUnique,
            indexColumns,
            includedColumns,
            source.IsEnabled
        );
    }

    public Dto.DatabaseIndex Map(IDatabaseIndex source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var indexColumnMapper = MapperRegistry.GetMapper<IDatabaseIndexColumn, Dto.DatabaseIndexColumn>();
        var columnMapper = MapperRegistry.GetMapper<IDatabaseColumn, Dto.DatabaseColumn>();

        var indexName = identifierMapper.Map(source.Name);
        var indexColumns = indexColumnMapper.MapList(source.Columns);
        var includedColumns = columnMapper.MapList(source.IncludedColumns);

        return new Dto.DatabaseIndex
        {
            IndexName = indexName,
            Columns = indexColumns,
            IncludedColumns = includedColumns,
            IsEnabled = source.IsEnabled,
            IsUnique = source.IsUnique
        };
    }
}