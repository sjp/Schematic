using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Serialization.Tests;

internal sealed class JsonRelationalDatabaseSerializerTests : SakilaTest
{
    private static IRelationalDatabaseSerializer Serializer { get; } = new JsonRelationalDatabaseSerializer();

    // the guards must be evaluated before the first await, so these assert on the synchronous call
    [Test]
    public static void SerializeAsync_GivenNullStream_ThrowsArgumentNullException()
    {
        var db = new EmptyRelationalDatabase(new IdentifierDefaults(null, null, "main"));

        Assert.That(() => Serializer.SerializeAsync(null, db), Throws.ArgumentNullException);
    }

    [Test]
    public static void SerializeAsync_GivenNullDatabase_ThrowsArgumentNullException()
    {
        using var stream = new MemoryStream();

        Assert.That(() => Serializer.SerializeAsync(stream, null), Throws.ArgumentNullException);
    }

    [Test]
    public static void DeserializeAsync_GivenNullStream_ThrowsArgumentNullException()
    {
        Assert.That(() => Serializer.DeserializeAsync(null, new VerbatimIdentifierResolutionStrategy()), Throws.ArgumentNullException);
    }

    [Test]
    public static void DeserializeAsync_GivenNullIdentifierResolver_ThrowsArgumentNullException()
    {
        using var stream = new MemoryStream();

        Assert.That(() => Serializer.DeserializeAsync(stream, null), Throws.ArgumentNullException);
    }

