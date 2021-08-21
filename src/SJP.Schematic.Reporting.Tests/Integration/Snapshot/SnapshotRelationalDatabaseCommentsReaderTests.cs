using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using LanguageExt;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Reporting.Snapshot;
using SJP.Schematic.Serialization.Mapping;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Integration.Snapshot
{
    internal sealed class SnapshotRelationalDatabaseCommentsReaderTests
    {
        private TemporaryDirectory _tempDir;

        private SnapshotRelationalDatabaseCommentsReader _commentsReader;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            _tempDir = new TemporaryDirectory();

            var dbPath = Path.Combine(_tempDir.DirectoryPath, "snapshot-relational-database-comments-reader-test.db");

            var builder = new SqliteConnectionStringBuilder { DataSource = dbPath };
            var connectionFactory = new SqliteConnectionFactory(builder.ToString());

            var mapperConfig = new MapperConfiguration(config => config.AddMaps(typeof(RelationalDatabaseProfile).Assembly));
            var mapper = mapperConfig.CreateMapper();

            _commentsReader = new SnapshotRelationalDatabaseCommentsReader(connectionFactory, mapper);

            var snapshotSchema = new SnapshotSchema(connectionFactory);
            await snapshotSchema.EnsureSchemaExistsAsync().ConfigureAwait(false);

            var commentsWriter = new SnapshotRelationalDatabaseCommentsWriter(connectionFactory, mapper);
            await commentsWriter.SnapshotDatabaseCommentsAsync(SampleComments).ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            _tempDir.Dispose();
        }

        [Test]
        public async Task GetAllTableComments_WhenGivenDatabaseWithTableComments_MatchesSnapshotSource()
        {
            var tableComments = await _commentsReader.GetAllTableComments().ToListAsync().ConfigureAwait(false);

            var sourceTableComments = await SampleComments.GetAllTableComments().ToListAsync().ConfigureAwait(false);

            tableComments.Should().BeEquivalentTo(sourceTableComments);
        }

        [Test]
        public async Task GetAllViewComments_WhenGivenDatabaseWithViewComments_MatchesSnapshotSource()
        {
            var viewComments = await _commentsReader.GetAllViewComments().ToListAsync().ConfigureAwait(false);

            var sourceViewComments = await SampleComments.GetAllViewComments().ToListAsync().ConfigureAwait(false);

            viewComments.Should().BeEquivalentTo(sourceViewComments);
        }

        [Test]
        public async Task GetAllSequenceComments_WhenGivenDatabaseWithSequenceComments_MatchesSnapshotSource()
        {
            var sequenceComments = await _commentsReader.GetAllSequenceComments().ToListAsync().ConfigureAwait(false);

            var sourceSequenceComments = await SampleComments.GetAllSequenceComments().ToListAsync().ConfigureAwait(false);

            sequenceComments.Should().BeEquivalentTo(sourceSequenceComments);
        }

        [Test]
        public async Task GetAllSynonymComments_WhenGivenDatabaseWithSynonymComments_MatchesSnapshotSource()
        {
            var synonymComments = await _commentsReader.GetAllSynonymComments().ToListAsync().ConfigureAwait(false);

            var sourceSynonymComments = await SampleComments.GetAllSynonymComments().ToListAsync().ConfigureAwait(false);

            synonymComments.Should().BeEquivalentTo(sourceSynonymComments);
        }

        [Test]
        public async Task GetAllRoutineComments_WhenGivenDatabaseWithRoutineComments_MatchesSnapshotSource()
        {
            var routineComments = await _commentsReader.GetAllRoutineComments().ToListAsync().ConfigureAwait(false);

            var sourceRoutineComments = await SampleComments.GetAllRoutineComments().ToListAsync().ConfigureAwait(false);

            routineComments.Should().BeEquivalentTo(sourceRoutineComments);
        }

        [Test]
        public async Task GetTableComments_WhenGivenTableNameForCommentThatExists_MatchesSnapshotSource()
        {
            var tableComments = await _commentsReader.GetTableComments("test_table_1").ToOption().ConfigureAwait(false);

            var sourceTableComments = await SampleComments.GetTableComments("test_table_1").ToOption().ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(tableComments, OptionIs.Some);

                var tableCommentsValue = tableComments.UnwrapSome();
                var sourceTableCommentsValue = sourceTableComments.UnwrapSome();

                tableCommentsValue.Should().BeEquivalentTo(sourceTableCommentsValue);
            });
        }

        [Test]
        public async Task GetViewComments_WhenGivenViewNameForCommentThatExists_MatchesSnapshotSource()
        {
            var viewComments = await _commentsReader.GetViewComments("test_view_1").ToOption().ConfigureAwait(false);

            var sourceViewComments = await SampleComments.GetViewComments("test_view_1").ToOption().ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(viewComments, OptionIs.Some);

                var viewCommentsValue = viewComments.UnwrapSome();
                var sourceViewCommentsValue = sourceViewComments.UnwrapSome();

                viewCommentsValue.Should().BeEquivalentTo(sourceViewCommentsValue);
            });
        }

        [Test]
        public async Task GetSequenceComments_WhenGivenSequenceNameForCommentThatExists_MatchesSnapshotSource()
        {
            var sequenceComments = await _commentsReader.GetSequenceComments("test_sequence_1").ToOption().ConfigureAwait(false);

            var sourceSequenceComments = await SampleComments.GetSequenceComments("test_sequence_1").ToOption().ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(sequenceComments, OptionIs.Some);

                var sequenceCommentsValue = sequenceComments.UnwrapSome();
                var sourceSequenceCommentsValue = sourceSequenceComments.UnwrapSome();

                sequenceCommentsValue.Should().BeEquivalentTo(sourceSequenceCommentsValue);
            });
        }

        [Test]
        public async Task GetSynonymComments_WhenGivenSynonymNameForCommentThatExists_MatchesSnapshotSource()
        {
            var synonymComments = await _commentsReader.GetSynonymComments("test_synonym_1").ToOption().ConfigureAwait(false);

            var sourceSynonymComments = await SampleComments.GetSynonymComments("test_synonym_1").ToOption().ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(synonymComments, OptionIs.Some);

                var synonymCommentsValue = synonymComments.UnwrapSome();
                var sourceSynonymCommentsValue = sourceSynonymComments.UnwrapSome();

                synonymCommentsValue.Should().BeEquivalentTo(sourceSynonymCommentsValue);
            });
        }

        [Test]
        public async Task GetRoutineComments_WhenGivenRoutineNameForCommentThatExists_MatchesSnapshotSource()
        {
            var routineComments = await _commentsReader.GetRoutineComments("test_routine_1").ToOption().ConfigureAwait(false);

            var sourceRoutineComments = await SampleComments.GetRoutineComments("test_routine_1").ToOption().ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(routineComments, OptionIs.Some);

                var routineCommentsValue = routineComments.UnwrapSome();
                var sourceRoutineCommentsValue = sourceRoutineComments.UnwrapSome();

                routineCommentsValue.Should().BeEquivalentTo(sourceRoutineCommentsValue);
            });
        }

        [Test]
        public async Task GetTableComments_WhenGivenTableNameForCommentThatDoesNotExist_ReturnsNone()
        {
            var tableComments = await _commentsReader.GetTableComments("not_a_table_name").ToOption().ConfigureAwait(false);

            Assert.That(tableComments, OptionIs.None);
        }

        [Test]
        public async Task GetViewComments_WhenGivenViewNameForCommentThatDoesNotExist_ReturnsNone()
        {
            var viewComments = await _commentsReader.GetViewComments("not_a_view_name").ToOption().ConfigureAwait(false);

            Assert.That(viewComments, OptionIs.None);
        }

        [Test]
        public async Task GetSequenceComments_WhenGivenSequenceNameForCommentThatDoesNotExist_ReturnsNone()
        {
            var sequenceComments = await _commentsReader.GetSequenceComments("not_a_sequence_name").ToOption().ConfigureAwait(false);

            Assert.That(sequenceComments, OptionIs.None);
        }

        [Test]
        public async Task GetSynonymComments_WhenGivenSynonymNameForCommentThatDoesNotExist_ReturnsNone()
        {
            var synonymComments = await _commentsReader.GetSynonymComments("not_a_synonym_name").ToOption().ConfigureAwait(false);

            Assert.That(synonymComments, OptionIs.None);
        }

        [Test]
        public async Task GetRoutineComments_WhenGivenRoutineNameForCommentThatDoesNotExist_ReturnsNone()
        {
            var routineComments = await _commentsReader.GetRoutineComments("not_a_routine_name").ToOption().ConfigureAwait(false);

            Assert.That(routineComments, OptionIs.None);
        }

        private static IRelationalDatabaseCommentProvider SampleComments { get; } =
            new RelationalDatabaseCommentProvider(
                new IdentifierDefaults(null, null, "main"),
                new VerbatimIdentifierResolutionStrategy(),
                new[]
                {
                    new RelationalDatabaseTableComments(
                        Identifier.CreateQualifiedIdentifier("main", "test_table_1"),
                        Option<string>.Some("a table comment"),
                        Option<string>.Some("a primary key comment"),
                        new Dictionary<Identifier, Option<string>>
                        {
                            ["table_column_1"] = Option<string>.Some("table column comment")
                        },
                        new Dictionary<Identifier, Option<string>>
                        {
                            ["table_check_1"] = Option<string>.Some("table check comment")
                        },
                        new Dictionary<Identifier, Option<string>>
                        {
                            ["table_unique_key_1"] = Option<string>.Some("table unique key comment")
                        },
                        new Dictionary<Identifier, Option<string>>
                        {
                            ["table_foreign_key_1"] = Option<string>.Some("table foreign key comment")
                        },
                        new Dictionary<Identifier, Option<string>>
                        {
                            ["table_index_1"] = Option<string>.Some("table index comment")
                        },
                        new Dictionary<Identifier, Option<string>>
                        {
                            ["table_trigger_1"] = Option<string>.Some("table trigger comment")
                        }
                    ),
                    new RelationalDatabaseTableComments(
                        Identifier.CreateQualifiedIdentifier("main", "test_table_2"),
                        Option<string>.None,
                        Option<string>.None,
                        new Dictionary<Identifier, Option<string>>(),
                        new Dictionary<Identifier, Option<string>>(),
                        new Dictionary<Identifier, Option<string>>(),
                        new Dictionary<Identifier, Option<string>>(),
                        new Dictionary<Identifier, Option<string>>(),
                        new Dictionary<Identifier, Option<string>>()
                    )
                },
                new[]
                {
                    new DatabaseViewComments(
                        Identifier.CreateQualifiedIdentifier("main", "test_view_1"),
                        Option<string>.Some("a view comment"),
                        new Dictionary<Identifier, Option<string>>
                        {
                            ["view_column_1"] = Option<string>.Some("view column comment")
                        }
                    ),
                    new DatabaseViewComments(
                        Identifier.CreateQualifiedIdentifier("main", "test_view_2"),
                        Option<string>.None,
                        new Dictionary<Identifier, Option<string>>()
                    )
                },
                new[]
                {
                    new DatabaseSequenceComments(
                        Identifier.CreateQualifiedIdentifier("main", "test_sequence_1"),
                        Option<string>.Some("a sequence comment")
                    ),
                    new DatabaseSequenceComments(
                        Identifier.CreateQualifiedIdentifier("main", "test_sequence_2"),
                        Option<string>.None
                    )
                },
                new[]
                {
                    new DatabaseSynonymComments(
                        Identifier.CreateQualifiedIdentifier("main", "test_synonym_1"),
                        Option<string>.Some("a synonym comment")
                    ),
                    new DatabaseSynonymComments(
                        Identifier.CreateQualifiedIdentifier("main", "test_synonym_2"),
                        Option<string>.None
                    )
                },
                new[]
                {
                    new DatabaseRoutineComments(
                        Identifier.CreateQualifiedIdentifier("main", "test_routine_1"),
                        Option<string>.Some("a routine comment")
                    ),
                    new DatabaseRoutineComments(
                        Identifier.CreateQualifiedIdentifier("main", "test_routine_2"),
                        Option<string>.None
                    )
                }
            );
    }
}
