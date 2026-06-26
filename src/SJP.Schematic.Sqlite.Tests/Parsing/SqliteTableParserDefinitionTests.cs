using System.Linq;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Parsing;

namespace SJP.Schematic.Sqlite.Tests.Parsing;

[TestFixture]
internal static class SqliteTableParserDefinitionTests
{
    private static ParsedTableData Parse(string definition) => new SqliteTableParser().Parse(definition);

    [Test]
    public static void Parse_GivenInlinePrimaryKey_ReturnsPrimaryKeyWithColumn()
    {
        var result = Parse("create table t (id integer primary key)");

        Assert.That(result.PrimaryKey.IsSome, Is.True);
        var pk = result.PrimaryKey.IfNone(() => null!);
        Assert.Multiple(() =>
        {
            Assert.That(pk.Name.IsNone, Is.True);
            Assert.That(pk.Columns.Select(c => c.Name), Is.EqualTo(new[] { "id" }));
        });
    }

    [Test]
    public static void Parse_GivenCompositeNamedPrimaryKey_ReturnsPrimaryKeyWithNameAndColumns()
    {
        var result = Parse("create table t (a int, b int, constraint pk_t primary key (a, b))");

        var pk = result.PrimaryKey.IfNone(() => null!);
        Assert.Multiple(() =>
        {
            Assert.That(pk.Name.IfNone(string.Empty), Is.EqualTo("pk_t"));
            Assert.That(pk.Columns.Select(c => c.Name), Is.EqualTo(new[] { "a", "b" }));
        });
    }

    [Test]
    public static void Parse_GivenInlineUniqueKey_ReturnsUnnamedUniqueKey()
    {
        var result = Parse("create table t (a int unique)");

        var uk = result.UniqueKeys.Single();
        Assert.Multiple(() =>
        {
            Assert.That(uk.Name.IsNone, Is.True);
            Assert.That(uk.Columns.Select(c => c.Name), Is.EqualTo(new[] { "a" }));
        });
    }

    [Test]
    public static void Parse_GivenNamedCompositeUniqueKey_ReturnsUniqueKeyWithNameAndColumns()
    {
        var result = Parse("create table t (a int, b int, constraint uq_t unique (a, b))");

        var uk = result.UniqueKeys.Single();
        Assert.Multiple(() =>
        {
            Assert.That(uk.Name.IfNone(string.Empty), Is.EqualTo("uq_t"));
            Assert.That(uk.Columns.Select(c => c.Name), Is.EqualTo(new[] { "a", "b" }));
        });
    }

    [Test]
    public static void Parse_GivenForeignKeyWithOnDeleteCascade_ReturnsForeignKeyDetails()
    {
        var result = Parse("create table t (a int, constraint fk_t foreign key (a) references p (b) on delete cascade)");

        var fk = result.ParentKeys.Single();
        Assert.Multiple(() =>
        {
            Assert.That(fk.Name.IfNone(string.Empty), Is.EqualTo("fk_t"));
            Assert.That(fk.Columns, Is.EqualTo(new[] { "a" }));
            Assert.That(fk.ParentTable.LocalName, Is.EqualTo("p"));
            Assert.That(fk.ParentColumns, Is.EqualTo(new[] { "b" }));
        });
    }

    [Test]
    public static void Parse_GivenMultiColumnForeignKey_ReturnsAllColumns()
    {
        var result = Parse("create table t (a int, b int, foreign key (a, b) references p (c, d))");

        var fk = result.ParentKeys.Single();
        Assert.Multiple(() =>
        {
            Assert.That(fk.Columns, Is.EqualTo(new[] { "a", "b" }));
            Assert.That(fk.ParentColumns, Is.EqualTo(new[] { "c", "d" }));
        });
    }

    [Test]
    public static void Parse_GivenInlineForeignKey_ReturnsForeignKey()
    {
        var result = Parse("create table t (a int references p (b))");

        var fk = result.ParentKeys.Single();
        Assert.Multiple(() =>
        {
            Assert.That(fk.Columns, Is.EqualTo(new[] { "a" }));
            Assert.That(fk.ParentTable.LocalName, Is.EqualTo("p"));
            Assert.That(fk.ParentColumns, Is.EqualTo(new[] { "b" }));
        });
    }

    [Test]
    public static void Parse_GivenGeneratedStoredColumn_ReturnsStoredComputedColumn()
    {
        var result = Parse("create table t (a int, b int generated always as (a * a) stored)");

        var column = result.Columns.Single(c => c.Name == "b");
        Assert.Multiple(() =>
        {
            Assert.That(column.ComputedColumnType, Is.EqualTo(SqliteGeneratedColumnType.Stored));
            Assert.That(column.ComputedDefinition, Is.EqualTo("(a * a)"));
        });
    }

