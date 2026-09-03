using System.Linq;
using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a user-defined type between its core and serialized representations.
/// </summary>
public class DatabaseUserDefinedTypeMapper
    : IImmutableMapper<Dto.DatabaseUserDefinedType, IDatabaseUserDefinedType>
    , IImmutableMapper<IDatabaseUserDefinedType, Dto.DatabaseUserDefinedType>
{
    /// <summary>
    /// Maps a serialized user-defined type to its core representation.
    /// </summary>
    /// <param name="source">A serialized user-defined type.</param>
    /// <returns>A user-defined type.</returns>
    public IDatabaseUserDefinedType Map(Dto.DatabaseUserDefinedType source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var optionalMapper = MapperRegistry.GetMapper<string?, Option<string>>();
        var dbTypeMapper = MapperRegistry.GetMapper<Dto.DbType?, Option<IDbType>>();
        var columnMapper = MapperRegistry.GetMapper<Dto.DatabaseColumn, IDatabaseColumn>();
        var checkMapper = MapperRegistry.GetMapper<Dto.DatabaseCheckConstraint, IDatabaseCheckConstraint>();

        return new DatabaseUserDefinedType(
            identifierMapper.Map(source.TypeName),
            source.Kind,
            dbTypeMapper.Map(source.BaseType),
            source.EnumValues.ToList(),
            source.Attributes.Select(columnMapper.Map).ToList(),
            source.Checks.Select(checkMapper.Map).ToList(),
            source.IsNullable,
            optionalMapper.Map(source.DefaultValue),
            optionalMapper.Map(source.Definition)
        );
    }

    /// <summary>
    /// Maps a user-defined type to its serialized representation.
    /// </summary>
    /// <param name="source">A user-defined type.</param>
    /// <returns>A serialized user-defined type.</returns>
    public Dto.DatabaseUserDefinedType Map(IDatabaseUserDefinedType source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var optionalMapper = MapperRegistry.GetMapper<Option<string>, string?>();
        var dbTypeMapper = MapperRegistry.GetMapper<Option<IDbType>, Dto.DbType?>();
        var columnMapper = MapperRegistry.GetMapper<IDatabaseColumn, Dto.DatabaseColumn>();
        var checkMapper = MapperRegistry.GetMapper<IDatabaseCheckConstraint, Dto.DatabaseCheckConstraint>();

        return new Dto.DatabaseUserDefinedType
        {
            TypeName = identifierMapper.Map(source.Name),
            Kind = source.Kind,
            BaseType = dbTypeMapper.Map(source.BaseType),
            EnumValues = source.EnumValues.ToList(),
            Attributes = source.Attributes.Select(columnMapper.Map).ToList(),
            Checks = source.Checks.Select(checkMapper.Map).ToList(),
            IsNullable = source.IsNullable,
            DefaultValue = optionalMapper.Map(source.DefaultValue),
            Definition = optionalMapper.Map(source.Definition),
        };
    }
}
