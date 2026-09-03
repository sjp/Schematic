using System;
using System.Collections.Generic;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite.Parsing;

namespace SJP.Schematic.Sqlite.Tests.Parsing;

[TestFixture]
internal static class ParsedModelGuardTests
{
    [Test]
    public static void PrimaryKeyCtor_GivenNullColumns_ThrowsArgumentNullException()
    {
        Assert.That(() => new PrimaryKey(Option<string>.None, (IEnumerable<IndexedColumn>)null), Throws.ArgumentNullException);
    }

    [Test]
    public static void PrimaryKeyCtor_GivenEmptyColumns_ThrowsArgumentException()
    {
        Assert.That(() => new PrimaryKey(Option<string>.None, []), Throws.ArgumentException);
    }

    [Test]
    public static void UniqueKeyCtor_GivenNullColumns_ThrowsArgumentNullException()
    {
        Assert.That(() => new UniqueKey(Option<string>.None, (IEnumerable<IndexedColumn>)null), Throws.ArgumentNullException);
    }

    [Test]
    public static void UniqueKeyCtor_GivenEmptyColumns_ThrowsArgumentException()
    {
        Assert.That(() => new UniqueKey(Option<string>.None, []), Throws.ArgumentException);
    }

    [Test]
    public static void ForeignKeyCtor_GivenNullColumnNames_ThrowsArgumentNullException()
    {
        Assert.That(() => new ForeignKey(Option<string>.None, (IReadOnlyCollection<string>)null, "parent_table", ["parent_column"]), Throws.ArgumentNullException);
    }

    [Test]
    public static void ForeignKeyCtor_GivenEmptyColumnNames_ThrowsArgumentException()
    {
        Assert.That(() => new ForeignKey(Option<string>.None, [], "parent_table", ["parent_column"]), Throws.ArgumentException);
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void ForeignKeyCtor_GivenWhiteSpaceColumnName_ThrowsArgumentException(string columnName)
    {
        Assert.That(() => new ForeignKey(Option<string>.None, [columnName], "parent_table", ["parent_column"]), Throws.ArgumentException);
    }

    [Test]
    public static void ForeignKeyCtor_GivenNullParentColumnNames_ThrowsArgumentNullException()
    {
        Assert.That(() => new ForeignKey(Option<string>.None, ["child_column"], "parent_table", null), Throws.ArgumentNullException);
    }

    [Test]
    public static void ForeignKeyCtor_GivenEmptyParentColumnNames_DoesNotThrow()
    {
        // an omitted parent column list is valid SQLite and refers to the parent's primary key
        Assert.That(() => new ForeignKey(Option<string>.None, ["child_column"], "parent_table", []), Throws.Nothing);
    }

    [TestCase("")]
    [TestCase("    ")]
    public static void ForeignKeyCtor_GivenWhiteSpaceParentColumnName_ThrowsArgumentException(string parentColumnName)
    {
        Assert.That(() => new ForeignKey(Option<string>.None, ["child_column"], "parent_table", [parentColumnName]), Throws.ArgumentException);
    }

    [Test]
    public static void ForeignKeyCtor_GivenMismatchingColumnCounts_ThrowsArgumentException()
    {
        Assert.That(() => new ForeignKey(Option<string>.None, ["child_column"], "parent_table", ["parent_column_1", "parent_column_2"]), Throws.ArgumentException);
    }

    [Test]
    public static void ParsedTableDataCtor_GivenNullColumns_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new ParsedTableData("create table test ( a int )", null, Option<PrimaryKey>.None, [], [], []),
            Throws.ArgumentNullException
        );
    }

    [Test]
    public static void ParsedTableDataCtor_GivenEmptyColumns_ThrowsArgumentException()
    {
        Assert.That(
            () => new ParsedTableData("create table test ( a int )", [], Option<PrimaryKey>.None, [], [], []),
            Throws.ArgumentException
        );
    }
}
