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
            new DatabaseColumn("id", TestDbTypes.BigInteger, false, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored()),
            new DatabaseColumn("current_mood", UserDefinedDbTypes.Named(moodName), true, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored()),
        ];
        var table = new RelationalDatabaseTable(OrdersName, columns, Option<DatabaseKey.Primary>.None, [], [], [], [], [], []);

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
        var customersKey = new DatabaseKey.Primary(Option<Identifier>.Some("pk_customers"), [customerColumns[1], customerColumns[0]], true);
        var customersRegionKey = new DatabaseKey.Unique(Option<Identifier>.Some("uk_customers_region"), [customerColumns[1]], true);

        var orderColumns = CreateColumns("order_id", "ORDER_ID", "customer_no", "region", "notes");
        var ordersKey = new DatabaseKey.Primary(Option<Identifier>.Some("pk_orders"), [orderColumns[0]], true);

        // Listed in the opposite order to the table's columns, so pairing must follow key position.
        var customerForeignKey = new DatabaseKey.Foreign(Option<Identifier>.Some("fk_orders_customers"), [orderColumns[3], orderColumns[2]], true);
        var regionForeignKey = new DatabaseKey.Foreign(Option<Identifier>.None, [orderColumns[3]], true);
        var parentKeys = new List<IDatabaseRelationalKey>
        {
            new DatabaseRelationalKey(OrdersName, customerForeignKey, CustomersName, customersKey, ReferentialAction.NoAction, ReferentialAction.NoAction),
            new DatabaseRelationalKey(OrdersName, regionForeignKey, CustomersName, customersRegionKey, ReferentialAction.NoAction, ReferentialAction.NoAction),
        };

        var lineColumns = CreateColumns("line_id", "line_order_id");
        var linesForeignKey = new DatabaseKey.Foreign(Option<Identifier>.Some("fk_lines_orders"), [lineColumns[1]], true);
        var childKeys = new List<IDatabaseRelationalKey>
        {
            new DatabaseRelationalKey(LinesName, linesForeignKey, OrdersName, ordersKey, ReferentialAction.NoAction, ReferentialAction.NoAction),
        };

        var orders = new RelationalDatabaseTable(OrdersName, orderColumns, ordersKey, [], parentKeys, childKeys, [], [], []);

        return new TableModelMapper([OrdersName, CustomersName, LinesName]).Map(orders, new UserDefinedTypeTargets([]));
    }

    private static IReadOnlyList<IDatabaseColumn> CreateColumns(params string[] names)
    {
        return names.Select(static name => (IDatabaseColumn)new DatabaseColumn(name, TestDbTypes.BigInteger, false, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Stored())).ToList();
    }

    [Test]
    public static void Map_GivenColumnOfEachValueSource_DescribesHowEachColumnGetsItsValues()
    {
        var sequenceName = Identifier.CreateQualifiedIdentifier("test_schema", "orders_id_seq");
        var identity = new AutoIncrement(1, 1, IdentityGeneration.Always, Option<decimal>.None, Option<decimal>.None, false, Option<Identifier>.Some(sequenceName));
        IDatabaseColumn[] columns =
        [
            new DatabaseColumn("id", TestDbTypes.BigInteger, false, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Generated(identity)),
            new DatabaseColumn("quantity", TestDbTypes.BigInteger, false, Option<DatabaseDefaultValue>.Some(new DatabaseDefaultValue.Literal("1")), new ColumnValueSource.Stored()),
            new DatabaseColumn("total", TestDbTypes.BigInteger, true, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Computed(Option<string>.Some("quantity * 2"), ComputedColumnStorage.Stored)),
            new DatabaseColumn("opaque", TestDbTypes.BigInteger, true, Option<DatabaseDefaultValue>.None, new ColumnValueSource.Computed(Option<string>.None, ComputedColumnStorage.Unknown)),
        ];
        var table = new RelationalDatabaseTable(OrdersName, columns, Option<DatabaseKey.Primary>.None, [], [], [], [], [], []);

        var model = new TableModelMapper([OrdersName]).Map(table, new UserDefinedTypeTargets([]));

        var generated = GetColumn(model, "id");
        var stored = GetColumn(model, "quantity");
        var computed = GetColumn(model, "total");
        var opaque = GetColumn(model, "opaque");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(generated.IsAutoIncrement, Is.True);
            Assert.That(generated.IdentityGeneration, Is.EqualTo("Always"));
            Assert.That(generated.IdentitySequenceName, Is.EqualTo("test_schema.orders_id_seq"));
            Assert.That(generated.IsComputed, Is.False);
            Assert.That(generated.ComputedDefinition, Is.Empty);
            Assert.That(generated.ComputedStorage, Is.Empty);

            Assert.That(stored.IsAutoIncrement, Is.False);
            Assert.That(stored.IdentityGeneration, Is.Empty);
            Assert.That(stored.IdentitySequenceName, Is.Empty);
            Assert.That(stored.IsComputed, Is.False);
            Assert.That(stored.ComputedDefinition, Is.Empty);
            Assert.That(stored.ComputedStorage, Is.Empty);
            Assert.That(stored.DefaultValue, Is.EqualTo("1"));

            Assert.That(computed.IsAutoIncrement, Is.False);
            Assert.That(computed.IsComputed, Is.True);
            Assert.That(computed.ComputedDefinition, Is.EqualTo("quantity * 2"));
            Assert.That(computed.ComputedStorage, Is.EqualTo("Stored"));

            Assert.That(opaque.IsComputed, Is.True);
            Assert.That(opaque.ComputedDefinition, Is.Empty);
            Assert.That(opaque.ComputedStorage, Is.Empty);
        }
    }
}
