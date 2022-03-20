using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Serialization.Tests;

internal sealed class JsonRelationalDatabaseSerializerTests : SakilaTest
{
    private static IRelationalDatabaseSerializer Serializer { get; } = new JsonRelationalDatabaseSerializer();

    [Test]
    public async Task Serialize_WhenInvoked_ExportsWithoutError()
    {
        var db = GetDatabase();
        using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db).ConfigureAwait(false);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        Assert.Multiple(() =>
        {
            Assert.That(json, Is.Not.Null);
            Assert.That(json, Is.Not.Empty);
        });
    }

    [Test]
    public async Task SerializeDeserialize_WhenEmptyDatabaseRoundTripped_ExportsAndParsesWithoutError()
    {
        var db = new EmptyRelationalDatabase(new IdentifierDefaults(null, null, "main"));

        using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db).ConfigureAwait(false);

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy()).ConfigureAwait(false);

        db.Should().BeEquivalentTo(importedDb);
    }

    [Test]
    public async Task SerializeDeserialize_WhenEmptyDatabaseRoundTripped_PreservesJsonStructure()
    {
        var db = new EmptyRelationalDatabase(new IdentifierDefaults(null, null, "main"));

        using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db).ConfigureAwait(false);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy()).ConfigureAwait(false);

        using var jsonOutputStream2 = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream2, importedDb).ConfigureAwait(false);
        var reExportedJson = Encoding.UTF8.GetString(jsonOutputStream2.ToArray());

        Assert.Multiple(() =>
        {
            Assert.That(reExportedJson, Is.Not.Null);
            Assert.That(reExportedJson, Is.Not.Empty);
            Assert.That(reExportedJson, Is.EqualTo(json));
        });
    }

    [Test]
    public async Task SerializeDeserialize_WhenRoundTripped_ExportsAndParsesWithoutError()
    {
        var db = GetDatabase();

        using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db).ConfigureAwait(false);

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy()).ConfigureAwait(false);

        db.Should().BeEquivalentTo(importedDb);
    }

    [Test]
    public async Task SerializeDeserialize_WhenRoundTripped_PreservesJsonStructure()
    {
        var db = GetDatabase();

        using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db).ConfigureAwait(false);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy()).ConfigureAwait(false);

        using var jsonOutputStream2 = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream2, importedDb).ConfigureAwait(false);
        var reExportedJson = Encoding.UTF8.GetString(jsonOutputStream2.ToArray());

        Assert.Multiple(() =>
        {
            Assert.That(reExportedJson, Is.Not.Null);
            Assert.That(reExportedJson, Is.Not.Empty);
            Assert.That(reExportedJson, Is.EqualTo(json));
        });
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

        using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db).ConfigureAwait(false);

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy()).ConfigureAwait(false);

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

        using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db).ConfigureAwait(false);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy()).ConfigureAwait(false);

        using var jsonOutputStream2 = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream2, importedDb).ConfigureAwait(false);
        var reExportedJson = Encoding.UTF8.GetString(jsonOutputStream2.ToArray());

        Assert.Multiple(() =>
        {
            Assert.That(reExportedJson, Is.Not.Null);
            Assert.That(reExportedJson, Is.Not.Empty);
            Assert.That(reExportedJson, Is.EqualTo(json));
        });
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

        using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db).ConfigureAwait(false);

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy()).ConfigureAwait(false);

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

        using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db).ConfigureAwait(false);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy()).ConfigureAwait(false);

        using var jsonOutputStream2 = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream2, importedDb).ConfigureAwait(false);
        var reExportedJson = Encoding.UTF8.GetString(jsonOutputStream2.ToArray());

        Assert.Multiple(() =>
        {
            Assert.That(reExportedJson, Is.Not.Null);
            Assert.That(reExportedJson, Is.Not.Empty);
            Assert.That(reExportedJson, Is.EqualTo(json));
        });
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

        using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db).ConfigureAwait(false);

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy()).ConfigureAwait(false);

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

        using var jsonOutputStream = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream, db).ConfigureAwait(false);
        var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

        jsonOutputStream.Seek(0, SeekOrigin.Begin);
        var importedDb = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy()).ConfigureAwait(false);

        using var jsonOutputStream2 = new MemoryStream();
        await Serializer.SerializeAsync(jsonOutputStream2, importedDb).ConfigureAwait(false);
        var reExportedJson = Encoding.UTF8.GetString(jsonOutputStream2.ToArray());

        Assert.Multiple(() =>
        {
            Assert.That(reExportedJson, Is.Not.Null);
            Assert.That(reExportedJson, Is.Not.Empty);
            Assert.That(reExportedJson, Is.EqualTo(json));
        });
    }
}
