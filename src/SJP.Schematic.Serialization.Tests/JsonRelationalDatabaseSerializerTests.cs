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
            [firstNameColumn],
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

    // a CLR type declared outside the core library, and outside every assembly the serializer references
    private sealed class ColumnClrType;

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

}