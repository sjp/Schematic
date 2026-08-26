using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            Assert.That(column, Is.InstanceOf<IDatabaseComputedColumn>());
            Assert.That(((IDatabaseComputedColumn)column).Definition.UnwrapSome(), Is.EqualTo(definition));
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
            Assert.That(column, Is.InstanceOf<IDatabaseComputedColumn>());
            Assert.That(((IDatabaseComputedColumn)column).Definition.IsNone, Is.True);
        }
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

    private static async Task<IRelationalDatabase> RoundTripAsync(IRelationalDatabase database)
    {
        await using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, database);

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        return await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy());
    }

    private static IRelationalDatabase CreateComputedColumnDatabase(Option<string> definition)
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
            new DatabaseComputedColumn("test_computed_column", columnType, true, Option<string>.None, definition),
        };

        // a primary key is present because a table without one cannot currently be round-tripped,
        // see issues/serialization-missing-primary-key-round-trip.md
        var primaryKey = new DatabaseKey(
            Option<Identifier>.Some("test_primary_key"),
            DatabaseKeyType.Primary,
            [firstNameColumn],
            true
        );

        var table = new RelationalDatabaseTable(
            "test_table_name",
            columns,
            Option<IDatabaseKey>.Some(primaryKey),
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