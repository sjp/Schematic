using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Serialization.Tests
{
    internal static class JsonRelationalDatabaseCommentSerializerTests
    {
        private static readonly Lazy<IMapper> _mapper = new(() =>
        {
            var config = new MapperConfiguration(config => config.AddMaps(typeof(Mapping.Comments.DatabaseCommentProviderProfile).Assembly));
            config.AssertConfigurationIsValid();

            return new Mapper(config);
        });

        private static IMapper Mapper { get; } = _mapper.Value;

        private static IRelationalDatabaseCommentSerializer Serializer { get; } = new JsonRelationalDatabaseCommentSerializer(Mapper);

        [Test]
        public static async Task Serialize_WhenInvoked_ExportsWithoutError()
        {
            var comments = SampleComments;
            using var jsonOutputStream = new MemoryStream();
            await Serializer.SerializeAsync(jsonOutputStream, comments).ConfigureAwait(false);
            var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

            Assert.Multiple(() =>
            {
                Assert.That(json, Is.Not.Null);
                Assert.That(json, Is.Not.Empty);
            });
        }

        [Test]
        public static async Task SerializeDeserialize_WhenEmptyDatabaseRoundTripped_ExportsAndParsesWithoutError()
        {
            var comments = new EmptyRelationalDatabaseCommentProvider(new IdentifierDefaults(null, null, "main"));

            using var jsonOutputStream = new MemoryStream();
            await Serializer.SerializeAsync(jsonOutputStream, comments).ConfigureAwait(false);

            jsonOutputStream.Seek(0, SeekOrigin.Begin);
            var importedComments = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy()).ConfigureAwait(false);

            comments.Should().BeEquivalentTo(importedComments);
        }

        [Test]
        public static async Task SerializeDeserialize_WhenEmptyDatabaseRoundTripped_PreservesJsonStructure()
        {
            var comments = new EmptyRelationalDatabaseCommentProvider(new IdentifierDefaults(null, null, "main"));

            using var jsonOutputStream = new MemoryStream();
            await Serializer.SerializeAsync(jsonOutputStream, comments).ConfigureAwait(false);
            var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

            jsonOutputStream.Seek(0, SeekOrigin.Begin);
            var importedComments = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy()).ConfigureAwait(false);

            using var jsonOutputStream2 = new MemoryStream();
            await Serializer.SerializeAsync(jsonOutputStream2, importedComments).ConfigureAwait(false);
            var reExportedJson = Encoding.UTF8.GetString(jsonOutputStream2.ToArray());

            Assert.Multiple(() =>
            {
                Assert.That(reExportedJson, Is.Not.Null);
                Assert.That(reExportedJson, Is.Not.Empty);
                Assert.That(reExportedJson, Is.EqualTo(json));
            });
        }

        [Test]
        public static async Task SerializeDeserialize_WhenRoundTripped_ExportsAndParsesWithoutError()
        {
            var comments = SampleComments;

            using var jsonOutputStream = new MemoryStream();
            await Serializer.SerializeAsync(jsonOutputStream, comments).ConfigureAwait(false);

            jsonOutputStream.Seek(0, SeekOrigin.Begin);
            var importedComments = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy()).ConfigureAwait(false);

            comments.Should().BeEquivalentTo(importedComments);
        }

        [Test]
        public static async Task SerializeDeserialize_WhenRoundTripped_PreservesStructure()
        {
            var comments = SampleComments;

            using var jsonOutputStream = new MemoryStream();
            await Serializer.SerializeAsync(jsonOutputStream, comments).ConfigureAwait(false);
            var json = Encoding.UTF8.GetString(jsonOutputStream.ToArray());

            jsonOutputStream.Seek(0, SeekOrigin.Begin);
            var importedComments = await Serializer.DeserializeAsync(jsonOutputStream, new VerbatimIdentifierResolutionStrategy()).ConfigureAwait(false);

            using var jsonOutputStream2 = new MemoryStream();
            await Serializer.SerializeAsync(jsonOutputStream2, importedComments).ConfigureAwait(false);
            var reExportedJson = Encoding.UTF8.GetString(jsonOutputStream2.ToArray());

            Assert.Multiple(() =>
            {
                Assert.That(reExportedJson, Is.Not.Null);
                Assert.That(reExportedJson, Is.Not.Empty);
                Assert.That(reExportedJson, Is.EqualTo(json));
            });
        }

        private static IRelationalDatabaseCommentProvider SampleComments { get; } =
            new RelationalDatabaseCommentProvider(
                new IdentifierDefaults(null, null, "main"),
                new VerbatimIdentifierResolutionStrategy(),
                new[]
                {
                    new RelationalDatabaseTableComments(
                        "test_table_1",
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
                        "test_table_2",
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
                        "test_view_1",
                        Option<string>.Some("a view comment"),
                        new Dictionary<Identifier, Option<string>>
                        {
                            ["view_column_1"] = Option<string>.Some("view column comment")
                        }
                    ),
                    new DatabaseViewComments(
                        "test_view_2",
                        Option<string>.None,
                        new Dictionary<Identifier, Option<string>>()
                    )
                },
                new[]
                {
                    new DatabaseSequenceComments(
                        "test_sequence_1",
                        Option<string>.Some("a sequence comment")
                    ),
                    new DatabaseSequenceComments(
                        "test_sequence_2",
                        Option<string>.None
                    )
                },
                new[]
                {
                    new DatabaseSynonymComments(
                        "test_synonym_1",
                        Option<string>.Some("a synonym comment")
                    ),
                    new DatabaseSynonymComments(
                        "test_synonym_2",
                        Option<string>.None
                    )
                },
                new[]
                {
                    new DatabaseRoutineComments(
                        "test_routine_1",
                        Option<string>.Some("a routine comment")
                    ),
                    new DatabaseRoutineComments(
                        "test_routine_2",
                        Option<string>.None
                    )
                }
            );
    }
}
