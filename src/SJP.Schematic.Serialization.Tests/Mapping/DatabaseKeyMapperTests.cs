using System;
using System.Linq;
using System.Text.Json;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Serialization.Mapping;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Serialization.Tests.Mapping;

internal static class DatabaseKeyMapperTests
{
    private static DatabaseColumn CreateColumn(string columnName)
    {
        return new DatabaseColumn(
            columnName,
            new ColumnDataType("int", DataType.Integer, "int", typeof(int), false, 4, Option<INumericPrecision>.None, Option<Identifier>.None),
            false,
            Option<IDatabaseDefaultValue>.None,
            Option<IAutoIncrement>.None
        );
    }

    private static DatabaseIndex CreateIndex(string indexName, IDatabaseColumn column)
    {
        return new DatabaseIndex(indexName, true, [new DatabaseIndexColumn(column.Name.LocalName, column, IndexColumnOrder.Ascending)], [], true, Option<string>.None);
    }

    [Test]
    public static void Map_GivenPrimaryKey_WritesPrimaryKeyTypeAndBackingIndex()
    {
        var mapper = new DatabaseKeyMapper();
        var column = CreateColumn("id");
        var key = new DatabaseKey.Primary(Option<Identifier>.Some("pk_test"), [column], true, Option<IDatabaseIndex>.Some(CreateIndex("pk_test", column)));

        var result = mapper.Map(key);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.KeyType, Is.EqualTo(DatabaseKeyType.Primary));
            Assert.That(result.BackingIndex, Is.Not.Null);
        }
    }

    [Test]
    public static void Map_GivenUniqueKey_WritesUniqueKeyTypeAndBackingIndex()
    {
        var mapper = new DatabaseKeyMapper();
        var column = CreateColumn("code");
        var key = new DatabaseKey.Unique(Option<Identifier>.Some("uk_test"), [column], true, Option<IDatabaseIndex>.Some(CreateIndex("uk_test", column)));

        var result = mapper.Map(key);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.KeyType, Is.EqualTo(DatabaseKeyType.Unique));
            Assert.That(result.BackingIndex, Is.Not.Null);
        }
    }

    [Test]
    public static void Map_GivenForeignKey_WritesForeignKeyTypeWithoutBackingIndex()
    {
        var mapper = new DatabaseKeyMapper();
        var key = new DatabaseKey.Foreign(Option<Identifier>.Some("fk_test"), [CreateColumn("parent_id")], true, false, ConstraintDeferrability.DeferrableInitiallyDeferred);

        var result = mapper.Map(key);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.KeyType, Is.EqualTo(DatabaseKeyType.Foreign));
            Assert.That(result.BackingIndex, Is.Null);
            Assert.That(result.IsValidated, Is.False);
            Assert.That(result.Deferrability, Is.EqualTo(ConstraintDeferrability.DeferrableInitiallyDeferred));
        }
    }

    [Test]
    public static void Map_GivenDefaultKey_ThrowsArgumentNullException()
    {
        var mapper = new DatabaseKeyMapper();

        Assert.That(
            () => mapper.Map(default(DatabaseKey)),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("source")
        );
    }

    [Test]
    public static void Map_GivenSerializedPrimaryKey_RoundTripsAsPrimaryKey()
    {
        var mapper = new DatabaseKeyMapper();
        var column = CreateColumn("id");
        var key = new DatabaseKey.Primary(Option<Identifier>.Some("pk_test"), [column], false, Option<IDatabaseIndex>.Some(CreateIndex("pk_test", column)), false, ConstraintDeferrability.DeferrableInitiallyImmediate);

        var result = mapper.Map(mapper.Map(key));

        Assert.That(result.Value, Is.TypeOf<DatabaseKey.Primary>());
        var primaryKey = (DatabaseKey.Primary)result.Value!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(primaryKey.Name.UnwrapSome().LocalName, Is.EqualTo("pk_test"));
            Assert.That(primaryKey.Columns.Select(static c => c.Name.LocalName), Is.EqualTo(new[] { "id" }));
            Assert.That(primaryKey.BackingIndex.UnwrapSome().Name.LocalName, Is.EqualTo("pk_test"));
            Assert.That(primaryKey.IsEnabled, Is.False);
            Assert.That(primaryKey.IsValidated, Is.False);
            Assert.That(primaryKey.Deferrability, Is.EqualTo(ConstraintDeferrability.DeferrableInitiallyImmediate));
        }
    }

    [Test]
    public static void Map_GivenSerializedUniqueKey_RoundTripsAsUniqueKey()
    {
        var mapper = new DatabaseKeyMapper();
        var key = new DatabaseKey.Unique(Option<Identifier>.Some("uk_test"), [CreateColumn("code")], true);

        var result = mapper.Map(mapper.Map(key));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Value, Is.TypeOf<DatabaseKey.Unique>());
            Assert.That(result.Name.UnwrapSome().LocalName, Is.EqualTo("uk_test"));
        }
    }

    [Test]
    public static void Map_GivenSerializedForeignKey_RoundTripsAsForeignKey()
    {
        var mapper = new DatabaseKeyMapper();
        var key = new DatabaseKey.Foreign(Option<Identifier>.Some("fk_test"), [CreateColumn("parent_id")], true);

        var result = mapper.Map(mapper.Map(key));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Value, Is.TypeOf<DatabaseKey.Foreign>());
            Assert.That(result.Name.UnwrapSome().LocalName, Is.EqualTo("fk_test"));
        }
    }

    [Test]
    public static void Map_GivenSerializedKeyWithInvalidKeyType_ThrowsArgumentException()
    {
        var mapper = new DatabaseKeyMapper();
        var serializedKey = mapper.Map(TestKeys.Primary()) with { KeyType = (DatabaseKeyType)55 };

        Assert.That(
            () => mapper.Map(serializedKey),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("source")
        );
    }

    [Test]
    public static void MapPrimaryKey_GivenSerializedUniqueKey_ThrowsArgumentException()
    {
        var mapper = new DatabaseKeyMapper();
        var serializedKey = mapper.Map(TestKeys.Unique());

        Assert.That(
            () => mapper.MapPrimaryKey(serializedKey, ColumnLookup.Empty),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("source")
        );
    }

    [Test]
    public static void MapForeignKey_GivenSerializedPrimaryKey_ThrowsArgumentException()
    {
        var mapper = new DatabaseKeyMapper();
        var serializedKey = mapper.Map(TestKeys.Primary());

        Assert.That(
            () => mapper.MapForeignKey(serializedKey, ColumnLookup.Empty),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("source")
        );
    }

    [Test]
    public static void MapCandidateKey_GivenSerializedForeignKey_ThrowsArgumentException()
    {
        var mapper = new DatabaseKeyMapper();
        var serializedKey = mapper.Map(TestKeys.Foreign());

        Assert.That(
            () => mapper.MapCandidateKey(serializedKey, ColumnLookup.Empty),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("source")
        );
    }

    [Test]
    public static void Map_GivenSerializedForeignKeyWithBackingIndex_IgnoresBackingIndex()
    {
        var mapper = new DatabaseKeyMapper();
        var column = CreateColumn("id");
        var serializedKey = mapper.Map(new DatabaseKey.Unique(Option<Identifier>.None, [column], true, Option<IDatabaseIndex>.Some(CreateIndex("ux_test", column))))
            with { KeyType = DatabaseKeyType.Foreign };

        var result = mapper.Map(serializedKey);

        Assert.That(result.Value, Is.TypeOf<DatabaseKey.Foreign>());
    }

    [TestCase(DatabaseKeyType.Primary, "\"Primary\"")]
    [TestCase(DatabaseKeyType.Unique, "\"Unique\"")]
    [TestCase(DatabaseKeyType.Foreign, "\"Foreign\"")]
    public static void KeyType_WhenSerialized_IsWrittenByName(DatabaseKeyType keyType, string expectedJson)
    {
        var json = JsonSerializer.Serialize(keyType, JsonSerializerSettings.Default);

        Assert.That(json, Is.EqualTo(expectedJson));
    }
}