    [Test]
    public static void Parse_GivenGeneratedColumnWithoutKeyword_ReturnsVirtualComputedColumn()
    {
        var result = Parse("create table t (a int, b int as (a * 2))");

        var column = result.Columns.Single(c => c.Name == "b");
        Assert.Multiple(() =>
        {
            Assert.That(column.ComputedColumnType, Is.EqualTo(SqliteGeneratedColumnType.Virtual));
            Assert.That(column.ComputedDefinition, Is.EqualTo("(a * 2)"));
        });
    }

    [TestCase("nocase", SqliteCollation.NoCase)]
    [TestCase("binary", SqliteCollation.Binary)]
    [TestCase("rtrim", SqliteCollation.Rtrim)]
    public static void Parse_GivenCollatedColumn_ReturnsExpectedCollation(string collation, SqliteCollation expected)
    {
        var result = Parse($"create table t (a text collate {collation})");

        var column = result.Columns.Single();
        Assert.That(column.Collation, Is.EqualTo(expected));
    }

    [Test]
    public static void Parse_GivenColumnWithoutCollation_ReturnsNoneCollation()
    {
        var result = Parse("create table t (a text)");

        var column = result.Columns.Single();
        Assert.That(column.Collation, Is.EqualTo(SqliteCollation.None));
    }

    [TestCase("create table t (a int default 5)", "5")]
    [TestCase("create table t (a text default 'x')", "'x'")]
    [TestCase("create table t (a text default current_timestamp)", "current_timestamp")]
    [TestCase("create table t (a int default (1 + 2))", "(1 + 2)")]
    public static void Parse_GivenColumnWithDefault_ReturnsDefaultValueText(string definition, string expected)
    {
        var result = Parse(definition);

        var column = result.Columns.Single();
        Assert.That(column.DefaultValue, Is.EqualTo(expected));
    }

    [Test]
    public static void Parse_GivenAutoIncrementColumn_ReturnsAutoIncrement()
    {
        var result = Parse("create table t (id integer primary key autoincrement)");

        var column = result.Columns.Single();
        Assert.That(column.IsAutoIncrement, Is.True);
    }

    [Test]
    public static void Parse_GivenNotNullColumn_ReturnsNotNullable()
    {
        var result = Parse("create table t (a int not null)");

        var column = result.Columns.Single();
        Assert.That(column.Nullable, Is.False);
    }

    [Test]
    public static void Parse_GivenWithoutRowidTable_ParsesColumnsAndPrimaryKey()
    {
        var result = Parse("create table t (a int primary key, b text) without rowid");

        Assert.Multiple(() =>
        {
            Assert.That(result.Columns.Select(c => c.Name), Is.EqualTo(new[] { "a", "b" }));
            Assert.That(result.PrimaryKey.IsSome, Is.True);
        });
    }

    [TestCase("create table t (a double precision)", "double precision")]
    [TestCase("create table t (a varchar(255))", "varchar(255)")]
    [TestCase("create table t (a decimal(10, 2))", "decimal(10, 2)")]
    public static void Parse_GivenColumnType_ReturnsOriginalTypeText(string definition, string expected)
    {
        var result = Parse(definition);

        var column = result.Columns.Single();
        Assert.That(column.TypeDefinition, Is.EqualTo(expected));
    }

    [Test]
    public static void Parse_GivenQuotedIdentifiers_ReturnsUnquotedColumnNames()
    {
        var result = Parse("create table t (\"order\" int, [Group] text, `value` int)");

        Assert.That(result.Columns.Select(c => c.Name), Is.EqualTo(new[] { "order", "Group", "value" }));
    }

    [Test]
    public static void Parse_GivenCheckConstraint_ReturnsDefinitionWithOriginalText()
    {
        var result = Parse("create table t (a int, constraint ck_t check (a > (1)))");

        var check = result.Checks.Single();
        Assert.Multiple(() =>
        {
            Assert.That(check.Name.IfNone(string.Empty), Is.EqualTo("ck_t"));
            Assert.That(check.Definition, Is.EqualTo("(a > (1))"));
        });
    }

    [Test]
    public static void Parse_GivenSelectBasedTable_ReturnsEmpty()
    {
        const string definition = "create table t as select 1 as x";
        var result = Parse(definition);

        Assert.Multiple(() =>
        {
            Assert.That(result.Columns, Is.Empty);
            Assert.That(result.Definition, Is.EqualTo(definition));
        });
    }
}
