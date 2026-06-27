using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Tests;

[TestFixture]
internal static class SqliteDbTypeProviderTests
{
    // TODO
    [Test]
    public static void Ctor_GivenNoComparers_CreatesWithoutError()
    {
        Assert.That(() => new SqliteDbTypeProvider(), Throws.Nothing);
    }

    [Test]
    public static void CreateColumnType_GivenJsonDataType_ReturnsTextAffinityColumnType()
    {
        var provider = new SqliteDbTypeProvider();
        var columnType = provider.CreateColumnType(new ColumnTypeMetadata { DataType = DataType.Json });

        // SQLite has no dedicated JSON type; JSON is stored using text affinity.
        using (Assert.EnterMultipleScope())
        {
            Assert.That(columnType.TypeName.LocalName, Is.EqualTo("TEXT"));
            Assert.That(columnType.DataType, Is.EqualTo(DataType.UnicodeText));
        }
    }
}