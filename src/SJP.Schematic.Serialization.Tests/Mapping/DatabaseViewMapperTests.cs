using System.Linq;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Serialization.Mapping;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Serialization.Tests.Mapping;

internal static class DatabaseViewMapperTests
{
    [Test]
    public static void Map_GivenViewWithTriggersAndCheckOption_RoundTrips()
    {
        var mapper = new DatabaseViewMapper();

        var trigger = new DatabaseTrigger(
            "test_trigger",
            "create trigger test_trigger instead of insert on test_view begin end",
            TriggerQueryTiming.InsteadOf,
            TriggerEvent.Insert,
            true,
            TriggerGranularity.Row,
            Option<string>.Some("new.id > 0"),
            ["id"]
        );

        var view = new DatabaseView(
            "test_view",
            "select * from test",
            [],
            [trigger],
            [],
            ViewCheckOption.Cascaded,
            true
        );

        var result = mapper.Map(mapper.Map(view));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsMaterialized, Is.False);
            Assert.That(result.CheckOption, Is.EqualTo(ViewCheckOption.Cascaded));
            Assert.That(result.IsUpdatable, Is.True);
            Assert.That(result.Indexes, Is.Empty);
            Assert.That(result.Triggers, Has.Count.EqualTo(1));
            Assert.That(result.Triggers.Single().Name.LocalName, Is.EqualTo("test_trigger"));
            Assert.That(result.Triggers.Single().QueryTiming, Is.EqualTo(TriggerQueryTiming.InsteadOf));
        });
    }

    [Test]
    public static void Map_GivenMaterializedViewWithIndexes_RoundTrips()
    {
        var mapper = new DatabaseViewMapper();

        var column = new DatabaseColumn(
            "id",
            new ColumnDataType("int", DataType.Integer, "int", typeof(int), false, 4, Option<INumericPrecision>.None, Option<Identifier>.None),
            false,
            Option<string>.None,
            Option<IAutoIncrement>.None
        );
        var index = new DatabaseIndex(
            "test_index",
            true,
            [new DatabaseIndexColumn("id", column, IndexColumnOrder.Ascending)],
            [],
            true,
            Option<string>.None,
            IndexType.BTree,
            Option<int>.None,
            true,
            true
        );

        var view = new DatabaseMaterializedView(
            "test_mat_view",
            "select * from test",
            [column],
            [],
            [index],
            MaterializedViewRefreshMode.OnDemand,
            Option<string>.Some("COMPLETE"),
            true
        );

        var result = mapper.Map(mapper.Map(view));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsMaterialized, Is.True);
            Assert.That(result, Is.InstanceOf<IDatabaseMaterializedView>());
            Assert.That(result.Indexes, Has.Count.EqualTo(1));
            Assert.That(result.Indexes.Single().Name.LocalName, Is.EqualTo("test_index"));

            var materializedView = (IDatabaseMaterializedView)result;
            Assert.That(materializedView.RefreshMode, Is.EqualTo(MaterializedViewRefreshMode.OnDemand));
            Assert.That(materializedView.RefreshMethod.UnwrapSome(), Is.EqualTo("COMPLETE"));
            Assert.That(materializedView.IsPopulated, Is.True);
        });
    }

    [Test]
    public static void Map_GivenDtoWithoutViewMetadata_ReturnsViewWithoutTriggersOrIndexes()
    {
        var mapper = new DatabaseViewMapper();

        var dto = new Dto.DatabaseView
        {
            ViewName = new Dto.Identifier { LocalName = "test_view" },
            Definition = "select * from test",
            Columns = [],
            IsMaterialized = false,
        };

        var result = mapper.Map(dto);

        Assert.Multiple(() =>
        {
            Assert.That(result.Triggers, Is.Empty);
            Assert.That(result.Indexes, Is.Empty);
            Assert.That(result.CheckOption, Is.EqualTo(ViewCheckOption.None));
            Assert.That(result.IsUpdatable, Is.False);
        });
    }
}
