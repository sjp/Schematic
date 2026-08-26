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
    public static void Ctor_GivenNullOrWhiteSpaceNamespace_ThrowsArgumentException(string ns)
    {
        var fileSystem = new MockFileSystem();
        var nameTranslator = new VerbatimNameTranslator();

        Assert.That(() => new EFCoreTableGenerator(fileSystem, nameTranslator, ns), Throws.InstanceOf<ArgumentException>());
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

        Assert.That(() => generator.Generate([], null, comment), Throws.ArgumentNullException);
    }

    [Test]
    public static void Generate_GivenParentKeyWithSameNameAsColumnProperty_GeneratesUniquelyNamedNavigationProperty()
    {
        var generator = GetTableGenerator();

        // The column name matches the class name, so its property name is suffixed to avoid the clash,
        // which in turn clashes with the navigation property name for the parent table.
        var column = CreateColumn("testtable");
        var parentKey = CreateRelationalKey("test table", "testtable_", column);
        var table = new RelationalDatabaseTable(
            "test table",
            [column],
            Option<IDatabaseKey>.None,
            [],
            [parentKey],
            [],
            [],
            [],
            []
        );

        var result = generator.Generate([table], table, Option<IRelationalDatabaseTableComments>.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Does.Contain("public string testtable_ { get; set; }"));
            Assert.That(result, Does.Contain("public virtual testtable_ testtable__1 { get; set; }"));
        }
    }

    [Test]
    public static void Generate_GivenSelfReferencingParentKey_GeneratesNavigationPropertyNotMatchingClassName()
    {
        var generator = GetTableGenerator();

        var column = CreateColumn("test_column");
        var parentKey = CreateRelationalKey("test_table", "test_table", column);
        var table = new RelationalDatabaseTable(
            "test_table",
            [column],
            Option<IDatabaseKey>.None,
            [],
            [parentKey],
            [],
            [],
            [],
            []
        );

        var result = generator.Generate([table], table, Option<IRelationalDatabaseTableComments>.None);

        Assert.That(result, Does.Contain("public virtual test_table test_table_1 { get; set; }"));
    }

    private static IDatabaseColumn CreateColumn(Identifier columnName)
    {
        var columnType = new ColumnDataType(
            "varchar",
            DataType.String,
            "varchar(50)",
            typeof(string),
            false,
            50,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );

        return new DatabaseColumn(columnName, columnType, false, Option<string>.None, Option<IAutoIncrement>.None);
    }

    private static IDatabaseRelationalKey CreateRelationalKey(Identifier childTableName, Identifier parentTableName, IDatabaseColumn childColumn) =>
        new DatabaseRelationalKey(
            childTableName,
            new DatabaseKey(Option<Identifier>.Some("test_child_key"), DatabaseKeyType.Foreign, [childColumn], true),
            parentTableName,
            new DatabaseKey(Option<Identifier>.Some("test_parent_key"), DatabaseKeyType.Primary, [childColumn], true),
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );
}