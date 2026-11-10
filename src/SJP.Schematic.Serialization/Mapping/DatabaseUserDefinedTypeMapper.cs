using System;
using System.Linq;
using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a user-defined type between its core and serialized representations.
/// </summary>
/// <remarks>
/// <para>
/// The serialized form records the kind of type in <see cref="Dto.DatabaseUserDefinedType.Kind"/>
/// and keeps one field for every member any kind of type has, so it is unchanged by the kinds of
/// type being separate cases of <see cref="DatabaseUserDefinedType"/>. The base type field holds
/// the aliased type of an alias or domain, the subtype of a range and the element type of a
/// collection, and the definition field holds the external name of a CLR type.
/// </para>
/// <para>
/// A kind of type that declares no nullability rule is written as nullable, and the fields that a
/// kind does not have are written empty. When reading, the fields that a kind does not have are
/// ignored.
/// </para>
/// </remarks>
public class DatabaseUserDefinedTypeMapper
    : IImmutableMapper<Dto.DatabaseUserDefinedType, DatabaseUserDefinedType>
    , IImmutableMapper<DatabaseUserDefinedType, Dto.DatabaseUserDefinedType>
{
    /// <summary>
    /// Maps a serialized user-defined type to its core representation.
    /// </summary>
    /// <param name="source">A serialized user-defined type.</param>
    /// <returns>A user-defined type of the kind <see cref="Dto.DatabaseUserDefinedType.Kind"/> records.</returns>
    /// <exception cref="ArgumentException"><see cref="Dto.DatabaseUserDefinedType.Kind"/> of <paramref name="source"/> is an invalid enum value.</exception>
    /// <remarks>
    /// An alias, domain, range or collection written without a base type, which no dialect
    /// produces, reads back as an unknown type rather than failing, as the base type is what
    /// defines each of them.
    /// </remarks>
    public DatabaseUserDefinedType Map(Dto.DatabaseUserDefinedType source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var optionalMapper = MapperRegistry.GetMapper<string?, Option<string>>();
        var dbTypeMapper = MapperRegistry.GetMapper<Dto.DbType?, Option<IDbType>>();

        var name = identifierMapper.Map(source.TypeName);
        var definition = optionalMapper.Map(source.Definition);

        return source.Kind switch
        {
            Dto.UserDefinedTypeKind.Unknown => new DatabaseUserDefinedType.Unknown(name, definition),
            Dto.UserDefinedTypeKind.Alias => dbTypeMapper.Map(source.BaseType).Match<DatabaseUserDefinedType>(
                aliasedType => new DatabaseUserDefinedType.Alias(name, aliasedType, source.IsNullable, optionalMapper.Map(source.DefaultValue)),
                () => new DatabaseUserDefinedType.Unknown(name, definition)),
            Dto.UserDefinedTypeKind.Domain => dbTypeMapper.Map(source.BaseType).Match<DatabaseUserDefinedType>(
                aliasedType => new DatabaseUserDefinedType.Domain(name, aliasedType, source.IsNullable, optionalMapper.Map(source.DefaultValue), MapChecks(source)),
                () => new DatabaseUserDefinedType.Unknown(name, definition)),
            Dto.UserDefinedTypeKind.Enum => new DatabaseUserDefinedType.Enum(name, source.EnumValues.ToList()),
            Dto.UserDefinedTypeKind.Composite => new DatabaseUserDefinedType.Composite(name, MapAttributes(source), definition),
            Dto.UserDefinedTypeKind.Range => dbTypeMapper.Map(source.BaseType).Match<DatabaseUserDefinedType>(
                subtype => new DatabaseUserDefinedType.Range(name, subtype),
                () => new DatabaseUserDefinedType.Unknown(name, definition)),
            Dto.UserDefinedTypeKind.Table => new DatabaseUserDefinedType.Table(name, MapAttributes(source), MapChecks(source)),
            Dto.UserDefinedTypeKind.Clr => new DatabaseUserDefinedType.Clr(name, definition),
            Dto.UserDefinedTypeKind.Collection => dbTypeMapper.Map(source.BaseType).Match<DatabaseUserDefinedType>(
                elementType => new DatabaseUserDefinedType.Collection(name, elementType, definition),
                () => new DatabaseUserDefinedType.Unknown(name, definition)),
            _ => throw new ArgumentException($"The {nameof(Dto.UserDefinedTypeKind)} provided must be a valid enum.", nameof(source)),
        };
    }

    private static IDatabaseColumn[] MapAttributes(Dto.DatabaseUserDefinedType source)
    {
        var columnMapper = MapperRegistry.GetMapper<Dto.DatabaseColumn, IDatabaseColumn>();
        return source.Attributes.Select(columnMapper.Map).ToArray();
    }

    private static IDatabaseCheckConstraint[] MapChecks(Dto.DatabaseUserDefinedType source)
    {
        var checkMapper = MapperRegistry.GetMapper<Dto.DatabaseCheckConstraint, IDatabaseCheckConstraint>();
        return source.Checks.Select(checkMapper.Map).ToArray();
    }

    /// <summary>
    /// Maps a user-defined type to its serialized representation.
    /// </summary>
    /// <param name="source">A user-defined type.</param>
    /// <returns>A serialized user-defined type.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> holds no type.</exception>
    public Dto.DatabaseUserDefinedType Map(DatabaseUserDefinedType source)
    {
        if (source.Value is null)
            throw new ArgumentNullException(nameof(source));

        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var optionalMapper = MapperRegistry.GetMapper<Option<string>, string?>();
        var dbTypeMapper = MapperRegistry.GetMapper<IDbType, Dto.DbType>();
        var columnMapper = MapperRegistry.GetMapper<IDatabaseColumn, Dto.DatabaseColumn>();
        var checkMapper = MapperRegistry.GetMapper<IDatabaseCheckConstraint, Dto.DatabaseCheckConstraint>();

        var result = new Dto.DatabaseUserDefinedType
        {
            TypeName = identifierMapper.Map(source.Name),
            IsNullable = true,
        };

        return source switch
        {
            DatabaseUserDefinedType.Unknown type => result with
            {
                Kind = Dto.UserDefinedTypeKind.Unknown,
                Definition = optionalMapper.Map(type.Definition),
            },
            DatabaseUserDefinedType.Alias type => result with
            {
                Kind = Dto.UserDefinedTypeKind.Alias,
                BaseType = dbTypeMapper.Map(type.AliasedType),
                IsNullable = type.IsNullable,
                DefaultValue = optionalMapper.Map(type.DefaultValue),
            },
            DatabaseUserDefinedType.Domain type => result with
            {
                Kind = Dto.UserDefinedTypeKind.Domain,
                BaseType = dbTypeMapper.Map(type.AliasedType),
                Checks = type.Checks.Select(checkMapper.Map).ToList(),
                IsNullable = type.IsNullable,
                DefaultValue = optionalMapper.Map(type.DefaultValue),
            },
            DatabaseUserDefinedType.Enum type => result with
            {
                Kind = Dto.UserDefinedTypeKind.Enum,
                EnumValues = type.Values.ToList(),
            },
            DatabaseUserDefinedType.Composite type => result with
            {
                Kind = Dto.UserDefinedTypeKind.Composite,
                Attributes = type.Attributes.Select(columnMapper.Map).ToList(),
                Definition = optionalMapper.Map(type.Definition),
            },
            DatabaseUserDefinedType.Range type => result with
            {
                Kind = Dto.UserDefinedTypeKind.Range,
                BaseType = dbTypeMapper.Map(type.Subtype),
            },
            DatabaseUserDefinedType.Table type => result with
            {
                Kind = Dto.UserDefinedTypeKind.Table,
                Attributes = type.Columns.Select(columnMapper.Map).ToList(),
                Checks = type.Checks.Select(checkMapper.Map).ToList(),
            },
            DatabaseUserDefinedType.Clr type => result with
            {
                Kind = Dto.UserDefinedTypeKind.Clr,
                Definition = optionalMapper.Map(type.ExternalName),
            },
            DatabaseUserDefinedType.Collection type => result with
            {
                Kind = Dto.UserDefinedTypeKind.Collection,
                BaseType = dbTypeMapper.Map(type.ElementType),
                Definition = optionalMapper.Map(type.Definition),
            },
        };
    }
}
