using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.Renderers;

internal static class TableRendererTests
{
    private const int ColumnsPerTable = 15;

    [Test]
    public static async Task RenderAsync_GivenHubTableWithManyChildren_TableDetailPayloadsGrowLinearly()
    {
        var smallSize = await GetTotalTableDetailBytesAsync(childCount: 250);
        var largeSize = await GetTotalTableDetailBytesAsync(childCount: 500);

        TestContext.Out.WriteLine($"Table detail payloads: 250 children = {smallSize:N0} bytes, 500 children = {largeSize:N0} bytes");

        // Doubling the number of children should roughly double the output. A payload that embeds
        // each table's neighbourhood would quadruple it instead, because every child of the hub is
        // within two relationships of every other child.
        Assert.That((double)largeSize / smallSize, Is.LessThan(2.5));
    }

    [Test]
    public static async Task RenderAsync_GivenChildOfHubTable_DoesNotEmbedRelatedTables()
    {
        using var tempDir = new TemporaryDirectory();
        var tables = CreateHubSchema(childCount: 3);

        await RenderTablesAsync(tempDir, tables);

        var childKey = Identifier.CreateQualifiedIdentifier("test_schema", "child_0").ToSafeKey();
        var content = await File.ReadAllTextAsync(Path.Combine(tempDir.DirectoryPath, "data", "tables", childKey + ".json"));

        Assert.That(content, Does.Not.Contain("child_1"));
    }

    private static async Task<long> GetTotalTableDetailBytesAsync(int childCount)
    {
        using var tempDir = new TemporaryDirectory();
        var tables = CreateHubSchema(childCount);

        await RenderTablesAsync(tempDir, tables);

        var tablesDirectory = new DirectoryInfo(Path.Combine(tempDir.DirectoryPath, "data", "tables"));
        return tablesDirectory.EnumerateFiles("*.json").Sum(static f => f.Length);
    }

    private static Task RenderTablesAsync(TemporaryDirectory tempDir, IReadOnlyCollection<IRelationalDatabaseTable> tables)
    {
        var renderer = new TableRenderer();
        var data = ReportDataFactory.Create(tables: tables);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        return renderer.RenderAsync(data, context);
    }

    // One "hub" table that every other table references, as with a users or tenants table.
    private static IReadOnlyCollection<IRelationalDatabaseTable> CreateHubSchema(int childCount)
    {
        var hubName = Identifier.CreateQualifiedIdentifier("test_schema", "hub");
        var hubColumns = CreateColumns("hub_id");
        var hubPrimaryKey = new DatabaseKey(Option<Identifier>.Some("pk_hub"), DatabaseKeyType.Primary, [hubColumns[0]], true);

        var children = new List<(Identifier Name, IReadOnlyList<IDatabaseColumn> Columns, IDatabaseKey PrimaryKey, IDatabaseRelationalKey ParentKey)>();
        for (var i = 0; i < childCount; i++)
        {
            var childName = Identifier.CreateQualifiedIdentifier("test_schema", "child_" + i);
            var childColumns = CreateColumns("child_id", "hub_id");
            var childPrimaryKey = new DatabaseKey(Option<Identifier>.Some("pk_child_" + i), DatabaseKeyType.Primary, [childColumns[0]], true);
            var foreignKey = new DatabaseKey(Option<Identifier>.Some("fk_child_" + i + "_hub"), DatabaseKeyType.Foreign, [childColumns[1]], true);
            var relationalKey = new DatabaseRelationalKey(childName, foreignKey, hubName, hubPrimaryKey, ReferentialAction.NoAction, ReferentialAction.NoAction);

            children.Add((childName, childColumns, childPrimaryKey, relationalKey));
        }

        var hub = new RelationalDatabaseTable(
            hubName,
            hubColumns,
            hubPrimaryKey,
            [],
            [],
            children.ConvertAll(static c => c.ParentKey),
            [],
            [],
            []
        );

        var tables = new List<IRelationalDatabaseTable> { hub };
        tables.AddRange(children.Select(static c => new RelationalDatabaseTable(
            c.Name,
            c.Columns,
            Option<IDatabaseKey>.Some(c.PrimaryKey),
            [],
            [c.ParentKey],
            [],
            [],
            [],
            []
        )));

        return tables;
    }

    private static IReadOnlyList<IDatabaseColumn> CreateColumns(params string[] keyColumnNames)
    {
        var columns = new List<IDatabaseColumn>();
        columns.AddRange(keyColumnNames.Select(static name => new DatabaseColumn(name, TestDbTypes.BigInteger, false, null, null)));

        for (var i = columns.Count; i < ColumnsPerTable; i++)
            columns.Add(new DatabaseColumn("attribute_column_" + i, TestDbTypes.BigInteger, true, null, null));

        return columns;
    }
}
