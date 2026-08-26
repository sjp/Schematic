using Boxed.Mapping;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps an index column between its core and serialized representations.
/// </summary>
public class DatabaseIndexColumnMapper
    : IImmutableMapper<Dto.DatabaseIndexColumn, IDatabaseIndexColumn>
    , IImmutableMapper<IDatabaseIndexColumn, Dto.DatabaseIndexColumn>
{
    /// <summary>
    /// Maps a serialized index column to its core representation.
    /// </summary>
    /// <param name="source">A serialized index column.</param>
    /// <returns>An index column.</returns>
    public IDatabaseIndexColumn Map(Dto.DatabaseIndexColumn source)
    {
        var columnMapper = MapperRegistry.GetMapper<Dto.DatabaseColumn, IDatabaseColumn>();

        return new DatabaseIndexColumn(
            source.Expression,
            columnMapper.MapList(source.DependentColumns),
            source.Order
        );
    }

    /// <summary>
    /// Maps an index column to its serialized representation.
    /// </summary>
    /// <param name="source">An index column.</param>
    /// <returns>A serialized index column.</returns>
    public Dto.DatabaseIndexColumn Map(IDatabaseIndexColumn source)
    {
        var columnMapper = MapperRegistry.GetMapper<IDatabaseColumn, Dto.DatabaseColumn>();

        return new Dto.DatabaseIndexColumn
        {
            Expression = source.Expression,
            DependentColumns = columnMapper.MapList(source.DependentColumns),
            Order = source.Order,
        };
    }
}