using System.Linq;
using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a database trigger between its core and serialized representations.
/// </summary>
public class DatabaseTriggerMapper
    : IImmutableMapper<Dto.DatabaseTrigger, IDatabaseTrigger>
    , IImmutableMapper<IDatabaseTrigger, Dto.DatabaseTrigger>
{
    /// <summary>
    /// Maps a serialized trigger to its core representation.
    /// </summary>
    /// <param name="source">A serialized trigger.</param>
    /// <returns>A trigger.</returns>
    public IDatabaseTrigger Map(Dto.DatabaseTrigger source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var optionalMapper = MapperRegistry.GetMapper<string?, Option<string>>();

        return new DatabaseTrigger(
            identifierMapper.Map(source.TriggerName),
            source.Definition,
            source.QueryTiming,
            source.TriggerEvent,
            source.IsEnabled,
            source.Granularity,
            optionalMapper.Map(source.Condition),
            source.UpdateColumns.Select(identifierMapper.Map).ToList()
        );
    }

    /// <summary>
    /// Maps a trigger to its serialized representation.
    /// </summary>
    /// <param name="source">A trigger.</param>
    /// <returns>A serialized trigger.</returns>
    public Dto.DatabaseTrigger Map(IDatabaseTrigger source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var optionalMapper = MapperRegistry.GetMapper<Option<string>, string?>();

        return new Dto.DatabaseTrigger
        {
            TriggerName = identifierMapper.Map(source.Name),
            Definition = source.Definition,
            QueryTiming = source.QueryTiming,
            TriggerEvent = source.TriggerEvent,
            IsEnabled = source.IsEnabled,
            Granularity = source.Granularity,
            Condition = optionalMapper.Map(source.Condition),
            UpdateColumns = source.UpdateColumns.Select(identifierMapper.Map).ToList(),
        };
    }
}