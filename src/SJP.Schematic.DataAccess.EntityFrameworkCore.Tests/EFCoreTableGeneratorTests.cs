using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore.Tests;

[TestFixture]
internal static class EFCoreTableGeneratorTests
{
    private static IDatabaseTableGenerator GetTableGenerator() => new EFCoreTableGenerator(new MockFileSystem(), new VerbatimNameTranslator(), "SJP.Schematic.Test");

    [Test]
    public static void Ctor_GivenNullNameFileSystem_ThrowsArgumentNullException()
    {
        var nameTranslator = new VerbatimNameTranslator();
        Assert.That(() => new EFCoreTableGenerator(null, nameTranslator, "test"), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullNameTranslator_ThrowsArgumentNullException()
    {
        var fileSystem = new MockFileSystem();
        Assert.That(() => new EFCoreTableGenerator(fileSystem, null, "test"), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceNamespace_ThrowsArgumentNullException(string ns)
    {
        var fileSystem = new MockFileSystem();
        var nameTranslator = new VerbatimNameTranslator();

        Assert.That(() => new EFCoreTableGenerator(fileSystem, nameTranslator, ns), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetFilePath_GivenNullDirectory_ThrowsArgumentNullException()
    {
        var generator = GetTableGenerator();

        Assert.That(() => generator.GetFilePath(null, "test"), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetFilePath_GivenNullObjectName_ThrowsArgumentNullException()
    {
        var generator = GetTableGenerator();

        using var tempDir = new TemporaryDirectory();
        var baseDir = new DirectoryInfoWrapper(new FileSystem(), new DirectoryInfo(tempDir.DirectoryPath));

        Assert.That(() => generator.GetFilePath(baseDir, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetFilePath_GivenNameWithOnlyLocalName_ReturnsExpectedPath()
    {
        var generator = GetTableGenerator();
        using var tempDir = new TemporaryDirectory();
        var baseDir = new DirectoryInfoWrapper(new MockFileSystem(), new DirectoryInfo(tempDir.DirectoryPath));
        const string testTableName = "table_name";
        var expectedPath = Path.Combine(tempDir.DirectoryPath, "Tables", testTableName + ".cs");

        var filePath = generator.GetFilePath(baseDir, testTableName);

        Assert.That(filePath.FullName, Is.EqualTo(expectedPath));
    }

    [Test]
    public static void GetFilePath_GivenNameWithSchemaAndLocalName_ReturnsExpectedPath()
    {
        var generator = GetTableGenerator();
        using var tempDir = new TemporaryDirectory();
        var baseDir = new DirectoryInfoWrapper(new MockFileSystem(), new DirectoryInfo(tempDir.DirectoryPath));
        const string testTableSchema = "table_schema";
        const string testTableName = "table_name";
        var expectedPath = Path.Combine(tempDir.DirectoryPath, "Tables", testTableSchema, testTableName + ".cs");

        var filePath = generator.GetFilePath(baseDir, new Identifier(testTableSchema, testTableName));

        Assert.That(filePath.FullName, Is.EqualTo(expectedPath));
    }

    [Test]
    public static void Generate_GivenNullTables_ThrowsArgumentNullException()
    {
        var generator = GetTableGenerator();
        var table = Mock.Of<IRelationalDatabaseTable>();
        var comment = Option<IRelationalDatabaseTableComments>.None;

        Assert.That(() => generator.Generate(null, table, comment), Throws.ArgumentNullException);
    }

    [Test]
    public static void Generate_GivenNullTable_ThrowsArgumentNullException()
    {
        var generator = GetTableGenerator();
        var comment = Option<IRelationalDatabaseTableComments>.None;

        Assert.That(() => generator.Generate(Array.Empty<IRelationalDatabaseTable>(), null, comment), Throws.ArgumentNullException);
    }
}