    [Test]
    public async Task Serialize_WhenInvoked_ExportsWithoutError()
    {
        var db = await GetSnapshotDatabaseAsync();
        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(json, Is.Not.Null);
            Assert.That(json, Is.Not.Empty);
        }
    }

    [Test]
    public static async Task DeserializeAsync_WhenJsonContainsIdentifierResolver_IgnoresProperty()
    {
        const string json = """
            {
                "IdentifierResolver": { "SomeProperty": "some value" },
                "IdentifierDefaults": { "Schema": "main" },
                "Tables": [],
                "Views": [],
                "Sequences": [],
                "Synonyms": [],
                "Routines": []
            }
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var importedDb = await Serializer.DeserializeAsync(stream, new VerbatimIdentifierResolutionStrategy());

        Assert.That(importedDb.IdentifierDefaults.Schema, Is.EqualTo("main"));
    }

    [Test]
    public static async Task SerializeAsync_WhenInvoked_DoesNotWriteIdentifierResolver()
    {
        var db = new EmptyRelationalDatabase(new IdentifierDefaults(null, null, "main"));

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        Assert.That(json, Does.Not.Contain("IdentifierResolver"));
    }

    [Test]
    public async Task SerializeDeserialize_WhenEmptyDatabaseRoundTripped_ExportsAndParsesWithoutError()
    {
        var db = new EmptyRelationalDatabase(new IdentifierDefaults(null, null, "main"));

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy());

        db.Should().BeEquivalentTo(importedDb);
    }

    [Test]
    public async Task SerializeDeserialize_WhenEmptyDatabaseRoundTripped_PreservesJsonStructure()
    {
        var db = new EmptyRelationalDatabase(new IdentifierDefaults(null, null, "main"));

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy());

        await using var jsonOutputStream2 = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream2, importedDb);
        var reExportedJson = Encoding.UTF8.GetString(jsonOutputStream2.ToArray());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(reExportedJson, Is.Not.Null);
            Assert.That(reExportedJson, Is.Not.Empty);
            Assert.That(reExportedJson, Is.EqualTo(json));
        }
    }

    [Test]
    public async Task SerializeDeserialize_WhenRoundTripped_ExportsAndParsesWithoutError()
    {
        var db = await GetSnapshotDatabaseAsync();

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy());

        db.Should().BeEquivalentTo(importedDb);
    }

    [Test]
    public async Task SerializeDeserialize_WhenRoundTripped_PreservesJsonStructure()
    {
        var db = await GetSnapshotDatabaseAsync();

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy());

        await using var jsonOutputStream2 = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream2, importedDb);
        var reExportedJson = Encoding.UTF8.GetString(jsonOutputStream2.ToArray());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(reExportedJson, Is.Not.Null);
            Assert.That(reExportedJson, Is.Not.Empty);
            Assert.That(reExportedJson, Is.EqualTo(json));
        }
    }

    // the above covers tables and views, but as the test database has no sequences, synonyms and routines we need to implement it
    [Test]
    public async Task SerializeDeserialize_WhenSequenceRoundTripped_ExportsAndParsesWithoutError()
    {
        var sequence = new DatabaseSequence(
            "test_sequence_name",
            1,
            10,
            Option<decimal>.Some(-10),
            Option<decimal>.Some(1000),
            true,
            20
        );
        var sequences = new[] { sequence };

        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();
        var synonyms = Array.Empty<IDatabaseSynonym>();
        var routines = Array.Empty<IDatabaseRoutine>();

        var db = new RelationalDatabase(
            new IdentifierDefaults(null, null, "main"),
            new VerbatimIdentifierResolutionStrategy(),
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy());

        db.Should().BeEquivalentTo(importedDb);
    }

    [Test]
    public async Task SerializeDeserialize_WhenSequenceRoundTripped_PreservesJsonStructure()
    {
        var sequence = new DatabaseSequence(
            "test_sequence_name",
            1,
            10,
            Option<decimal>.Some(-10),
            Option<decimal>.Some(1000),
            true,
            20
        );
        var sequences = new[] { sequence };

        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();
        var synonyms = Array.Empty<IDatabaseSynonym>();
        var routines = Array.Empty<IDatabaseRoutine>();

        var db = new RelationalDatabase(
            new IdentifierDefaults(null, null, "main"),
            new VerbatimIdentifierResolutionStrategy(),
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy());

        await using var jsonOutputStream2 = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream2, importedDb);
        var reExportedJson = Encoding.UTF8.GetString(jsonOutputStream2.ToArray());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(reExportedJson, Is.Not.Null);
            Assert.That(reExportedJson, Is.Not.Empty);
            Assert.That(reExportedJson, Is.EqualTo(json));
        }
    }

    [Test]
    public async Task SerializeDeserialize_WhenSynonymRoundTripped_ExportsAndParsesWithoutError()
    {
        var synonym = new DatabaseSynonym("test_synonym_name", "test_target_name");
        var synonyms = new[] { synonym };

        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();
        var sequences = Array.Empty<IDatabaseSequence>();
        var routines = Array.Empty<IDatabaseRoutine>();

        var db = new RelationalDatabase(
            new IdentifierDefaults(null, null, "main"),
            new VerbatimIdentifierResolutionStrategy(),
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy());

        db.Should().BeEquivalentTo(importedDb);
    }

    [Test]
    public async Task SerializeDeserialize_WhenSynonymRoundTripped_PreservesJsonStructure()
    {
        var synonym = new DatabaseSynonym("test_synonym_name", "test_target_name");
        var synonyms = new[] { synonym };

        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();
        var sequences = Array.Empty<IDatabaseSequence>();
        var routines = Array.Empty<IDatabaseRoutine>();

        var db = new RelationalDatabase(
            new IdentifierDefaults(null, null, "main"),
            new VerbatimIdentifierResolutionStrategy(),
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy());

        await using var jsonOutputStream2 = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream2, importedDb);
        var reExportedJson = Encoding.UTF8.GetString(jsonOutputStream2.ToArray());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(reExportedJson, Is.Not.Null);
            Assert.That(reExportedJson, Is.Not.Empty);
            Assert.That(reExportedJson, Is.EqualTo(json));
        }
    }

    [Test]
    public async Task SerializeDeserialize_WhenRoutineRoundTripped_ExportsAndParsesWithoutError()
    {
        var routine = new DatabaseRoutine("test_routine_name", "test_routine_definition");
        var routines = new[] { routine };

        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();
        var sequences = Array.Empty<IDatabaseSequence>();
        var synonyms = Array.Empty<IDatabaseSynonym>();

        var db = new RelationalDatabase(
            new IdentifierDefaults(null, null, "main"),
            new VerbatimIdentifierResolutionStrategy(),
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy());

        db.Should().BeEquivalentTo(importedDb);
    }

    [Test]
    public async Task SerializeDeserialize_WhenRoutineRoundTripped_PreservesJsonStructure()
    {
        var routine = new DatabaseRoutine("test_routine_name", "test_routine_definition");
        var routines = new[] { routine };

        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();
        var sequences = Array.Empty<IDatabaseSequence>();
        var synonyms = Array.Empty<IDatabaseSynonym>();

        var db = new RelationalDatabase(
            new IdentifierDefaults(null, null, "main"),
            new VerbatimIdentifierResolutionStrategy(),
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy());

        await using var jsonOutputStream2 = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream2, importedDb);
        var reExportedJson = Encoding.UTF8.GetString(jsonOutputStream2.ToArray());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(reExportedJson, Is.Not.Null);
            Assert.That(reExportedJson, Is.Not.Empty);
            Assert.That(reExportedJson, Is.EqualTo(json));
        }
    }

    [Test]
    public static async Task SerializeDeserialize_WhenComputedColumnWithDefinitionRoundTripped_PreservesComputedDefinition()
    {
        const string definition = "([first_name] + ' ' + [last_name])";
        var db = CreateComputedColumnDatabase(Option<string>.Some(definition));

        var importedDb = await RoundTripAsync(db);

        var tables = await importedDb.GetAllTables();
        var column = tables.Single().Columns.Single(c => c.Name.LocalName == "test_computed_column");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(column.IsComputed, Is.True);
            Assert.That(column.ComputedDefinition.UnwrapSome(), Is.EqualTo(definition));
        }
    }

    [Test]
    public static async Task SerializeDeserialize_WhenComputedColumnWithoutDefinitionRoundTripped_PreservesMissingDefinition()
    {
        var db = CreateComputedColumnDatabase(Option<string>.None);

        var importedDb = await RoundTripAsync(db);

        var tables = await importedDb.GetAllTables();
        var column = tables.Single().Columns.Single(c => c.Name.LocalName == "test_computed_column");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(column.IsComputed, Is.True);
            Assert.That(column.ComputedDefinition.IsNone, Is.True);
        }
    }

    [TestCase(ComputedColumnStorage.Stored)]
    [TestCase(ComputedColumnStorage.Virtual)]
    [TestCase(ComputedColumnStorage.Unknown)]
    public static async Task SerializeDeserialize_WhenComputedColumnRoundTripped_PreservesComputedStorage(ComputedColumnStorage storage)
    {
        var db = CreateComputedColumnDatabase(Option<string>.Some("([first_name] + ' ' + [last_name])"), storage);

        var importedDb = await RoundTripAsync(db);

        var tables = await importedDb.GetAllTables();
        var column = tables.Single().Columns.Single(c => c.Name.LocalName == "test_computed_column");

        Assert.That(column.ComputedStorage, Is.EqualTo(storage));
    }

    [Test]
    public static async Task SerializeDeserialize_WhenComputedColumnRoundTripped_PreservesJsonStructure()
    {
        var db = CreateComputedColumnDatabase(Option<string>.Some("([first_name] + ' ' + [last_name])"));

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy());

        await using var jsonOutputStream2 = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream2, importedDb);
        var reExportedJson = Encoding.UTF8.GetString(jsonOutputStream2.ToArray());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(reExportedJson, Is.Not.Null);
            Assert.That(reExportedJson, Is.Not.Empty);
            Assert.That(reExportedJson, Is.EqualTo(json));
        }
    }


    [Test]
    public static async Task SerializeDeserialize_WhenFilteredIndexRoundTripped_PreservesFilterDefinition()
    {
        const string filterDefinition = "([first_name] IS NOT NULL)";
        var db = CreateIndexDatabase(Option<string>.Some(filterDefinition));

        var importedDb = await RoundTripAsync(db);

        var tables = await importedDb.GetAllTables();
        var index = tables.Single().Indexes.Single();

        Assert.That(index.FilterDefinition.UnwrapSome(), Is.EqualTo(filterDefinition));
    }

    [Test]
    public static async Task SerializeDeserialize_WhenUnfilteredIndexRoundTripped_PreservesMissingFilterDefinition()
    {
        var db = CreateIndexDatabase(Option<string>.None);

        var importedDb = await RoundTripAsync(db);

        var tables = await importedDb.GetAllTables();
        var index = tables.Single().Indexes.Single();

        Assert.That(index.FilterDefinition.IsNone, Is.True);
    }

    [Test]
    public static async Task Serialize_WhenIndexUnfiltered_OmitsFilterDefinition()
    {
        var db = CreateIndexDatabase(Option<string>.None);

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        Assert.That(json, Does.Not.Contain("FilterDefinition"));
    }

    [Test]
    public static async Task SerializeDeserialize_WhenFilteredIndexRoundTripped_PreservesJsonStructure()
    {
        var db = CreateIndexDatabase(Option<string>.Some("([first_name] IS NOT NULL)"));

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy());

        await using var jsonOutputStream2 = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream2, importedDb);
        var reExportedJson = Encoding.UTF8.GetString(jsonOutputStream2.ToArray());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(reExportedJson, Is.Not.Null);
            Assert.That(reExportedJson, Is.Not.Empty);
            Assert.That(reExportedJson, Is.EqualTo(json));
        }
    }

    [Test]
    public static async Task SerializeDeserialize_WhenIndexWithPhysicalPropertiesRoundTripped_PreservesProperties()
    {
        var db = CreateDetailedIndexDatabase();

        var importedDb = await RoundTripAsync(db);

        var tables = await importedDb.GetAllTables();
        var index = tables.Single().Indexes.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(index.IndexType, Is.EqualTo(IndexType.Gin));
            Assert.That(index.FillFactor.UnwrapSome(), Is.EqualTo(80));
            Assert.That(index.IsValid, Is.False);
            Assert.That(index.IsVisible, Is.False);
        }
    }

    [Test]
    public static async Task SerializeDeserialize_WhenIndexColumnWithOptionsRoundTripped_PreservesOptions()
    {
        var db = CreateDetailedIndexDatabase();

        var importedDb = await RoundTripAsync(db);

        var tables = await importedDb.GetAllTables();
        var indexColumn = tables.Single().Indexes.Single().Columns.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(indexColumn.NullOrder, Is.EqualTo(IndexColumnNullOrder.NullsFirst));
            Assert.That(indexColumn.Collation.UnwrapSome().LocalName, Is.EqualTo("en_US"));
            Assert.That(indexColumn.PrefixLength.UnwrapSome(), Is.EqualTo(12));
        }
    }

    [Test]
    public static async Task SerializeDeserialize_WhenKeyWithBackingIndexRoundTripped_PreservesBackingIndex()
    {
        var db = CreateDetailedIndexDatabase();

        var importedDb = await RoundTripAsync(db);

        var tables = await importedDb.GetAllTables();
        var primaryKey = tables.Single().PrimaryKey.UnwrapSome();
        var backingIndex = primaryKey.BackingIndex.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(backingIndex.Name.LocalName, Is.EqualTo("test_pk_index"));
            Assert.That(backingIndex.IndexType, Is.EqualTo(IndexType.Clustered));
            Assert.That(backingIndex.IsUnique, Is.True);
        }
    }

    [Test]
    public static async Task SerializeDeserialize_WhenKeyWithoutBackingIndexRoundTripped_PreservesMissingBackingIndex()
    {
        var db = CreateIndexDatabase(Option<string>.None);

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        Assert.That(json, Does.Not.Contain("BackingIndex"));
    }

    [Test]
    public static async Task SerializeDeserialize_WhenTriggerRoundTripped_PreservesGranularityConditionAndUpdateColumns()
    {
        var db = CreateTriggerDatabase();

        var importedDb = await RoundTripAsync(db);

        var tables = await importedDb.GetAllTables();
        var trigger = tables.Single().Triggers.Single(t => t.Name.LocalName == "test_detailed_trigger");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(trigger.QueryTiming, Is.EqualTo(TriggerQueryTiming.Compound));
            Assert.That(trigger.TriggerEvent, Is.EqualTo(TriggerEvent.Update | TriggerEvent.Truncate | TriggerEvent.Other));
            Assert.That(trigger.Granularity, Is.EqualTo(TriggerGranularity.Row));
            Assert.That(trigger.Condition.UnwrapSome(), Is.EqualTo("new.first_name is not null"));
            Assert.That(trigger.UpdateColumns.Select(static c => c.LocalName), Is.EqualTo(new[] { "first_name" }));
        }
    }

    [Test]
    public static async Task SerializeDeserialize_WhenPlainTriggerRoundTripped_PreservesUnknownGranularityAndEmptyOptionals()
    {
        var db = CreateTriggerDatabase();

        var importedDb = await RoundTripAsync(db);

        var tables = await importedDb.GetAllTables();
        var trigger = tables.Single().Triggers.Single(t => t.Name.LocalName == "test_plain_trigger");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(trigger.Granularity, Is.EqualTo(TriggerGranularity.Unknown));
            Assert.That(trigger.Condition.IsNone, Is.True);
            Assert.That(trigger.UpdateColumns, Is.Empty);
        }
    }

    [Test]
    public static async Task SerializeDeserialize_WhenConstraintStateRoundTripped_PreservesValidationAndDeferrability()
    {
        var db = CreateConstraintStateDatabase();

        var importedDb = await RoundTripAsync(db);

        var table = (await importedDb.GetAllTables()).Single();
        var primaryKey = table.PrimaryKey.UnwrapSome();
        var check = table.Checks.Single();
        var relationalKey = table.ParentKeys.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(primaryKey.IsValidated, Is.True);
            Assert.That(primaryKey.Deferrability, Is.EqualTo(ConstraintDeferrability.DeferrableInitiallyDeferred));
            Assert.That(check.IsValidated, Is.False);
            Assert.That(check.Deferrability, Is.EqualTo(ConstraintDeferrability.NotDeferrable));
            Assert.That(relationalKey.ChildKey.IsValidated, Is.False);
            Assert.That(relationalKey.ChildKey.Deferrability, Is.EqualTo(ConstraintDeferrability.DeferrableInitiallyImmediate));
            Assert.That(relationalKey.MatchType, Is.EqualTo(ForeignKeyMatchType.Full));
            Assert.That(relationalKey.SetNullColumns.Select(static c => c.Name.LocalName), Is.EqualTo(new[] { "first_name" }));
        }
    }

    [Test]
    public static async Task SerializeDeserialize_WhenConstraintStateOmittedFromJson_DefaultsToValidatedAndNotDeferrable()
    {
        const string json = """
            {
              "IdentifierDefaults": { "Server": null, "Database": null, "Schema": "main" },
              "Tables": [
                {
                  "TableName": { "Schema": "main", "LocalName": "test_table_name" },
                  "Columns": [],
                  "Checks": [ { "CheckName": { "LocalName": "test_check" }, "Definition": "1 = 1", "IsEnabled": true } ],
                  "Indexes": [],
                  "UniqueKeys": [],
                  "ParentKeys": [],
                  "ChildKeys": [],
                  "Triggers": []
                }
              ],
              "Views": [],
              "Sequences": [],
              "Synonyms": [],
              "Routines": []
            }
            """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var db = await Serializer.DeserializeAsync(stream, new VerbatimIdentifierResolutionStrategy());

        var check = (await db.GetAllTables()).Single().Checks.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(check.IsValidated, Is.True);
            Assert.That(check.Deferrability, Is.EqualTo(ConstraintDeferrability.NotDeferrable));
        }
    }

    [Test]
    public static async Task SerializeDeserialize_WhenIndexWithPhysicalPropertiesRoundTripped_PreservesJsonStructure()
    {
        var db = CreateDetailedIndexDatabase();

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy());

        await using var jsonOutputStream2 = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream2, importedDb);
        var reExportedJson = Encoding.UTF8.GetString(jsonOutputStream2.ToArray());

        Assert.That(reExportedJson, Is.EqualTo(json));
    }

    [Test]
    public static async Task SerializeDeserialize_WhenIdentityColumnRoundTripped_PreservesIdentityMetadata()
    {
        var db = CreateIdentityColumnDatabase();

        var importedDb = await RoundTripAsync(db);

        var tables = await importedDb.GetAllTables();
        var autoIncrement = tables.Single().Columns.Single().AutoIncrement.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(autoIncrement.InitialValue, Is.EqualTo(10));
            Assert.That(autoIncrement.Increment, Is.EqualTo(5));
            Assert.That(autoIncrement.Generation, Is.EqualTo(IdentityGeneration.Always));
            Assert.That(autoIncrement.MinValue.UnwrapSome(), Is.EqualTo(-100));
            Assert.That(autoIncrement.MaxValue.UnwrapSome(), Is.EqualTo(9999));
            Assert.That(autoIncrement.Cycle, Is.True);
            Assert.That(autoIncrement.SequenceName.UnwrapSome().LocalName, Is.EqualTo("test_sequence"));
        }
    }

    [Test]
    public static async Task SerializeDeserialize_WhenIdentityColumnRoundTripped_PreservesJsonStructure()
    {
        var db = CreateIdentityColumnDatabase();

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy());

        await using var jsonOutputStream2 = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream2, importedDb);
        var reExportedJson = Encoding.UTF8.GetString(jsonOutputStream2.ToArray());

        Assert.That(reExportedJson, Is.EqualTo(json));
    }

    [Test]
    public static async Task SerializeDeserialize_WhenTableWithoutPrimaryKeyRoundTripped_PreservesMissingPrimaryKey()
    {
        var db = CreatePrimaryKeyDatabase(hasPrimaryKey: false);

        var importedDb = await RoundTripAsync(db);

        var tables = await importedDb.GetAllTables();

        Assert.That(tables.Single().PrimaryKey.IsNone, Is.True);
    }

    [Test]
    public static async Task SerializeDeserialize_WhenTableWithPrimaryKeyRoundTripped_PreservesPrimaryKey()
    {
        var db = CreatePrimaryKeyDatabase(hasPrimaryKey: true);

        var importedDb = await RoundTripAsync(db);

        var tables = await importedDb.GetAllTables();
        var primaryKey = tables.Single().PrimaryKey.UnwrapSome();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(primaryKey.Name.UnwrapSome().LocalName, Is.EqualTo("test_primary_key"));
            Assert.That(primaryKey.Columns.Single().Name.LocalName, Is.EqualTo("first_name"));
        }
    }

    [Test]
    public static async Task Serialize_WhenTableHasNoPrimaryKey_OmitsPrimaryKey()
    {
        var db = CreatePrimaryKeyDatabase(hasPrimaryKey: false);

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        Assert.That(json, Does.Not.Contain("PrimaryKey"));
    }

    [Test]
    public static async Task SerializeDeserialize_WhenTableWithoutPrimaryKeyRoundTripped_PreservesJsonStructure()
    {
        var db = CreatePrimaryKeyDatabase(hasPrimaryKey: false);

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy());

        await using var jsonOutputStream2 = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream2, importedDb);
        var reExportedJson = Encoding.UTF8.GetString(jsonOutputStream2.ToArray());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(reExportedJson, Is.Not.Null);
            Assert.That(reExportedJson, Is.Not.Empty);
            Assert.That(reExportedJson, Is.EqualTo(json));
        }
    }

    [Test]
    public static async Task DeserializeAsync_GivenWellFormedTableDefinition_ParsesWithoutError()
    {
        var json = CreateTableDatabaseJson(ValidTableNameJson, ValidColumnsJson, ValidChecksJson);

        var importedDb = await DeserializeJsonAsync(json);
        var tables = await importedDb.GetAllTables();

        Assert.That(tables.Single().Name.LocalName, Is.EqualTo("test_table_name"));
    }

    [Test]
    public static void DeserializeAsync_WhenColumnTypeMissing_ThrowsJsonExceptionNamingProperty()
    {
        const string columnsJson = """
            [ { "ColumnName": { "LocalName": "test_column_name" }, "IsNullable": false } ]
            """;
        var json = CreateTableDatabaseJson(ValidTableNameJson, columnsJson, ValidChecksJson);

        Assert.That(
            async () => await DeserializeJsonAsync(json),
            Throws.TypeOf<JsonException>().With.Message.Contains("'Type'")
        );
    }

    [Test]
    public static void DeserializeAsync_WhenCheckDefinitionMissing_ThrowsJsonExceptionNamingProperty()
    {
        const string checksJson = """
            [ { "CheckName": { "LocalName": "test_check_name" }, "IsEnabled": true } ]
            """;
        var json = CreateTableDatabaseJson(ValidTableNameJson, ValidColumnsJson, checksJson);

        Assert.That(
            async () => await DeserializeJsonAsync(json),
            Throws.TypeOf<JsonException>().With.Message.Contains("'Definition'")
        );
    }

    [Test]
    public static void DeserializeAsync_WhenTableNameNull_ThrowsJsonExceptionNamingPropertyPath()
    {
        var json = CreateTableDatabaseJson("null", ValidColumnsJson, ValidChecksJson);

        Assert.That(
            async () => await DeserializeJsonAsync(json),
            Throws.TypeOf<JsonException>()
                .With.Message.Contains("'TableName'")
                .And.Message.Contains("$.Tables[0].TableName")
        );
    }

    [Test]
    public static void DeserializeAsync_WhenIdentifierHasNoComponents_ThrowsJsonExceptionNamingProperty()
    {
        var json = CreateTableDatabaseJson("{ }", ValidColumnsJson, ValidChecksJson);

        Assert.That(
            async () => await DeserializeJsonAsync(json),
            Throws.TypeOf<JsonException>().With.Message.Contains("'LocalName'")
        );
    }

    [Test]
    public static void DeserializeAsync_WhenClrTypeNameUnresolvable_ThrowsExceptionNamingTypeName()
    {
        var json = CreateTableDatabaseJson(ValidTableNameJson, CreateClrTypeNameColumnsJson("Some.Unresolvable.Type"), ValidChecksJson);

        Assert.That(
            async () => await DeserializeJsonAsync(json),
            Throws.InvalidOperationException
                .With.Message.Contains("Some.Unresolvable.Type")
                .And.Message.Contains("test_type_name")
        );
    }

    [Test]
    public static void DeserializeAsync_WhenClrTypeNameMalformed_ThrowsExceptionNamingTypeName()
    {
        var json = CreateTableDatabaseJson(ValidTableNameJson, CreateClrTypeNameColumnsJson("a, b, c, d, e"), ValidChecksJson);

        Assert.That(
            async () => await DeserializeJsonAsync(json),
            Throws.InvalidOperationException
                .With.Message.Contains("a, b, c, d, e")
                .And.Message.Contains("test_type_name")
        );
    }

    // assemblies are never loaded on demand, so a document cannot name one to have it located and loaded
    [Test]
    public static void DeserializeAsync_WhenClrTypeNameQualifiedByUnloadedAssembly_ThrowsExceptionNamingTypeName()
    {
        var json = CreateTableDatabaseJson(ValidTableNameJson, CreateClrTypeNameColumnsJson("System.String, Not.A.Loaded.Assembly"), ValidChecksJson);

        Assert.That(
            async () => await DeserializeJsonAsync(json),
            Throws.InvalidOperationException
                .With.Message.Contains("System.String, Not.A.Loaded.Assembly")
                .And.Message.Contains("test_type_name")
        );
    }

    [Test]
    public static async Task DeserializeAsync_WhenClrTypeNameQualifiedByLoadedAssembly_ResolvesClrType()
    {
        const string clrTypeName = "SJP.Schematic.Core.Identifier, SJP.Schematic.Core";
        var json = CreateTableDatabaseJson(ValidTableNameJson, CreateClrTypeNameColumnsJson(clrTypeName), ValidChecksJson);

        var importedDb = await DeserializeJsonAsync(json);
        var tables = await importedDb.GetAllTables();
        var column = tables.Single().Columns.Single();

        Assert.That(column.Type.ClrType, Is.EqualTo(typeof(Identifier)));
    }

    [Test]
    public static async Task DeserializeAsync_WhenClrTypeNameAbsent_DefaultsToObjectClrType()
    {
        const string columnsJson = """
            [
                {
                    "ColumnName": { "LocalName": "test_column_name" },
                    "IsNullable": false,
                    "Type": {
                        "TypeName": { "LocalName": "test_type_name" },
                        "DataType": "String",
                        "Definition": "varchar(100)",
                        "IsFixedLength": false,
                        "MaxLength": 100
                    }
                }
            ]
            """;
        var json = CreateTableDatabaseJson(ValidTableNameJson, columnsJson, ValidChecksJson);

        var importedDb = await DeserializeJsonAsync(json);
        var tables = await importedDb.GetAllTables();
        var column = tables.Single().Columns.Single();

        Assert.That(column.Type.ClrType, Is.EqualTo(typeof(object)));
    }

    [Test]
    public static async Task SerializeDeserialize_WhenClrTypeOutsideCoreLibraryRoundTripped_PreservesClrType()
    {
        var db = CreateColumnClrTypeDatabase(typeof(ColumnClrType));

        var importedDb = await RoundTripAsync(db);

        var tables = await importedDb.GetAllTables();
        var column = tables.Single().Columns.Single();

        Assert.That(column.Type.ClrType, Is.EqualTo(typeof(ColumnClrType)));
    }

    // the CLR type name is the member under test, so it is the only part of the column that varies
    private static string CreateClrTypeNameColumnsJson(string clrTypeName) => $$"""
        [
            {
                "ColumnName": { "LocalName": "test_column_name" },
                "IsNullable": false,
                "Type": {
                    "TypeName": { "LocalName": "test_type_name" },
                    "DataType": "String",
                    "Definition": "varchar(100)",
                    "IsFixedLength": false,
                    "MaxLength": 100,
                    "ClrTypeName": "{{clrTypeName}}"
                }
            }
        ]
        """;

    private const string ValidTableNameJson = """{ "Schema": "main", "LocalName": "test_table_name" }""";

    private const string ValidColumnsJson = """
        [
            {
                "ColumnName": { "LocalName": "test_column_name" },
                "IsNullable": false,
                "Type": {
                    "TypeName": { "LocalName": "varchar" },
                    "DataType": "String",
                    "Definition": "varchar(100)",
                    "IsFixedLength": false,
                    "MaxLength": 100,
                    "ClrTypeName": "System.String"
                }
            }
        ]
        """;

    private const string ValidChecksJson = """
        [ { "CheckName": { "LocalName": "test_check_name" }, "Definition": "test_column_name IS NOT NULL", "IsEnabled": true } ]
        """;

    // each malformed-JSON test starts from a document that is complete apart from the one member under test
    private static string CreateTableDatabaseJson(string tableNameJson, string columnsJson, string checksJson) => $$"""
        {
            "IdentifierDefaults": { "Schema": "main" },
            "Tables": [
                {
                    "TableName": {{tableNameJson}},
                    "PrimaryKey": null,
                    "Columns": {{columnsJson}},
                    "Checks": {{checksJson}},
                    "Indexes": [],
                    "UniqueKeys": [],
                    "ParentKeys": [],
                    "ChildKeys": [],
                    "Triggers": []
                }
            ],
            "Views": [],
            "Sequences": [],
            "Synonyms": [],
            "Routines": []
        }
        """;

    private static async Task<IRelationalDatabase> DeserializeJsonAsync(string json)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return await Serializer.DeserializeAsync(stream, new VerbatimIdentifierResolutionStrategy());
    }

    private static async Task<IRelationalDatabase> RoundTripAsync(IRelationalDatabase database)
    {
        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, database);

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        return await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy());
    }

    private static IRelationalDatabase CreateColumnClrTypeDatabase(Type clrType)
    {
        var columnType = new ColumnDataType(
            "varchar",
            DataType.String,
            "varchar(100)",
            clrType,
            false,
            100,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );

        var firstNameColumn = new DatabaseColumn("first_name", columnType, false, Option<string>.None, Option<IAutoIncrement>.None);

        var table = new RelationalDatabaseTable(
            "test_table_name",
            [firstNameColumn],
            Option<IDatabaseKey>.None,
            [],
            [],
            [],
            [],
            [],
            []
        );

        return new RelationalDatabase(
            new IdentifierDefaults(null, null, "main"),
            new VerbatimIdentifierResolutionStrategy(),
            [table],
            [],
            [],
            [],
            []
        );
    }

    // a CLR type declared outside the core library, and outside every assembly the serializer references
    private sealed class ColumnClrType;

    private static IRelationalDatabase CreateComputedColumnDatabase(Option<string> definition)
        => CreateComputedColumnDatabase(definition, ComputedColumnStorage.Stored);

    private static IRelationalDatabase CreateComputedColumnDatabase(Option<string> definition, ComputedColumnStorage storage)
    {
        var columnType = new ColumnDataType(
            "varchar",
            DataType.String,
            "varchar(100)",
            typeof(string),
            false,
            100,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );

        var firstNameColumn = new DatabaseColumn("first_name", columnType, false, Option<string>.None, Option<IAutoIncrement>.None);
        var columns = new List<IDatabaseColumn>
        {
            firstNameColumn,
            new DatabaseColumn("test_computed_column", columnType, true, Option<string>.None, Option<IAutoIncrement>.None, true, definition, storage),
        };

        var table = new RelationalDatabaseTable(
            "test_table_name",
            columns,
            Option<IDatabaseKey>.None,
            [],
            [],
            [],
            [],
            [],
            []
        );

        return new RelationalDatabase(
            new IdentifierDefaults(null, null, "main"),
            new VerbatimIdentifierResolutionStrategy(),
            [table],
            [],
            [],
            [],
            []
        );
    }

    private static IRelationalDatabase CreatePrimaryKeyDatabase(bool hasPrimaryKey)
    {
        var columnType = new ColumnDataType(
            "varchar",
            DataType.String,
            "varchar(100)",
            typeof(string),
            false,
            100,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );

        var firstNameColumn = new DatabaseColumn("first_name", columnType, false, Option<string>.None, Option<IAutoIncrement>.None);

        var primaryKey = hasPrimaryKey
            ? Option<IDatabaseKey>.Some(new DatabaseKey(
                Option<Identifier>.Some("test_primary_key"),
                DatabaseKeyType.Primary,
                [firstNameColumn],
                true
            ))
            : Option<IDatabaseKey>.None;

        var table = new RelationalDatabaseTable(
            "test_table_name",
            [firstNameColumn],
            primaryKey,
            [],
            [],
            [],
            [],
            [],
            []
        );

        return new RelationalDatabase(
            new IdentifierDefaults(null, null, "main"),
            new VerbatimIdentifierResolutionStrategy(),
            [table],
            [],
            [],
            [],
            []
        );
    }

    // A table whose constraints carry every non-default state the model can express: a deferrable
    // primary key, an unvalidated check, and an unvalidated deferrable foreign key with a match type
    // and an ON DELETE SET NULL column subset.
    private static IRelationalDatabase CreateConstraintStateDatabase()
    {
        var columnType = new ColumnDataType(
            "varchar",
            DataType.String,
            "varchar(100)",
            typeof(string),
            false,
            100,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );

        var firstNameColumn = new DatabaseColumn("first_name", columnType, true, Option<string>.None, Option<IAutoIncrement>.None);

        var primaryKey = new DatabaseKey(
            Option<Identifier>.Some("test_pk_name"),
            DatabaseKeyType.Primary,
            [firstNameColumn],
            true,
            Option<IDatabaseIndex>.None,
            true,
            ConstraintDeferrability.DeferrableInitiallyDeferred
        );

        var foreignKey = new DatabaseKey(
            Option<Identifier>.Some("test_fk_name"),
            DatabaseKeyType.Foreign,
            [firstNameColumn],
            true,
            Option<IDatabaseIndex>.None,
            false,
            ConstraintDeferrability.DeferrableInitiallyImmediate
        );

        var check = new DatabaseCheckConstraint(
            Option<Identifier>.Some("test_check_name"),
            "first_name is not null",
            true,
            false,
            ConstraintDeferrability.NotDeferrable
        );

        var relationalKey = new DatabaseRelationalKey(
            "test_table_name",
            foreignKey,
            "test_table_name",
            primaryKey,
            ReferentialAction.SetNull,
            ReferentialAction.NoAction,
            ForeignKeyMatchType.Full,
            [firstNameColumn]
        );

        var table = new RelationalDatabaseTable(
            "test_table_name",
            [firstNameColumn],
            Option<IDatabaseKey>.Some(primaryKey),
            [],
            [relationalKey],
            [],
            [],
            [check],
            []
        );

        return new RelationalDatabase(
            new IdentifierDefaults(null, null, "main"),
            new VerbatimIdentifierResolutionStrategy(),
            [table],
            [],
            [],
            [],
            []
        );
    }

    // A table with one trigger carrying every fact the model can express, alongside a trigger that
    // carries none of them, so that both the populated and the defaulted paths are covered.
    private static IRelationalDatabase CreateTriggerDatabase()
    {
        var columnType = new ColumnDataType(
            "varchar",
            DataType.String,
            "varchar(100)",
            typeof(string),
            false,
            100,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );

        var firstNameColumn = new DatabaseColumn("first_name", columnType, true, Option<string>.None, Option<IAutoIncrement>.None);

        var detailedTrigger = new DatabaseTrigger(
            "test_detailed_trigger",
            "create trigger test_detailed_trigger ...",
            TriggerQueryTiming.Compound,
            TriggerEvent.Update | TriggerEvent.Truncate | TriggerEvent.Other,
            true,
            TriggerGranularity.Row,
            Option<string>.Some("new.first_name is not null"),
            ["first_name"]
        );

        var plainTrigger = new DatabaseTrigger(
            "test_plain_trigger",
            "create trigger test_plain_trigger ...",
            TriggerQueryTiming.After,
            TriggerEvent.Insert,
            false
        );

        var table = new RelationalDatabaseTable(
            "test_table_name",
            [firstNameColumn],
            Option<IDatabaseKey>.None,
            [],
            [],
            [],
            [],
            [],
            [detailedTrigger, plainTrigger]
        );

        return new RelationalDatabase(
            new IdentifierDefaults(null, null, "main"),
            new VerbatimIdentifierResolutionStrategy(),
            [table],
            [],
            [],
            [],
            []
        );
    }

    private static IRelationalDatabase CreateDetailedIndexDatabase()
    {
        var columnType = new ColumnDataType(
            "varchar",
            DataType.String,
            "varchar(100)",
            typeof(string),
            false,
            100,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );

        var firstNameColumn = new DatabaseColumn("first_name", columnType, false, Option<string>.None, Option<IAutoIncrement>.None);

        var index = new DatabaseIndex(
            "test_index_name",
            false,
            [
                new DatabaseIndexColumn(
                    "first_name",
                    firstNameColumn,
                    IndexColumnOrder.Descending,
                    IndexColumnNullOrder.NullsFirst,
                    Option<Identifier>.Some("en_US"),
                    Option<int>.Some(12)
                ),
            ],
            [],
            false,
            Option<string>.None,
            IndexType.Gin,
            Option<int>.Some(80),
            false,
            false
        );

        var backingIndex = new DatabaseIndex(
            "test_pk_index",
            true,
            [new DatabaseIndexColumn("first_name", firstNameColumn, IndexColumnOrder.Ascending)],
            [],
            true,
            Option<string>.None,
            IndexType.Clustered,
            Option<int>.None,
            true,
            true
        );

        var primaryKey = new DatabaseKey(
            Option<Identifier>.Some("test_pk_name"),
            DatabaseKeyType.Primary,
            [firstNameColumn],
            true,
            Option<IDatabaseIndex>.Some(backingIndex)
        );

        var table = new RelationalDatabaseTable(
            "test_table_name",
            [firstNameColumn],
            Option<IDatabaseKey>.Some(primaryKey),
            [],
            [],
            [],
            [index],
            [],
            []
        );

        return new RelationalDatabase(
            new IdentifierDefaults(null, null, "main"),
            new VerbatimIdentifierResolutionStrategy(),
            [table],
            [],
            [],
            [],
            []
        );
    }

    private static IRelationalDatabase CreateIndexDatabase(Option<string> filterDefinition)
    {
        var columnType = new ColumnDataType(
            "varchar",
            DataType.String,
            "varchar(100)",
            typeof(string),
            false,
            100,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );

        var firstNameColumn = new DatabaseColumn("first_name", columnType, false, Option<string>.None, Option<IAutoIncrement>.None);

        var index = new DatabaseIndex(
            "test_index_name",
            false,
            [new DatabaseIndexColumn("first_name", firstNameColumn, IndexColumnOrder.Ascending)],
            [],
            true,
            filterDefinition
        );

        var table = new RelationalDatabaseTable(
            "test_table_name",
            [firstNameColumn],
            Option<IDatabaseKey>.None,
            [],
            [],
            [],
            [index],
            [],
            []
        );

        return new RelationalDatabase(
            new IdentifierDefaults(null, null, "main"),
            new VerbatimIdentifierResolutionStrategy(),
            [table],
            [],
            [],
            [],
            []
        );
    }

    [Test]
    public async Task SerializeDeserialize_WhenRoutineWithSignatureRoundTripped_ExportsAndParsesWithoutError()
    {
        var db = GetDatabaseWithOverloadedRoutine();

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy());

        db.Should().BeEquivalentTo(importedDb);
    }

    [Test]
    public async Task SerializeDeserialize_WhenRoutineWithSignatureRoundTripped_PreservesJsonStructure()
    {
        var db = GetDatabaseWithOverloadedRoutine();

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy());

        await using var jsonOutputStream2 = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream2, importedDb);
        var reExportedJson = Encoding.UTF8.GetString(jsonOutputStream2.ToArray());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(reExportedJson, Is.Not.Null);
            Assert.That(reExportedJson, Is.Not.Empty);
            Assert.That(reExportedJson, Is.EqualTo(json));
        }
    }

    [Test]
    public async Task SerializeDeserialize_WhenRoutineWithSignatureRoundTripped_PreservesSignature()
    {
        var db = GetDatabaseWithOverloadedRoutine();

        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db);

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy());

        var routine = (await importedDb.GetAllRoutines()).Single();
        var parameter = routine.Parameters.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(routine.RoutineType, Is.EqualTo(RoutineType.Function));
            Assert.That(routine.Language.UnwrapSome(), Is.EqualTo("plpgsql"));
            Assert.That(routine.ReturnType.UnwrapSome().TypeName.LocalName, Is.EqualTo("integer"));
            Assert.That(parameter.Name.UnwrapSome().LocalName, Is.EqualTo("test_parameter"));
            Assert.That(parameter.Direction, Is.EqualTo(RoutineParameterDirection.InputOutput));
            Assert.That(parameter.DefaultValue.UnwrapSome(), Is.EqualTo("1"));
            Assert.That(parameter.Ordinal, Is.EqualTo(1));
            // the second overload takes no arguments, so it also proves an empty parameter list survives
            Assert.That(routine.Overloads.Select(static o => o.Parameters.Count), Is.EqualTo(new[] { 1, 0 }));
        }
    }

    private static IRelationalDatabase GetDatabaseWithOverloadedRoutine()
    {
        var integerType = new ColumnDataType(
            "integer",
            DataType.Integer,
            "integer",
            typeof(int),
            false,
            0,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );
        var parameter = new DatabaseRoutineParameter(
            Option<Identifier>.Some("test_parameter"),
            integerType,
            RoutineParameterDirection.InputOutput,
            Option<string>.Some("1"),
            1
        );
        var overloads = new IDatabaseRoutineOverload[]
        {
            new DatabaseRoutineOverload("create function test_routine_name(integer) ...", [parameter], Option<IDbType>.Some(integerType)),
            new DatabaseRoutineOverload("create function test_routine_name() ...", [], Option<IDbType>.Some(integerType)),
        };
        var routine = new DatabaseRoutine(
            "test_routine_name",
            "test_routine_definition",
            RoutineType.Function,
            Option<string>.Some("plpgsql"),
            [parameter],
            Option<IDbType>.Some(integerType),
            overloads
        );

        return new RelationalDatabase(
            new IdentifierDefaults(null, null, "main"),
            new VerbatimIdentifierResolutionStrategy(),
            [],
            [],
            [],
            [],
            [routine]
        );
    }

    private static IRelationalDatabase CreateIdentityColumnDatabase()
    {
        var columnType = new ColumnDataType(
            "integer",
            DataType.Integer,
            "integer",
            typeof(int),
            false,
            4,
            Option<INumericPrecision>.None,
            Option<Identifier>.None
        );

        var autoIncrement = new AutoIncrement(
            10,
            5,
            IdentityGeneration.Always,
            Option<decimal>.Some(-100),
            Option<decimal>.Some(9999),
            true,
            Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier("test_schema", "test_sequence"))
        );

        var idColumn = new DatabaseColumn(
            "id",
            columnType,
            false,
            Option<string>.None,
            Option<IAutoIncrement>.Some(autoIncrement)
        );

        var table = new RelationalDatabaseTable(
            "test_table_name",
            [idColumn],
            Option<IDatabaseKey>.None,
            [],
            [],
            [],
            [],
            [],
            []
        );

        return new RelationalDatabase(
            new IdentifierDefaults(null, null, "main"),
            new VerbatimIdentifierResolutionStrategy(),
            [table],
            [],
            [],
            [],
            []
        );
    }
}