using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class RelationalDatabaseTableMapper
    : IImmutableMapper<Dto.RelationalDatabaseTable, IRelationalDatabaseTable>
    , IImmutableMapper<IRelationalDatabaseTable, Dto.RelationalDatabaseTable>
{
    public IRelationalDatabaseTable Map(Dto.RelationalDatabaseTable source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var columnMapper = MapperRegistry.GetMapper<Dto.DatabaseColumn, IDatabaseColumn>();
        var optionalKeyMapper = MapperRegistry.GetMapper<Dto.DatabaseKey?, Option<IDatabaseKey>>();
        var keyMapper = MapperRegistry.GetMapper<Dto.DatabaseKey, IDatabaseKey>();
        var relationalKeyMapper = MapperRegistry.GetMapper<Dto.DatabaseRelationalKey, IDatabaseRelationalKey>();
        var indexMapper = MapperRegistry.GetMapper<Dto.DatabaseIndex, IDatabaseIndex>();
        var checkMapper = MapperRegistry.GetMapper<Dto.DatabaseCheckConstraint, IDatabaseCheckConstraint>();
        var triggerMapper = MapperRegistry.GetMapper<Dto.DatabaseTrigger, IDatabaseTrigger>();

        return new RelationalDatabaseTable(
            identifierMapper.Map<Dto.Identifier, Identifier>(source.TableName!),
            columnMapper.MapList(source.Columns),
            optionalKeyMapper.Map(source.PrimaryKey),
            keyMapper.MapList(source.UniqueKeys),
            relationalKeyMapper.MapList(source.ParentKeys),
            relationalKeyMapper.MapList(source.ChildKeys),
            indexMapper.MapList(source.Indexes),
            checkMapper.MapList(source.Checks),
            triggerMapper.MapList(source.Triggers)
        );
    }

    public Dto.RelationalDatabaseTable Map(IRelationalDatabaseTable source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var columnMapper = MapperRegistry.GetMapper<IDatabaseColumn, Dto.DatabaseColumn>();
        var optionalKeyMapper = MapperRegistry.GetMapper<Option<IDatabaseKey>, Dto.DatabaseKey?>();
        var keyMapper = MapperRegistry.GetMapper<IDatabaseKey, Dto.DatabaseKey>();
        var relationalKeyMapper = MapperRegistry.GetMapper<IDatabaseRelationalKey, Dto.DatabaseRelationalKey>();
        var indexMapper = MapperRegistry.GetMapper<IDatabaseIndex, Dto.DatabaseIndex>();
        var checkMapper = MapperRegistry.GetMapper<IDatabaseCheckConstraint, Dto.DatabaseCheckConstraint>();
        var triggerMapper = MapperRegistry.GetMapper<IDatabaseTrigger, Dto.DatabaseTrigger>();

        return new Dto.RelationalDatabaseTable
        {
            TableName = identifierMapper.Map(source.Name),
            Columns = columnMapper.MapList(source.Columns),
            PrimaryKey = optionalKeyMapper.Map(source.PrimaryKey),
            UniqueKeys = keyMapper.MapList(source.UniqueKeys),
            ParentKeys = relationalKeyMapper.MapList(source.ParentKeys),
            ChildKeys = relationalKeyMapper.MapList(source.ChildKeys),
            Indexes = indexMapper.MapList(source.Indexes),
            Checks = checkMapper.MapList(source.Checks),
            Triggers = triggerMapper.MapList(source.Triggers)
        };
    }
}
