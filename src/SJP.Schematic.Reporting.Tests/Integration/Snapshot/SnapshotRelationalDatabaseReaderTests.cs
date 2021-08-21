using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using LanguageExt;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Snapshot;
using SJP.Schematic.Serialization;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration.Snapshot
{
    internal sealed class SnapshotRelationalDatabaseReaderTests : SakilaTest
    {
        private TemporaryDirectory _tempDir;
        private IDbConnectionFactory _connectionFactory;

        private SnapshotRelationalDatabaseReader _databaseReader;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            _tempDir = new TemporaryDirectory();

            var dbPath = Path.Combine(_tempDir.DirectoryPath, "snapshot-relational-database-reader-test.db");

            var builder = new SqliteConnectionStringBuilder { DataSource = dbPath };
            _connectionFactory = new SqliteConnectionFactory(builder.ToString());

            var mapperConfig = new MapperConfiguration(config => config.AddMaps(typeof(JsonRelationalDatabaseSerializer).Assembly));
            var mapper = mapperConfig.CreateMapper();

            _databaseReader = new SnapshotRelationalDatabaseReader(_connectionFactory, mapper);

            var snapshotSchema = new SnapshotSchema(_connectionFactory);
            await snapshotSchema.EnsureSchemaExistsAsync().ConfigureAwait(false);

            var writer = new SnapshotRelationalDatabaseWriter(_connectionFactory, mapper);
            await writer.SnapshotDatabaseObjectsAsync(GetDatabase()).ConfigureAwait(false);
            // also snapshot faked objects
            await writer.SnapshotDatabaseObjectsAsync(SampleDatabase).ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            _tempDir.Dispose();
        }

        [Test]
        public async Task GetAllTables_WhenGivenDatabaseWithTables_MatchesSnapshotSource()
        {
            var tables = await _databaseReader.GetAllTables().ToListAsync().ConfigureAwait(false);

            var sourceTables = await GetDatabase().GetAllTables().ToListAsync().ConfigureAwait(false);

            tables.Should().BeEquivalentTo(sourceTables);
        }

        [Test]
        public async Task GetAllViews_WhenGivenDatabaseWithViews_MatchesSnapshotSource()
        {
            var views = await _databaseReader.GetAllViews().ToListAsync().ConfigureAwait(false);

            var sourceViews = await GetDatabase().GetAllViews().ToListAsync().ConfigureAwait(false);

            views.Should().BeEquivalentTo(sourceViews);
        }

        [Test]
        public async Task GetAllSequences_WhenGivenDatabaseWithSequences_MatchesSnapshotSource()
        {
            var sequences = await _databaseReader.GetAllSequences().ToListAsync().ConfigureAwait(false);

            var sourceSequences = await SampleDatabase.GetAllSequences().ToListAsync().ConfigureAwait(false);

            sequences.Should().BeEquivalentTo(sourceSequences);
        }

        [Test]
        public async Task GetAllSynonyms_WhenGivenDatabaseWithSynonyms_MatchesSnapshotSource()
        {
            var synonyms = await _databaseReader.GetAllSynonyms().ToListAsync().ConfigureAwait(false);

            var sourceSynonyms = await SampleDatabase.GetAllSynonyms().ToListAsync().ConfigureAwait(false);

            synonyms.Should().BeEquivalentTo(sourceSynonyms);
        }

        [Test]
        public async Task GetAllRoutines_WhenGivenDatabaseWithRoutines_MatchesSnapshotSource()
        {
            var sequences = await _databaseReader.GetAllRoutines().ToListAsync().ConfigureAwait(false);

            var sourceSequences = await SampleDatabase.GetAllRoutines().ToListAsync().ConfigureAwait(false);

            sequences.Should().BeEquivalentTo(sourceSequences);
        }

        [Test]
        public async Task GetTable_WhenGivenTableNameForTableThatExists_MatchesSnapshotSource()
        {
            var table = await _databaseReader.GetTable("film").ToOption().ConfigureAwait(false);

            var sourceTable = await GetDatabase().GetTable("film").ToOption().ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(table, OptionIs.Some);

                var viewCommentsValue = table.UnwrapSome();
                var sourceViewCommentsValue = sourceTable.UnwrapSome();

                viewCommentsValue.Should().BeEquivalentTo(sourceViewCommentsValue);
            });
        }

        [Test]
        public async Task GetView_WhenGivenViewNameForViewThatExists_MatchesSnapshotSource()
        {
            var view = await _databaseReader.GetView("film_list").ToOption().ConfigureAwait(false);

            var sourceView = await GetDatabase().GetView("film_list").ToOption().ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(view, OptionIs.Some);

                var tableValue = view.UnwrapSome();
                var sourceTableValue = sourceView.UnwrapSome();

                tableValue.Should().BeEquivalentTo(sourceTableValue);
            });
        }

        [Test]
        public async Task GetSequence_WhenGivenSequenceNameForSequenceThatExists_MatchesSnapshotSource()
        {
            var sequence = await _databaseReader.GetSequence("test_sequence_1").ToOption().ConfigureAwait(false);

            var sourceSequence = await SampleDatabase.GetSequence("test_sequence_1").ToOption().ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(sequence, OptionIs.Some);

                var sequenceValue = sequence.UnwrapSome();
                var sourceSequenceValue = sourceSequence.UnwrapSome();

                sequenceValue.Should().BeEquivalentTo(sourceSequenceValue);
            });
        }

        [Test]
        public async Task GetSynonym_WhenGivenSynonymNameForSynonymThatExists_MatchesSnapshotSource()
        {
            var synonym = await _databaseReader.GetSynonym("test_synonym_1").ToOption().ConfigureAwait(false);

            var sourceSynonym = await SampleDatabase.GetSynonym("test_synonym_1").ToOption().ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(synonym, OptionIs.Some);

                var synonymValue = synonym.UnwrapSome();
                var sourceSynonymValue = sourceSynonym.UnwrapSome();

                synonymValue.Should().BeEquivalentTo(sourceSynonymValue);
            });
        }

        [Test]
        public async Task GetRoutine_WhenGivenRoutineNameForRoutineThatExists_MatchesSnapshotSource()
        {
            var routine = await _databaseReader.GetRoutine("test_routine_1").ToOption().ConfigureAwait(false);

            var sourceRoutine = await SampleDatabase.GetRoutine("test_routine_1").ToOption().ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(routine, OptionIs.Some);

                var synonymValue = routine.UnwrapSome();
                var sourcesynonymValue = sourceRoutine.UnwrapSome();

                synonymValue.Should().BeEquivalentTo(sourcesynonymValue);
            });
        }

        [Test]
        public async Task GetTable_WhenGivenTableNameForTableThatDoesNotExist_ReturnsNone()
        {
            var table = await _databaseReader.GetTable("not_a_table_name").ToOption().ConfigureAwait(false);

            Assert.That(table, OptionIs.None);
        }

        [Test]
        public async Task GetView_WhenGivenViewNameForViewThatDoesNotExist_ReturnsNone()
        {
            var view = await _databaseReader.GetView("not_a_view_name").ToOption().ConfigureAwait(false);

            Assert.That(view, OptionIs.None);
        }

        [Test]
        public async Task GetSequence_WhenGivenSequenceNameForSequenceThatDoesNotExist_ReturnsNone()
        {
            var sequence = await _databaseReader.GetSequence("not_a_sequence_name").ToOption().ConfigureAwait(false);

            Assert.That(sequence, OptionIs.None);
        }

        [Test]
        public async Task GetSynonym_WhenGivenSynonymNameForSynonymThatDoesNotExist_ReturnsNone()
        {
            var synonym = await _databaseReader.GetSynonym("not_a_synonym_name").ToOption().ConfigureAwait(false);

            Assert.That(synonym, OptionIs.None);
        }

        [Test]
        public async Task GetRoutine_WhenGivenRoutineNameForRoutineThatDoesNotExist_ReturnsNone()
        {
            var routine = await _databaseReader.GetRoutine("not_a_routine_name").ToOption().ConfigureAwait(false);

            Assert.That(routine, OptionIs.None);
        }

        private static IRelationalDatabase SampleDatabase { get; } = new RelationalDatabase(
            new IdentifierDefaults(null, null, "main"),
            new VerbatimIdentifierResolutionStrategy(),
            Array.Empty<IRelationalDatabaseTable>(),
            Array.Empty<IDatabaseView>(),
            new[]
            {
                new DatabaseSequence(
                    Identifier.CreateQualifiedIdentifier("main", "test_sequence_1"),
                    1,
                    10,
                    Option<decimal>.Some(-10),
                    Option<decimal>.Some(1000),
                    true,
                    20
                )
            },
            new[]
            {
                new DatabaseSynonym(
                    Identifier.CreateQualifiedIdentifier("main", "test_synonym_1"),
                    Identifier.CreateQualifiedIdentifier("main", "test_sequence_1")
                )
            },
            new[]
            {
                new DatabaseRoutine(
                    Identifier.CreateQualifiedIdentifier("main", "test_routine_1"),
                    "test_routine_definition"
                )
            }
        );
    }
}
