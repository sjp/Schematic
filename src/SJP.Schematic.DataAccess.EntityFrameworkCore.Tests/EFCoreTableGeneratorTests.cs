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

        return new DatabaseColumn(columnName, columnType, false, Option<IDatabaseDefaultValue>.None, Option<IAutoIncrement>.None);
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

    private static IRelationalDatabaseTable CreateTableWithColumnType(IDbType columnType)
    {
        var column = new DatabaseColumn("test_column", columnType, false, Option<IDatabaseDefaultValue>.None, Option<IAutoIncrement>.None);

        return new RelationalDatabaseTable(
            "test_table",
            [column],
            Option<IDatabaseKey>.None,
            [],
            [],
            [],
            [],
            [],
            []
        );
    }

    private static string GenerateForColumnType(IDbType columnType)
    {
        var table = CreateTableWithColumnType(columnType);
        return GetTableGenerator().Generate([table], table, Option<IRelationalDatabaseTableComments>.None);
    }

    private static ColumnDataType CreateColumnDataType(
        string typeName,
        DataType dataType,
        Type clrType,
        Option<INumericPrecision> numericPrecision = default,
        Option<int> fractionalSecondsPrecision = default
    ) => new(
        typeName,
        dataType,
        typeName,
        clrType,
        false,
        0,
        numericPrecision,
        Option<Identifier>.None,
        Option<IDbType>.None,
        [],
        Option<IDbType>.None,
        false,
        fractionalSecondsPrecision: fractionalSecondsPrecision
    );

    // a row version is maintained by the database and used for concurrency checks, which is exactly
    // what EF Core's timestamp annotation describes
    [Test]
    public static void Generate_GivenRowVersionColumn_GeneratesTimestampAttribute()
    {
        var result = GenerateForColumnType(CreateColumnDataType("rowversion", DataType.RowVersion, typeof(byte[])));

        Assert.That(result, Does.Contain("[Timestamp]"));
    }

    // EF Core already treats a string property as unicode, so only a non-unicode column is annotated
    [TestCase(DataType.String)]
    [TestCase(DataType.Text)]
    public static void Generate_GivenNonUnicodeStringColumn_GeneratesUnicodeAttribute(DataType dataType)
    {
        var result = GenerateForColumnType(CreateColumnDataType("varchar", dataType, typeof(string)));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Does.Contain("[Unicode(false)]"));
            Assert.That(result, Does.Contain("using Microsoft.EntityFrameworkCore;"));
        }
    }

    [TestCase(DataType.Unicode)]
    [TestCase(DataType.UnicodeText)]
    public static void Generate_GivenUnicodeStringColumn_GeneratesNoUnicodeAttribute(DataType dataType)
    {
        var result = GenerateForColumnType(CreateColumnDataType("nvarchar", dataType, typeof(string)));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Does.Not.Contain("[Unicode("));
            Assert.That(result, Does.Not.Contain("using Microsoft.EntityFrameworkCore;"));
        }
    }

    [TestCase(DataType.Numeric)]
    [TestCase(DataType.Money)]
    public static void Generate_GivenNumericColumnWithPrecision_GeneratesPrecisionAttribute(DataType dataType)
    {
        var precision = Option<INumericPrecision>.Some(new NumericPrecision(18, 4));
        var result = GenerateForColumnType(CreateColumnDataType("decimal", dataType, typeof(decimal), precision));

        Assert.That(result, Does.Contain("[Precision(18, 4)]"));
    }

    [Test]
    public static void Generate_GivenNumericColumnWithoutScale_GeneratesPrecisionAttributeWithoutScale()
    {
        var precision = Option<INumericPrecision>.Some(new NumericPrecision(18, 0));
        var result = GenerateForColumnType(CreateColumnDataType("decimal", DataType.Numeric, typeof(decimal), precision));

        Assert.That(result, Does.Contain("[Precision(18)]"));
    }

    [Test]
    public static void Generate_GivenNumericColumnWithoutPrecision_GeneratesNoPrecisionAttribute()
    {
        var result = GenerateForColumnType(CreateColumnDataType("decimal", DataType.Numeric, typeof(decimal)));

        Assert.That(result, Does.Not.Contain("[Precision("));
    }

    // the precision annotation describes a temporal property by the digits its seconds are kept to,
    // which is a single number rather than the precision and scale a numeric property carries
    [TestCase(DataType.DateTime, "datetime2")]
    [TestCase(DataType.DateTimeOffset, "datetimeoffset")]
    [TestCase(DataType.Time, "time")]
    public static void Generate_GivenTemporalColumnWithFractionalSecondsPrecision_GeneratesPrecisionAttribute(DataType dataType, string typeName)
    {
        var result = GenerateForColumnType(CreateColumnDataType(typeName, dataType, typeof(DateTime), fractionalSecondsPrecision: 3));

        Assert.That(result, Does.Contain("[Precision(3)]"));
    }

    [Test]
    public static void Generate_GivenTemporalColumnWithoutFractionalSecondsPrecision_GeneratesNoPrecisionAttribute()
    {
        var result = GenerateForColumnType(CreateColumnDataType("datetime", DataType.DateTime, typeof(DateTime)));

        Assert.That(result, Does.Not.Contain("[Precision("));
    }
}
