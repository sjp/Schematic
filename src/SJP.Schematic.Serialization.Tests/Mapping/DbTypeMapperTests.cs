using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Serialization.Mapping;

namespace SJP.Schematic.Serialization.Tests.Mapping;

internal static class DbTypeMapperTests
{
    private static IDbType ColumnType { get; } = new ColumnDataType(
        "varchar",
        DataType.Unicode,
        "varchar(255)",
        typeof(string),
        false,
        255,
        Option<INumericPrecision>.None,
        Option<Identifier>.None
    );

    [Test]
    public static void Map_GivenEqualDocumentsTwice_ReturnsSameInstance()
    {
        var mapper = new DbTypeMapper();

        var first = mapper.Map(mapper.Map(ColumnType));
        var second = mapper.Map(mapper.Map(ColumnType));

        Assert.That(second, Is.SameAs(first));
    }

    [Test]
    public static void Map_GivenDocumentsDifferingInClrTypeName_ReturnsDifferentTypes()
    {
        var mapper = new DbTypeMapper();
        var known = mapper.Map(ColumnType);
        var unknown = known with { ClrTypeName = "Example.UnloadedType" };

        var knownType = mapper.Map(known);
        var unknownType = mapper.Map(unknown);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(unknownType, Is.Not.SameAs(knownType));
            Assert.That(knownType.ClrTypeName, Is.EqualTo("System.String"));
            Assert.That(unknownType.ClrTypeName, Is.EqualTo("Example.UnloadedType"));
            Assert.That(unknownType.ClrType, Is.EqualTo(typeof(object)));
        }
    }
}
