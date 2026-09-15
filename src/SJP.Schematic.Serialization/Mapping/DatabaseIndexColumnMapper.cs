using System.Linq;
using Boxed.Mapping;
using LanguageExt;
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
    public IDatabaseIndexColumn Map(Dto.DatabaseIndexColumn source) => Map(source, ColumnLookup.Empty);

    internal IDatabaseIndexColumn Map(Dto.DatabaseIndexColumn source, ColumnLookup columns)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier?, Option<Identifier>>();
        var intOptionMapper = MapperRegistry.GetMapper<int?, Option<int>>();

        return new DatabaseIndexColumn(
            source.Expression,
            columns.ResolveList(source.DependentColumns),
            source.Order,
            source.NullOrder,
            identifierMapper.Map(source.Collation),
            intOptionMapper.Map(source.PrefixLength)
        );
    }

    /// <summary>
    /// Maps an index column to its serialized representation.
    /// </summary>
    /// <param name="source">An index column.</param>
    /// <returns>A serialized index column.</returns>
    public Dto.DatabaseIndexColumn Map(IDatabaseIndexColumn source) => Map(source, new SerializedObjectCache());

    internal Dto.DatabaseIndexColumn Map(IDatabaseIndexColumn source, SerializedObjectCache cache)
    {
        var columnMapper = (DatabaseColumnMapper)MapperRegistry.GetMapper<IDatabaseColumn, Dto.DatabaseColumn>();
        var identifierMapper = MapperRegistry.GetMapper<Option<Identifier>, Dto.Identifier?>();
        var intOptionMapper = MapperRegistry.GetMapper<Option<int>, int?>();

        return new Dto.DatabaseIndexColumn
        {
            Expression = source.Expression,
            DependentColumns = source.DependentColumns.Select(column => columnMapper.Map(column, cache)).ToList(),
            Order = source.Order,
            NullOrder = source.NullOrder,
            Collation = identifierMapper.Map(source.Collation),
            PrefixLength = intOptionMapper.Map(source.PrefixLength),
        };
    }
}