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

namespace SJP.Schematic.DataAccess.OrmLite.Tests;

[TestFixture]
internal static class OrmLiteTableGeneratorTests
{
    private static IDatabaseTableGenerator GetTableGenerator() => new OrmLiteTableGenerator(new MockFileSystem(), new VerbatimNameTranslator(), "SJP.Schematic.Test");

    [Test]
    public static void Ctor_GivenNullFileSystem_ThrowsArgumentNullException()
    {
        Assert.That(() => new OrmLiteTableGenerator(null, new VerbatimNameTranslator(), "test"), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullNameTranslator_ThrowsArgumentNullException()
    {
        Assert.That(() => new OrmLiteTableGenerator(new MockFileSystem(), null, "test"), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpaceNamespace_ThrowsArgumentException(string ns)
    {
        var fileSystem = new MockFileSystem();
        var nameTranslator = new VerbatimNameTranslator();
        Assert.That(() => new OrmLiteTableGenerator(fileSystem, nameTranslator, ns), Throws.InstanceOf<ArgumentException>());
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
        var baseDir = new DirectoryInfoWrapper(new FileSystem(), new DirectoryInfo(tempDir.DirectoryPath));
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
        var baseDir = new DirectoryInfoWrapper(new FileSystem(), new DirectoryInfo(tempDir.DirectoryPath));
        const string testTableSchema = "table_schema";
        const string testTableName = "table_name";
        var expectedPath = Path.Combine(tempDir.DirectoryPath, "Tables", testTableSchema, testTableName + ".cs");

        var filePath = generator.GetFilePath(baseDir, new Identifier(testTableSchema, testTableName));

        Assert.That(filePath.FullName, Is.EqualTo(expectedPath));
    }

    [Test]
    public static void Generate_GivenNullDatabase_ThrowsArgumentNullException()
    {
        var generator = GetTableGenerator();
        var table = Mock.Of<IRelationalDatabaseTable>();

        Assert.That(() => generator.Generate(null, table, Option<IRelationalDatabaseTableComments>.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void Generate_GivenNullTable_ThrowsArgumentNullException()
    {
        var generator = GetTableGenerator();

        Assert.That(() => generator.Generate([], null, Option<IRelationalDatabaseTableComments>.None), Throws.ArgumentNullException);
    }
}