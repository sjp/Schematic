using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;
using SJP.Schematic.Reporting.Html.ViewModels.Mappers;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.ViewModels.Mappers;

internal static class TableModelMapperTests
{
    private static readonly Identifier OrdersName = Identifier.CreateQualifiedIdentifier("test_schema", "orders");
    private static readonly Identifier CustomersName = Identifier.CreateQualifiedIdentifier("test_schema", "customers");
    private static readonly Identifier LinesName = Identifier.CreateQualifiedIdentifier("test_schema", "order_lines");

    [Test]
    public static void Map_GivenColumnsOfUserDefinedAndBuiltInTypes_LinksOnlyTheUserDefinedOne()
    {
        var moodName = Identifier.CreateQualifiedIdentifier("test_schema", "mood");
        IDatabaseColumn[] columns =
        [
            new DatabaseColumn("id", TestDbTypes.BigInteger, false, null, null),
            new DatabaseColumn("current_mood", UserDefinedDbTypes.Named(moodName), true, null, null),
        ];
        var table = new RelationalDatabaseTable(OrdersName, columns, Option<IDatabaseKey>.None, [], [], [], [], [], []);

        var model = new TableModelMapper([OrdersName]).Map(table, new UserDefinedTypeTargets([moodName]));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(GetColumn(model, "id").TypeUrl, Is.Null);
            Assert.That(GetColumn(model, "current_mood").TypeUrl, Is.EqualTo(UrlRouter.GetUserDefinedTypeUrl(moodName)));
        }
    }

    [Test]
    public static void Map_GivenCompositeForeignKey_LinksEachColumnToParentColumnAtSamePosition()
    {
        var model = MapOrders();

        var regionLinks = GetColumn(model, "region").ParentKeys.ToList();
        var customerLinks = GetColumn(model, "customer_no").ParentKeys.ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(regionLinks[0].ParentColumnName, Is.EqualTo("region_code"));
            Assert.That(customerLinks.Select(static k => k.ParentColumnName), Is.EqualTo(new[] { "number" }));
            Assert.That(customerLinks[0].ConstraintDescription, Is.EqualTo("test_schema.orders.customer_no references test_schema.customers.number via fk_orders_customers"));
        }
    }

    [Test]
    public static void Map_GivenColumnInSeveralForeignKeys_ListsLinksInKeyOrder()
    {
        var model = MapOrders();

        var links = GetColumn(model, "region").ParentKeys.Select(static k => k.ConstraintDescription);

        Assert.That(links, Is.EqualTo(new[]
        {
            "test_schema.orders.region references test_schema.customers.region_code via fk_orders_customers",
            "test_schema.orders.region references test_schema.customers.region_code",
        }));
    }

    [Test]
    public static void Map_GivenChildKey_LinksToChildColumnAtSamePositionUsingForeignKeyName()
    {
        var model = MapOrders();

        var links = GetColumn(model, "order_id").ChildKeys.ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(links.Select(static k => k.ChildColumnName), Is.EqualTo(new[] { "line_order_id" }));
            Assert.That(links[0].ConstraintDescription, Is.EqualTo("test_schema.order_lines.line_order_id references test_schema.orders.order_id via fk_lines_orders"));
        }
    }

    [Test]
    public static void Map_GivenColumnsDifferingOnlyByCase_OnlyFlagsExactKeyColumnNames()
    {
        var model = MapOrders();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(GetColumn(model, "order_id").IsPrimaryKey, Is.True);
            Assert.That(GetColumn(model, "ORDER_ID").IsPrimaryKey, Is.False);
            Assert.That(GetColumn(model, "ORDER_ID").ChildKeys, Is.Empty);
            Assert.That(GetColumn(model, "region").IsForeignKey, Is.True);
            Assert.That(GetColumn(model, "notes").IsForeignKey, Is.False);
            Assert.That(GetColumn(model, "notes").ParentKeys, Is.Empty);
        }
    }

    private static Table.Column GetColumn(Table model, string columnName) => model.Columns.Single(c => c.ColumnName == columnName);

    private static Table MapOrders()
    {
        var customerColumns = CreateColumns("number", "region_code");
        var customersKey = new DatabaseKey(Option<Identifier>.Some("pk_customers"), DatabaseKeyType.Primary, [customerColumns[1], customerColumns[0]], true);
        var customersRegionKey = new DatabaseKey(Option<Identifier>.Some("uk_customers_region"), DatabaseKeyType.Unique, [customerColumns[1]], true);

        var orderColumns = CreateColumns("order_id", "ORDER_ID", "customer_no", "region", "notes");
        var ordersKey = new DatabaseKey(Option<Identifier>.Some("pk_orders"), DatabaseKeyType.Primary, [orderColumns[0]], true);

        // Listed in the opposite order to the table's columns, so pairing must follow key position.
        var customerForeignKey = new DatabaseKey(Option<Identifier>.Some("fk_orders_customers"), DatabaseKeyType.Foreign, [orderColumns[3], orderColumns[2]], true);
        var regionForeignKey = new DatabaseKey(Option<Identifier>.None, DatabaseKeyType.Foreign, [orderColumns[3]], true);
        var parentKeys = new List<IDatabaseRelationalKey>
        {
            new DatabaseRelationalKey(OrdersName, customerForeignKey, CustomersName, customersKey, ReferentialAction.NoAction, ReferentialAction.NoAction),
            new DatabaseRelationalKey(OrdersName, regionForeignKey, CustomersName, customersRegionKey, ReferentialAction.NoAction, ReferentialAction.NoAction),
        };

        var lineColumns = CreateColumns("line_id", "line_order_id");
        var linesForeignKey = new DatabaseKey(Option<Identifier>.Some("fk_lines_orders"), DatabaseKeyType.Foreign, [lineColumns[1]], true);
        var childKeys = new List<IDatabaseRelationalKey>
        {
            new DatabaseRelationalKey(LinesName, linesForeignKey, OrdersName, ordersKey, ReferentialAction.NoAction, ReferentialAction.NoAction),
        };

        var orders = new RelationalDatabaseTable(OrdersName, orderColumns, ordersKey, [], parentKeys, childKeys, [], [], []);

        return new TableModelMapper([OrdersName, CustomersName, LinesName]).Map(orders, new UserDefinedTypeTargets([]));
    }

    private static IReadOnlyList<IDatabaseColumn> CreateColumns(params string[] names)
    {
        return names.Select(static name => (IDatabaseColumn)new DatabaseColumn(name, TestDbTypes.BigInteger, false, null, null)).ToList();
    }
}
