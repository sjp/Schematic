using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests;

internal static class SqlServerCatalogMapperTests
{
    [Test]
    public static void MapColumnValueSource_GivenNeitherComputedNorIdentity_ReturnsStored()
    {
        var result = SqlServerCatalogMapper.MapColumnValueSource(false, null, null, false, null, null);

        Assert.That(result, Is.EqualTo((ColumnValueSource)new ColumnValueSource.Stored()));
    }

    [TestCase(true, ComputedColumnStorage.Stored)]
    [TestCase(false, ComputedColumnStorage.Virtual)]
    [TestCase(null, ComputedColumnStorage.Virtual)]
    public static void MapColumnValueSource_GivenComputedColumn_ReturnsComputedWithDefinitionAndStorage(bool? isPersisted, ComputedColumnStorage expectedStorage)
    {
        var result = SqlServerCatalogMapper.MapColumnValueSource(true, "([a]+[b])", isPersisted, false, null, null);

        var expected = new ColumnValueSource.Computed(Option<string>.Some("([a]+[b])"), expectedStorage);
        Assert.That(result, Is.EqualTo((ColumnValueSource)expected));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public static void MapColumnValueSource_GivenComputedColumnWithoutDefinition_ReturnsComputedWithoutDefinition(string definition)
    {
        var result = SqlServerCatalogMapper.MapColumnValueSource(true, definition, false, false, null, null);

        Assert.That(result.UnwrapComputed().Definition, OptionIs.None);
    }

    [Test]
    public static void MapColumnValueSource_GivenIdentityColumn_ReturnsGeneratedWithSeedAndIncrement()
    {
        var result = SqlServerCatalogMapper.MapColumnValueSource(false, null, null, true, 10, 5);

        var identity = result.UnwrapIdentity();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(identity.InitialValue, Is.EqualTo(10));
            Assert.That(identity.Increment, Is.EqualTo(5));
        }
    }

    [TestCase(null)]
    [TestCase(0)]
    public static void MapColumnValueSource_GivenIdentityColumnWithoutSeedOrIncrement_ReturnsGeneratedWithDefaults(int? increment)
    {
        var result = SqlServerCatalogMapper.MapColumnValueSource(false, null, null, true, null, increment);

        var identity = result.UnwrapIdentity();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(identity.InitialValue, Is.EqualTo(1));
            Assert.That(identity.Increment, Is.EqualTo(1));
        }
    }
}
