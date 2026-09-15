using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Serialization.Mapping;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Serialization.Tests.Mapping;

internal static class RelationalDatabaseMapperTests
{
    [Test]
    public static void Map_GivenNullSource_ThrowsArgumentNullException()
    {
        var mapper = new RelationalDatabaseMapper();

        Assert.That(() => mapper.Map(null, new VerbatimIdentifierResolutionStrategy()), Throws.ArgumentNullException);
    }

    [Test]
    public static void Map_GivenNullIdentifierResolver_ThrowsArgumentNullException()
    {
        var mapper = new RelationalDatabaseMapper();

        Assert.That(() => mapper.Map(EmptyDto, null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task Map_GivenIdentifierResolver_UsesResolverForObjectLookup()
    {
        var mapper = new RelationalDatabaseMapper();
        var resolver = new RecordingIdentifierResolutionStrategy();

        var database = mapper.Map(EmptyDto, resolver);
        _ = await database.GetTable("test_table").IsSome;

        Assert.That(resolver.ResolvedNames, Does.Contain((Identifier)"test_table"));
    }

    [Test]
    public static async Task Map_GivenForeignKeyBetweenTables_ReturnsBothSidesAsColumnsOfTheirTables()
    {
        var mapper = new RelationalDatabaseMapper();
        var database = CreateParentChildDatabase("parent_table", "child_table");

        var dto = await mapper.MapAsync(database, default);
        var result = mapper.Map(dto, new VerbatimIdentifierResolutionStrategy());

        var parent = await result.GetTable("parent_table").UnwrapSomeAsync();
        var child = await result.GetTable("child_table").UnwrapSomeAsync();
        var parentKey = child.ParentKeys.Single();
        var childKey = parent.ChildKeys.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parentKey.ChildKey.Columns.Single(), Is.SameAs(child.Columns[1]));
            Assert.That(parentKey.ParentKey.Columns.Single(), Is.SameAs(parent.Columns[0]));
            Assert.That(parentKey.ParentKey.BackingIndex.UnwrapSome().Columns.Single().DependentColumns.Single(), Is.SameAs(parent.Columns[0]));
            Assert.That(childKey.ChildKey.Columns.Single(), Is.SameAs(child.Columns[1]));
            Assert.That(childKey.ParentKey.Columns.Single(), Is.SameAs(parent.Columns[0]));
        }
    }

    [Test]
    public static async Task Map_GivenDuplicateTableNames_ReturnsForeignKeyColumnsOfThatNameAsCopies()
    {
        var mapper = new RelationalDatabaseMapper();
        var database = CreateParentChildDatabase("parent_table", "child_table");

        var dto = await mapper.MapAsync(database, default);
        var parentDto = dto.Tables.Single(static table => table.TableName.LocalName == "parent_table");
        dto = dto with { Tables = [.. dto.Tables, parentDto] };

        var result = mapper.Map(dto, new VerbatimIdentifierResolutionStrategy());

        var parent = await result.GetTable("parent_table").UnwrapSomeAsync();
        var child = await result.GetTable("child_table").UnwrapSomeAsync();
        var parentKey = child.ParentKeys.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(parentKey.ChildKey.Columns.Single(), Is.SameAs(child.Columns[1]));
            Assert.That(parentKey.ParentKey.Columns.Single(), Is.Not.SameAs(parent.Columns[0]));
            Assert.That(parentKey.ParentKey.Columns.Single().Name.LocalName, Is.EqualTo("id"));
        }
    }

    [Test]
    public static async Task MapAsync_GivenForeignKeyBetweenTables_ReusesSerializedParentKey()
    {
        var mapper = new RelationalDatabaseMapper();
        var database = CreateParentChildDatabase("parent_table", "child_table");

        var dto = await mapper.MapAsync(database, default);

        var parent = dto.Tables.Single(static table => table.TableName.LocalName == "parent_table");
        var child = dto.Tables.Single(static table => table.TableName.LocalName == "child_table");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(child.ParentKeys.Single().ParentKey, Is.SameAs(parent.PrimaryKey));
            Assert.That(parent.ChildKeys.Single(), Is.SameAs(child.ParentKeys.Single()));
            Assert.That(parent.PrimaryKey!.Columns.Single(), Is.SameAs(parent.Columns.First()));
        }
    }

    private static IRelationalDatabase CreateParentChildDatabase(string parentTableName, string childTableName)
    {
        var columnType = new ColumnDataType("int", DataType.Integer, "int", typeof(int), false, 4, Option<INumericPrecision>.None, Option<Identifier>.None);
        IDatabaseColumn CreateColumn(string name) => new DatabaseColumn(name, columnType, false, Option<IDatabaseDefaultValue>.None, Option<IAutoIncrement>.None);

        var parentId = CreateColumn("id");
        var parentIndex = new DatabaseIndex("parent_pk_index", true, [new DatabaseIndexColumn("id", parentId, IndexColumnOrder.Ascending)], [], true, Option<string>.None);
        var parentKey = new DatabaseKey(Option<Identifier>.Some("parent_pk"), DatabaseKeyType.Primary, [parentId], true, Option<IDatabaseIndex>.Some(parentIndex));

        var childId = CreateColumn("id");
        var childParentId = CreateColumn("parent_id");
        var foreignKey = new DatabaseRelationalKey(
            childTableName,
            new DatabaseKey(Option<Identifier>.Some("child_fk"), DatabaseKeyType.Foreign, [childParentId], true),
            parentTableName,
            parentKey,
            ReferentialAction.NoAction,
            ReferentialAction.NoAction
        );

        var parent = new RelationalDatabaseTable(parentTableName, [parentId], Option<IDatabaseKey>.Some(parentKey), [], [], [foreignKey], [], [], []);
        var child = new RelationalDatabaseTable(childTableName, [childId, childParentId], Option<IDatabaseKey>.None, [], [foreignKey], [], [], [], []);

        return new RelationalDatabase(
            new IdentifierDefaults(null, null, null),
            new VerbatimIdentifierResolutionStrategy(),
            [parent, child],
            [],
            [],
            [],
            []
        );
    }

    private static Dto.RelationalDatabase EmptyDto => new()
    {
        IdentifierDefaults = new Dto.IdentifierDefaults { Schema = "main" },
        Tables = [],
        Views = [],
        Sequences = [],
        Synonyms = [],
        Routines = [],
    };
}