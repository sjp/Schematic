using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Serialization;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Serialization;

[TestFixture]
internal static class JsonDataWriterTests
{
    [Test]
    public static void Serialize_GivenNullObject_ThrowsArgumentNullException()
    {
        var writer = new JsonDataWriter();
        Assert.That(() => writer.Serialize(null!), Throws.ArgumentNullException);
    }

    [Test]
    public static void Serialize_GivenViewModel_UsesCamelCasePropertyNames()
    {
        var writer = new JsonDataWriter();
        var synonym = new Synonym(
            new Identifier("test_synonym"),
            new Identifier("test_target"),
            Option<Uri>.Some(new Uri("#/tables/test-target-abcd1234", UriKind.Relative)));

        var json = writer.Serialize(synonym);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(json, Does.Contain("\"name\":"));
            Assert.That(json, Does.Contain("\"synonymUrl\":"));
            Assert.That(json, Does.Contain("\"targetName\":"));
            Assert.That(json, Does.Contain("\"targetUrl\":"));
            Assert.That(json, Does.Not.Contain("\"Name\":"));
        }
    }

    [Test]
    public static void Serialize_GivenViewModelWithUnresolvedOptionalProperty_OmitsNullProperty()
    {
        var writer = new JsonDataWriter();
        var synonym = new Synonym(
            new Identifier("test_synonym"),
            new Identifier("test_target"),
            Option<Uri>.None);

        var json = writer.Serialize(synonym);

        Assert.That(json, Does.Not.Contain("\"targetUrl\""));
    }

    [Test]
    public static void Serialize_GivenViewModelWithEnumProperty_WritesEnumAsString()
    {
        var writer = new JsonDataWriter();
        var column = new Columns.ColumnSummary(
            new Identifier("test_table"),
            Columns.ParentObjectType.Table,
            "#/tables/test-table-abcd1234",
            1,
            "test_column",
            "integer",
            false,
            Option<string>.None,
            false,
            false,
            false);
        var columns = new Columns([column]);

        var json = writer.Serialize(columns);

        Assert.That(json, Does.Contain("\"parentType\":\"Table\""));
    }

    [Test]
    public static void Serialize_GivenViewModel_ProducesUnindentedJson()
    {
        var writer = new JsonDataWriter();
        var synonym = new Synonym(new Identifier("a"), new Identifier("b"), Option<Uri>.None);

        var json = writer.Serialize(synonym);

        Assert.That(json, Does.Not.Contain("\n"));
    }

    [Test]
    public static void WriteJsonAsync_GivenNullFile_ThrowsArgumentNullException()
    {
        var writer = new JsonDataWriter();
        Assert.That(() => writer.WriteJsonAsync(null!, "{}"), Throws.ArgumentNullException);
    }

    [Test]
    public static void WriteJsonAsync_GivenNullJson_ThrowsArgumentNullException()
    {
        using var tempDir = new TemporaryDirectory();
        var file = new FileInfo(Path.Combine(tempDir.DirectoryPath, "test.json"));

        var writer = new JsonDataWriter();
        Assert.That(() => writer.WriteJsonAsync(file, null!), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task WriteJsonAsync_GivenJson_WritesExactStringToFile()
    {
        using var tempDir = new TemporaryDirectory();
        var file = new FileInfo(Path.Combine(tempDir.DirectoryPath, "test.json"));
        const string json = "{\"key\":\"value\"}";

        var writer = new JsonDataWriter();
        await writer.WriteJsonAsync(file, json);

        var content = await File.ReadAllTextAsync(file.FullName);
        Assert.That(content, Is.EqualTo(json));
    }

    [Test]
    public static async Task WriteJsonAsync_GivenMissingParentDirectory_CreatesDirectory()
    {
        using var tempDir = new TemporaryDirectory();
        var file = new FileInfo(Path.Combine(tempDir.DirectoryPath, "nested", "data", "test.json"));

        var writer = new JsonDataWriter();
        await writer.WriteJsonAsync(file, "{}");

        Assert.That(File.Exists(file.FullName), Is.True);
    }

    [Test]
    public static async Task WriteJsonAsync_WritesFileAsUtf8WithoutBom()
    {
        using var tempDir = new TemporaryDirectory();
        var file = new FileInfo(Path.Combine(tempDir.DirectoryPath, "test.json"));

        var writer = new JsonDataWriter();
        await writer.WriteJsonAsync(file, "{}");

        var bytes = await File.ReadAllBytesAsync(file.FullName);
        var bom = Encoding.UTF8.GetPreamble();

        var startsWithBom = bytes.Length >= bom.Length;
        for (var i = 0; startsWithBom && i < bom.Length; i++)
        {
            if (bytes[i] != bom[i])
                startsWithBom = false;
        }

        Assert.That(startsWithBom, Is.False);
    }
}
