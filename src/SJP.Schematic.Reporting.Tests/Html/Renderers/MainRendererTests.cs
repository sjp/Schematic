using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.Renderers;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.Renderers;

internal static class MainRendererTests
{
    [Test]
    public static async Task RenderAsync_GivenUnnamedUniqueKeysAndChecks_CountsEveryConstraint()
    {
        using var tempDir = new TemporaryDirectory();
        var column = Mock.Of<IDatabaseColumn>();

        var primaryKey = new DatabaseKey(Option<Identifier>.Some("pk_test"), DatabaseKeyType.Primary, [column], true);
        var uniqueKeys = new[]
        {
            new DatabaseKey(Option<Identifier>.None, DatabaseKeyType.Unique, [column], true),
            new DatabaseKey(Option<Identifier>.Some("uk_test"), DatabaseKeyType.Unique, [column], true),
        };
        var checks = new[]
        {
            new DatabaseCheckConstraint(Option<Identifier>.None, "a > 0", true),
            new DatabaseCheckConstraint(Option<Identifier>.Some("ck_test"), "b > 0", true),
        };
        var parentKey = new DatabaseRelationalKey(
            "test_table",
            new DatabaseKey(Option<Identifier>.None, DatabaseKeyType.Foreign, [column], true),
            "parent_table",
            new DatabaseKey(Option<Identifier>.Some("pk_parent"), DatabaseKeyType.Primary, [column], true),
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );

        var table = new RelationalDatabaseTable(
            "test_table",
            [column],
            Option<IDatabaseKey>.Some(primaryKey),
            uniqueKeys,
            [parentKey],
            [],
            [],
            checks,
            []
        );

        var main = await RenderMainAsync(tempDir, [table]);

        const int expectedConstraints = 1 + 2 + 1 + 2;
        Assert.That(main.GetProperty("constraintsCount").GetUInt32(), Is.EqualTo(expectedConstraints));
    }

    [Test]
    public static async Task RenderAsync_GivenIndexesWithoutDistinctNames_CountsEveryIndex()
    {
        using var tempDir = new TemporaryDirectory();
        var indexes = new[]
        {
            Mock.Of<IDatabaseIndex>(),
            Mock.Of<IDatabaseIndex>(),
        };

        var table = new RelationalDatabaseTable(
            "test_table",
            [],
            Option<IDatabaseKey>.None,
            [],
            [],
            [],
            indexes,
            [],
            []
        );

        var main = await RenderMainAsync(tempDir, [table]);

        Assert.That(main.GetProperty("indexesCount").GetUInt32(), Is.EqualTo(2));
    }

    private static async Task<JsonElement> RenderMainAsync(TemporaryDirectory tempDir, IReadOnlyCollection<IRelationalDatabaseTable> tables)
    {
        var renderer = new MainRenderer();
        var data = ReportDataFactory.Create(tables: tables);
        var context = new RenderContext(new JsonDataWriter(), new BundleBuilder(), new DirectoryInfo(tempDir.DirectoryPath));
        await renderer.RenderAsync(data, context);

        var outputFile = Path.Combine(tempDir.DirectoryPath, "data", "main.json");
        await using var stream = File.OpenRead(outputFile);
        using var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement.Clone();
    }
}
