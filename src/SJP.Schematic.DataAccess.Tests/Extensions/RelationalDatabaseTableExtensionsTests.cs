using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.DataAccess.Extensions;

namespace SJP.Schematic.DataAccess.Tests.Extensions;

[TestFixture]
internal static class RelationalDatabaseTableExtensionsTests
{
    [Test]
    public static void GetMappedColumns_GivenNullTable_ThrowsArgumentNullException()
    {
        Assert.That(() => RelationalDatabaseTableExtensions.GetMappedColumns(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetMappedColumns_GivenNoHiddenColumns_ReturnsEveryColumn()
    {
        var columns = new[] { CreateColumn("first_column", false), CreateColumn("second_column", false) };
        var table = CreateTable(columns, Option<IDatabaseKey>.None, []);

        var result = table.GetMappedColumns().Select(static c => c.Name.LocalName).ToList();

        Assert.That(result, Is.EqualTo(new[] { "first_column", "second_column" }));
    }

    [Test]
    public static void GetMappedColumns_GivenHiddenColumn_OmitsTheHiddenColumn()
    {
        var visibleColumn = CreateColumn("first_column", false);
        var hiddenColumn = CreateColumn("second_column", true);
        var table = CreateTable([visibleColumn, hiddenColumn], Option<IDatabaseKey>.None, []);

        var result = table.GetMappedColumns().Select(static c => c.Name.LocalName).ToList();

        Assert.That(result, Is.EqualTo(new[] { "first_column" }));
    }

    [Test]
    public static void GetMappedColumns_GivenHiddenPrimaryKeyColumn_KeepsTheHiddenColumn()
    {
        var visibleColumn = CreateColumn("first_column", false);
        var hiddenColumn = CreateColumn("second_column", true);
        var primaryKey = new DatabaseKey(Option<Identifier>.Some("pk_test"), DatabaseKeyType.Primary, [hiddenColumn], true);
        var table = CreateTable([visibleColumn, hiddenColumn], Option<IDatabaseKey>.Some(primaryKey), []);

        var result = table.GetMappedColumns().Select(static c => c.Name.LocalName).ToList();

        Assert.That(result, Is.EqualTo(new[] { "first_column", "second_column" }));
    }

    [Test]
    public static void GetMappedColumns_GivenHiddenForeignKeyColumn_KeepsTheHiddenColumn()
    {
        var visibleColumn = CreateColumn("first_column", false);
        var hiddenColumn = CreateColumn("second_column", true);
        var parentKey = new DatabaseRelationalKey(
            "child_table",
            new DatabaseKey(Option<Identifier>.Some("fk_test"), DatabaseKeyType.Foreign, [hiddenColumn], true),
            "parent_table",
            new DatabaseKey(Option<Identifier>.Some("pk_parent"), DatabaseKeyType.Primary, [visibleColumn], true),
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );
        var table = CreateTable([visibleColumn, hiddenColumn], Option<IDatabaseKey>.None, [parentKey]);

        var result = table.GetMappedColumns().Select(static c => c.Name.LocalName).ToList();

        Assert.That(result, Is.EqualTo(new[] { "first_column", "second_column" }));
    }

    [Test]
    public static void GetMappedColumns_GivenHiddenIndexedColumn_KeepsTheHiddenColumn()
    {
        var visibleColumn = CreateColumn("first_column", false);
        var hiddenColumn = CreateColumn("second_column", true);
        var index = new DatabaseIndex(
            "ix_test",
            false,
            [new DatabaseIndexColumn("second_column", hiddenColumn, IndexColumnOrder.Ascending)],
            [],
            true,
            Option<string>.None
        );
        var table = new RelationalDatabaseTable(
            "child_table",
            [visibleColumn, hiddenColumn],
            Option<IDatabaseKey>.None,
            [],
            [],
            [],
            [index],
            [],
            []
        );

        var result = table.GetMappedColumns().Select(static c => c.Name.LocalName).ToList();

        Assert.That(result, Is.EqualTo(new[] { "first_column", "second_column" }));
    }

    private static IRelationalDatabaseTable CreateTable(
        IReadOnlyList<IDatabaseColumn> columns,
        Option<IDatabaseKey> primaryKey,
        IReadOnlyCollection<IDatabaseRelationalKey> parentKeys
    ) => new RelationalDatabaseTable("child_table", columns, primaryKey, [], parentKeys, [], [], [], []);

    private static IDatabaseColumn CreateColumn(Identifier columnName, bool isHidden)
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

        return new DatabaseColumn(
            columnName,
            columnType,
            false,
            Option<IDatabaseDefaultValue>.None,
            Option<IAutoIncrement>.None,
            false,
            Option<string>.None,
            ComputedColumnStorage.Unknown,
            isHidden
        );
    }
}
