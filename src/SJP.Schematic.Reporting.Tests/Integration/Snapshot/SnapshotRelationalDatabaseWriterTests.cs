using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using LanguageExt;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Snapshot;
using SJP.Schematic.Serialization.Mapping;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration.Snapshot
{
    internal sealed class SnapshotRelationalDatabaseWriterTests : SakilaTest
    {
        private TemporaryDirectory _tempDir;
        private IDbConnectionFactory _connectionFactory;
        private IMapper _mapper;

        private SnapshotRelationalDatabaseWriter _databaseWriter;

        private static readonly Lazy<JsonSerializerOptions> _settings = new(LoadSettings);

        private static JsonSerializerOptions LoadSettings()
        {
            var settings = new JsonSerializerOptions
            {
                WriteIndented = true,
                IgnoreNullValues = true,
            };
            settings.Converters.Add(new JsonStringEnumConverter());

            return settings;
        }

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            _tempDir = new TemporaryDirectory();

            var dbPath = Path.Combine(_tempDir.DirectoryPath, "snapshot-relational-database-writer-test.db");

            var builder = new SqliteConnectionStringBuilder { DataSource = dbPath };
            _connectionFactory = new SqliteConnectionFactory(builder.ToString());

            var mapperConfig = new MapperConfiguration(config => config.AddMaps(typeof(RelationalDatabaseProfile).Assembly));
            _mapper = mapperConfig.CreateMapper();

            _databaseWriter = new SnapshotRelationalDatabaseWriter(_connectionFactory, _mapper);

            var snapshotSchema = new SnapshotSchema(_connectionFactory);
            await snapshotSchema.EnsureSchemaExistsAsync().ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            _tempDir.Dispose();
        }

        [Test]
        public async Task SnapshotDatabaseObjectsAsync_WhenGivenDatabaseWithTablesAndViews_SnapshotsExpectedData()
        {
            var db = GetDatabase();

            await _databaseWriter.SnapshotDatabaseObjectsAsync(db).ConfigureAwait(false);

            var dbTables = await db.GetAllTables().ToListAsync().ConfigureAwait(false);
            var dbViews = await db.GetAllViews().ToListAsync().ConfigureAwait(false);

            Assert.Multiple(async () =>
            {
                foreach (var table in dbTables)
                {
                    await AssertTableMatchesAsync(table).ConfigureAwait(false);
                }

                foreach (var view in dbViews)
                {
                    await AssertViewMatchesAsync(view).ConfigureAwait(false);
                }
            });
        }

        [Test]
        public async Task SnapshotDatabaseObjectsAsync_WhenGivenDatabaseWithSequences_SnapshotsExpectedData()
        {
            var sequence = new DatabaseSequence(
                Identifier.CreateQualifiedIdentifier("main", "test_sequence_name"),
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

            await _databaseWriter.SnapshotDatabaseObjectsAsync(db).ConfigureAwait(false);

            var dbSequences = await db.GetAllSequences().ToListAsync().ConfigureAwait(false);

            Assert.Multiple(async () =>
            {
                foreach (var dbSequence in dbSequences)
                {
                    await AssertSequenceMatchesAsync(dbSequence).ConfigureAwait(false);
                }
            });
        }

        [Test]
        public async Task SnapshotDatabaseObjectsAsync_WhenGivenDatabaseWithSynonyms_SnapshotsExpectedData()
        {
            var synonym = new DatabaseSynonym(
                Identifier.CreateQualifiedIdentifier("main", "test_synonym_name"),
                Identifier.CreateQualifiedIdentifier("main", "test_target_name")
            );
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

            await _databaseWriter.SnapshotDatabaseObjectsAsync(db).ConfigureAwait(false);

            var dbSynonyms = await db.GetAllSynonyms().ToListAsync().ConfigureAwait(false);

            Assert.Multiple(async () =>
            {
                foreach (var dbSynonym in dbSynonyms)
                {
                    await AssertSynonymMatchesAsync(dbSynonym).ConfigureAwait(false);
                }
            });
        }

        [Test]
        public async Task SnapshotDatabaseObjectsAsync_WhenGivenDatabaseWithRoutines_SnapshotsExpectedData()
        {
            var routine = new DatabaseRoutine(
                Identifier.CreateQualifiedIdentifier("main", "test_routine_name"),
                "test_routine_definition"
            );
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

            await _databaseWriter.SnapshotDatabaseObjectsAsync(db).ConfigureAwait(false);

            var dbRoutines = await db.GetAllRoutines().ToListAsync().ConfigureAwait(false);

            Assert.Multiple(async () =>
            {
                foreach (var dbRoutine in dbRoutines)
                {
                    await AssertRoutineMatchesAsync(dbRoutine).ConfigureAwait(false);
                }
            });
        }

        private async Task AssertTableMatchesAsync(IRelationalDatabaseTable table)
        {
            var snapshotTableJson = await _connectionFactory.ExecuteScalarAsync<string>(
                "SELECT definition_json FROM database_object WHERE local_name = @TableName AND object_type = 'TABLE'",
                new { TableName = table.Name.LocalName },
                CancellationToken.None
            ).ConfigureAwait(false);

            var tableDto = _mapper.Map<Serialization.Dto.RelationalDatabaseTable>(table);
            var tableJson = JsonSerializer.Serialize(tableDto, _settings.Value);

            Assert.That(tableJson, Is.EqualTo(snapshotTableJson));
        }

        private async Task AssertViewMatchesAsync(IDatabaseView view)
        {
            var snapshotViewJson = await _connectionFactory.ExecuteScalarAsync<string>(
                "SELECT definition_json FROM database_object WHERE local_name = @ViewName AND object_type = 'VIEW'",
                new { ViewName = view.Name.LocalName },
                CancellationToken.None
            ).ConfigureAwait(false);

            var viewDto = _mapper.Map<Serialization.Dto.DatabaseView>(view);
            var viewJson = JsonSerializer.Serialize(viewDto, _settings.Value);

            Assert.That(viewJson, Is.EqualTo(snapshotViewJson));
        }

        private async Task AssertSequenceMatchesAsync(IDatabaseSequence sequence)
        {
            var snapshotSequenceJson = await _connectionFactory.ExecuteScalarAsync<string>(
                "SELECT definition_json FROM database_object WHERE local_name = @SequenceName AND object_type = 'SEQUENCE'",
                new { SequenceName = sequence.Name.LocalName },
                CancellationToken.None
            ).ConfigureAwait(false);

            var sequenceDto = _mapper.Map<Serialization.Dto.DatabaseSequence>(sequence);
            var sequenceJson = JsonSerializer.Serialize(sequenceDto, _settings.Value);

            Assert.That(sequenceJson, Is.EqualTo(snapshotSequenceJson));
        }

        private async Task AssertSynonymMatchesAsync(IDatabaseSynonym synonym)
        {
            var snapshotSynonymJson = await _connectionFactory.ExecuteScalarAsync<string>(
                "SELECT definition_json FROM database_object WHERE local_name = @SynonymName AND object_type = 'SYNONYM'",
                new { SynonymName = synonym.Name.LocalName },
                CancellationToken.None
            ).ConfigureAwait(false);

            var synonymJsonDto = JsonSerializer.Deserialize<Serialization.Dto.DatabaseSynonym>(snapshotSynonymJson, _settings.Value);
            var snapshotSynonym = _mapper.Map<DatabaseSynonym>(synonymJsonDto);

            snapshotSynonym.Should().BeEquivalentTo(synonym);

            var synonymDto = _mapper.Map<Serialization.Dto.DatabaseSynonym>(synonym);
            var synonymJson = JsonSerializer.Serialize(synonymDto, _settings.Value);

            Assert.That(synonymJson, Is.EqualTo(snapshotSynonymJson));
        }

        private async Task AssertRoutineMatchesAsync(IDatabaseRoutine routine)
        {
            var snapshotRoutineJson = await _connectionFactory.ExecuteScalarAsync<string>(
                "SELECT definition_json FROM database_object WHERE local_name = @RoutineName AND object_type = 'ROUTINE'",
                new { RoutineName = routine.Name.LocalName },
                CancellationToken.None
            ).ConfigureAwait(false);

            var routineDto = _mapper.Map<Serialization.Dto.DatabaseRoutine>(routine);
            var routineJson = JsonSerializer.Serialize(routineDto, _settings.Value);

            Assert.That(routineJson, Is.EqualTo(snapshotRoutineJson));
        }
    }
}
