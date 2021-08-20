using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using LanguageExt;
using Microsoft.Data.Sqlite;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Reporting.Snapshot;
using SJP.Schematic.Serialization.Mapping;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Integration.Snapshot
{
    internal sealed class SnapshotRelationalDatabaseCommentsWriterTests
    {
        private TemporaryDirectory _tempDir;
        private IDbConnectionFactory _connectionFactory;
        private IMapper _mapper;

        private SnapshotRelationalDatabaseCommentsWriter _commentsWriter;

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

            var dbPath = Path.Combine(_tempDir.DirectoryPath, "snapshot-relational-database-comments-writer-test.db");

            var builder = new SqliteConnectionStringBuilder { DataSource = dbPath };
            _connectionFactory = new SqliteConnectionFactory(builder.ToString());

            var mapperConfig = new MapperConfiguration(config => config.AddMaps(typeof(RelationalDatabaseProfile).Assembly));
            _mapper = mapperConfig.CreateMapper();

            _commentsWriter = new SnapshotRelationalDatabaseCommentsWriter(_connectionFactory, _mapper);

            var snapshotSchema = new SnapshotSchema(_connectionFactory);
            await snapshotSchema.EnsureSchemaExistsAsync().ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            _tempDir.Dispose();
        }

        [Test]
        public async Task SnapshotDatabaseCommentsAsync_WhenGivenDatabaseWithTables_SnapshotsExpectedData()
        {
            var dbComments = SampleComments;

            await _commentsWriter.SnapshotDatabaseCommentsAsync(dbComments).ConfigureAwait(false);

            var dbTableComments = await dbComments.GetAllTableComments().ToListAsync().ConfigureAwait(false);
            foreach (var dbTableComment in dbTableComments)
            {
                await AssertTableCommentsMatchAsync(dbTableComment).ConfigureAwait(false);
            }
        }

        [Test]
        public async Task SnapshotDatabaseCommentsAsync_WhenGivenDatabaseWithViews_SnapshotsExpectedData()
        {
            var dbComments = SampleComments;

            await _commentsWriter.SnapshotDatabaseCommentsAsync(dbComments).ConfigureAwait(false);

            var dbViewComments = await dbComments.GetAllViewComments().ToListAsync().ConfigureAwait(false);
            foreach (var dbViewComment in dbViewComments)
            {
                await AssertViewCommentsMatchAsync(dbViewComment).ConfigureAwait(false);
            }
        }

        [Test]
        public async Task SnapshotDatabaseCommentsAsync_WhenGivenDatabaseWithSequences_SnapshotsExpectedData()
        {
            var dbComments = SampleComments;

            await _commentsWriter.SnapshotDatabaseCommentsAsync(dbComments).ConfigureAwait(false);

            var dbSequenceComments = await dbComments.GetAllSequenceComments().ToListAsync().ConfigureAwait(false);
            foreach (var dbSequenceComment in dbSequenceComments)
            {
                await AssertSequenceCommentsMatchAsync(dbSequenceComment).ConfigureAwait(false);
            }
        }

        [Test]
        public async Task SnapshotDatabaseCommentsAsync_WhenGivenDatabaseWithSynonyms_SnapshotsExpectedData()
        {
            var dbComments = SampleComments;

            await _commentsWriter.SnapshotDatabaseCommentsAsync(dbComments).ConfigureAwait(false);

            var dbSynonymComments = await dbComments.GetAllSynonymComments().ToListAsync().ConfigureAwait(false);
            foreach (var dbSynonymComment in dbSynonymComments)
            {
                await AssertSynonymCommentsMatchAsync(dbSynonymComment).ConfigureAwait(false);
            }
        }

        [Test]
        public async Task SnapshotDatabaseCommentsAsync_WhenGivenDatabaseWithRoutines_SnapshotsExpectedData()
        {
            var dbComments = SampleComments;

            await _commentsWriter.SnapshotDatabaseCommentsAsync(dbComments).ConfigureAwait(false);

            var dbRoutineComments = await dbComments.GetAllRoutineComments().ToListAsync().ConfigureAwait(false);
            foreach (var dbRoutineComment in dbRoutineComments)
            {
                await AssertRoutineCommentsMatchAsync(dbRoutineComment).ConfigureAwait(false);
            }
        }

        private async Task AssertTableCommentsMatchAsync(IRelationalDatabaseTableComments tableComments)
        {
            var snapshotCommentsJson = await _connectionFactory.ExecuteScalarAsync<string>(
                "SELECT comment_json FROM database_comment WHERE local_name = @TableName AND object_type = 'TABLE'",
                new { TableName = tableComments.TableName.LocalName },
                CancellationToken.None
            ).ConfigureAwait(false);

            var tableCommentsDto = _mapper.Map<Serialization.Dto.Comments.DatabaseTableComments>(tableComments);
            var tableCommentsJson = JsonSerializer.Serialize(tableCommentsDto, _settings.Value);

            Assert.That(tableCommentsJson, Is.EqualTo(snapshotCommentsJson));
        }

        private async Task AssertViewCommentsMatchAsync(IDatabaseViewComments viewComments)
        {
            var snapshotCommentsJson = await _connectionFactory.ExecuteScalarAsync<string>(
                "SELECT comment_json FROM database_comment WHERE local_name = @ViewName AND object_type = 'VIEW'",
                new { ViewName = viewComments.ViewName.LocalName },
                CancellationToken.None
            ).ConfigureAwait(false);

            var viewCommentsDto = _mapper.Map<Serialization.Dto.Comments.DatabaseViewComments>(viewComments);
            var viewCommentsJson = JsonSerializer.Serialize(viewCommentsDto, _settings.Value);

            Assert.That(viewCommentsJson, Is.EqualTo(snapshotCommentsJson));
        }

        private async Task AssertSequenceCommentsMatchAsync(IDatabaseSequenceComments sequenceComments)
        {
            var snapshotCommentsJson = await _connectionFactory.ExecuteScalarAsync<string>(
                "SELECT comment_json FROM database_comment WHERE local_name = @SequenceName AND object_type = 'SEQUENCE'",
                new { SequenceName = sequenceComments.SequenceName.LocalName },
                CancellationToken.None
            ).ConfigureAwait(false);

            var sequenceCommentsDto = _mapper.Map<Serialization.Dto.Comments.DatabaseSequenceComments>(sequenceComments);
            var sequenceCommentsJson = JsonSerializer.Serialize(sequenceCommentsDto, _settings.Value);

            Assert.That(sequenceCommentsJson, Is.EqualTo(snapshotCommentsJson));
        }

        private async Task AssertSynonymCommentsMatchAsync(IDatabaseSynonymComments synonymComments)
        {
            var snapshotCommentsJson = await _connectionFactory.ExecuteScalarAsync<string>(
                "SELECT comment_json FROM database_comment WHERE local_name = @SynonymName AND object_type = 'SYNONYM'",
                new { SynonymName = synonymComments.SynonymName.LocalName },
                CancellationToken.None
            ).ConfigureAwait(false);

            var synonymCommentsDto = _mapper.Map<Serialization.Dto.Comments.DatabaseSynonymComments>(synonymComments);
            var synonymCommentsJson = JsonSerializer.Serialize(synonymCommentsDto, _settings.Value);

            Assert.That(synonymCommentsJson, Is.EqualTo(snapshotCommentsJson));
        }

        private async Task AssertRoutineCommentsMatchAsync(IDatabaseRoutineComments routineComments)
        {
            var snapshotCommentsJson = await _connectionFactory.ExecuteScalarAsync<string>(
                "SELECT comment_json FROM database_comment WHERE local_name = @RoutineName AND object_type = 'ROUTINE'",
                new { RoutineName = routineComments.RoutineName.LocalName },
                CancellationToken.None
            ).ConfigureAwait(false);

            var routineCommentsDto = _mapper.Map<Serialization.Dto.Comments.DatabaseRoutineComments>(routineComments);
            var routineCommentsJson = JsonSerializer.Serialize(routineCommentsDto, _settings.Value);

            Assert.That(routineCommentsJson, Is.EqualTo(snapshotCommentsJson));
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
