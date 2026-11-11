using System.Linq;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.ViewModels.Mappers;

internal static class ColumnsModelMapperTests
{
    private static readonly Identifier MoodName = Identifier.CreateQualifiedIdentifier("test_schema", "mood");

    [Test]
    public static void Map_GivenTableColumnOfUserDefinedType_LinksTypeToItsPage()
    {
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "person");
        IDatabaseColumn[] columns =
        [
            new DatabaseColumn("id", TestDbTypes.BigInteger, false, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored()),
            new DatabaseColumn("current_mood", UserDefinedDbTypes.Named(MoodName), true, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored()),
        ];
        var table = new RelationalDatabaseTable(tableName, columns, Option<DatabaseKey.Primary>.None, [], [], [], [], [], []);

        var summaries = new ColumnsModelMapper().Map(table, new UserDefinedTypeTargets([MoodName])).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(summaries[0].TypeUrl, Is.Null);
            Assert.That(summaries[1].TypeUrl, Is.EqualTo(UrlRouter.GetUserDefinedTypeUrl(MoodName)));
        }
    }

    [Test]
    public static void Map_GivenViewColumnOfUserDefinedType_LinksTypeToItsPage()
    {
        var viewName = Identifier.CreateQualifiedIdentifier("test_schema", "person_moods");
        IDatabaseColumn[] columns =
        [
            new DatabaseColumn("current_mood", UserDefinedDbTypes.Named(MoodName), true, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored()),
        ];
        var view = new DatabaseView(viewName, "select current_mood from person", columns);

        var summaries = new ColumnsModelMapper().Map(view, new UserDefinedTypeTargets([MoodName])).ToList();

        Assert.That(summaries[0].TypeUrl, Is.EqualTo(UrlRouter.GetUserDefinedTypeUrl(MoodName)));
    }

    [Test]
    public static void Map_GivenTableWithKeys_FlagsColumnsByExactKeyColumnName()
    {
        var tableName = Identifier.CreateQualifiedIdentifier("test_schema", "orders");
        var parentName = Identifier.CreateQualifiedIdentifier("test_schema", "customers");

        IDatabaseColumn[] columns =
        [
            new DatabaseColumn("id", TestDbTypes.BigInteger, false, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored()),
            new DatabaseColumn("ID", TestDbTypes.BigInteger, false, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored()),
            new DatabaseColumn("code", TestDbTypes.BigInteger, false, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored()),
            new DatabaseColumn("customer_id", TestDbTypes.BigInteger, false, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored()),
        ];
        var parentColumn = new DatabaseColumn("id", TestDbTypes.BigInteger, false, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored());

        var primaryKey = new DatabaseKey.Primary(Option<Identifier>.Some("pk_orders"), [columns[0]], true);
        var uniqueKey = new DatabaseKey.Unique(Option<Identifier>.Some("uk_orders_code"), [columns[2], columns[3]], true);
        var foreignKey = new DatabaseRelationalKey(
            tableName,
            new DatabaseKey.Foreign(Option<Identifier>.Some("fk_orders_customers"), [columns[3]], true),
            parentName,
            new DatabaseKey.Primary(Option<Identifier>.Some("pk_customers"), [parentColumn], true),
            ReferentialAction.NoAction,
            ReferentialAction.NoAction);
        var table = new RelationalDatabaseTable(tableName, columns, primaryKey, [uniqueKey], [foreignKey], [], [], [], []);

        var summaries = new ColumnsModelMapper().Map(table, new UserDefinedTypeTargets([])).ToList();

        Assert.That(
            summaries.Select(static c => (c.ColumnName, c.IsPrimaryKey, c.IsUniqueKey, c.IsForeignKey)),
            Is.EqualTo(new[]
            {
                ("id", true, false, false),
                ("ID", false, false, false),
                ("code", false, true, false),
                ("customer_id", false, true, true),
            }));
    }
}
