using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Comments
{
    [TestFixture]
    internal static class RelationalDatabaseTableCommentsTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RelationalDatabaseTableComments(
                null,
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            ));
        }

        [Test]
        public static void Ctor_GivenNullColumnComments_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                null,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            ));
        }

        [Test]
        public static void Ctor_GivenNullCheckComments_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                null,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            ));
        }

        [Test]
        public static void Ctor_GivenNullUniqueKeyComments_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                null,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            ));
        }

        [Test]
        public static void Ctor_GivenNullForeignKeyComments_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                null,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            ));
        }

        [Test]
        public static void Ctor_GivenNullIndexComments_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                null,
                new Dictionary<Identifier, Option<string>>()
            ));
        }

        [Test]
        public static void Ctor_GivenNullTriggerComments_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                null
            ));
        }

        [Test]
        public static void Ctor_GivenNoNullArguments_DoesNotThrow()
        {
            _ = new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            );
            Assert.Pass();
        }

        [Test]
        public static void TableName_PropertyGet_EqualsCtorArg()
        {
            Identifier tableName = "test_table";
            var comments = new RelationalDatabaseTableComments(
                tableName,
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            );

            Assert.AreEqual(tableName, comments.TableName);
        }

        [Test]
        public static void Comment_PropertyGetWhenCtorGivenNone_IsNone()
        {
            var comments = new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            );

            Assert.IsTrue(comments.Comment.IsNone);
        }

        [Test]
        public static void Comment_PropertyGetWhenCtorGivenValidCommentValue_MatchesCommentValue()
        {
            const string commentText = "this is a test comment";
            var commentArg = Option<string>.Some(commentText);
            var comments = new RelationalDatabaseTableComments(
                "test_table",
                commentArg,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            );

            Assert.AreEqual(commentText, comments.Comment.UnwrapSome());
        }

        [Test]
        public static void PrimaryKeyComment_PropertyGetWhenCtorGivenNone_IsNone()
        {
            var comments = new RelationalDatabaseTableComments(
                "test_table_valid_name",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            );

            Assert.IsTrue(comments.Comment.IsNone);
        }

        [Test]
        public static void PrimaryKeyComment_PropertyGetWhenCtorGivenValidCommentValue_MatchesCommentValue()
        {
            const string commentText = "this is a test comment";
            var commentArg = Option<string>.Some(commentText);
            var comments = new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                commentArg,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            );

            Assert.AreEqual(commentText, comments.PrimaryKeyComment.UnwrapSome());
        }

        [Test]
        public static void ColumnComments_PropertyGetWhenCtorGivenEmptyDictionary_IsEmpty()
        {
            var comments = new RelationalDatabaseTableComments(
                "test_table_for_columns",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            );

            var count = comments.ColumnComments.Count;

            Assert.Zero(count);
        }

        [Test]
        public static void ColumnComments_PropertyGetWhenCtorGivenDictionaryWithValues_MatchesKeys()
        {
            var columnNames = new[]
            {
                new Identifier("test_column_1"),
                new Identifier("test_column_2"),
                new Identifier("test_column_3")
            };
            var columnComments = new Dictionary<Identifier, Option<string>>
            {
                [columnNames[0]] = Option<string>.None,
                [columnNames[1]] = Option<string>.None,
                [columnNames[2]] = Option<string>.None
            };

            var comments = new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                columnComments,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            );

            var propColumnComments = comments.ColumnComments;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(columnComments.Count, propColumnComments.Count);

                var seqEqual = columnComments.Keys.SequenceEqual(propColumnComments.Keys);
                Assert.IsTrue(seqEqual);
            });
        }

        [Test]
        public static void ColumnComments_PropertyGetWhenCtorGivenDictionaryWithValues_ContainsExpectedValues()
        {
            var columnNames = new[]
            {
                new Identifier("test_column_1"),
                new Identifier("test_column_2"),
                new Identifier("test_column_3")
            };
            var columnComments = new Dictionary<Identifier, Option<string>>
            {
                [columnNames[0]] = Option<string>.None,
                [columnNames[1]] = Option<string>.Some("test comment for second column"),
                [columnNames[2]] = Option<string>.Some("test comment for third column")
            };

            var comments = new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                columnComments,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            );

            var propColumnComments = comments.ColumnComments;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(columnComments.Count, propColumnComments.Count);

                Assert.AreEqual(columnComments["test_column_1"].IsNone, propColumnComments["test_column_1"].IsNone);
                Assert.AreEqual(columnComments["test_column_2"].IsNone, propColumnComments["test_column_2"].IsNone);
                Assert.AreEqual(columnComments["test_column_3"].IsNone, propColumnComments["test_column_3"].IsNone);

                Assert.AreEqual(columnComments["test_column_2"].UnwrapSome(), propColumnComments["test_column_2"].UnwrapSome());
                Assert.AreEqual(columnComments["test_column_3"].UnwrapSome(), propColumnComments["test_column_3"].UnwrapSome());
            });
        }

        [Test]
        public static void CheckComments_PropertyGetWhenCtorGivenEmptyDictionary_IsEmpty()
        {
            var comments = new RelationalDatabaseTableComments(
                "test_table_for_checks",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            );

            var count = comments.CheckComments.Count;

            Assert.Zero(count);
        }

        [Test]
        public static void CheckComments_PropertyGetWhenCtorGivenDictionaryWithValues_MatchesKeys()
        {
            var checkNames = new[]
            {
                new Identifier("test_check_1"),
                new Identifier("test_check_2"),
                new Identifier("test_check_3")
            };
            var checkComments = new Dictionary<Identifier, Option<string>>
            {
                [checkNames[0]] = Option<string>.None,
                [checkNames[1]] = Option<string>.None,
                [checkNames[2]] = Option<string>.None
            };

            var comments = new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                checkComments,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            );

            var propCheckComments = comments.CheckComments;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(checkComments.Count, propCheckComments.Count);

                var seqEqual = checkComments.Keys.SequenceEqual(propCheckComments.Keys);
                Assert.IsTrue(seqEqual);
            });
        }

        [Test]
        public static void CheckComments_PropertyGetWhenCtorGivenDictionaryWithValues_ContainsExpectedValues()
        {
            var checkNames = new[]
            {
                new Identifier("test_check_1"),
                new Identifier("test_check_2"),
                new Identifier("test_check_3")
            };
            var checkComments = new Dictionary<Identifier, Option<string>>
            {
                [checkNames[0]] = Option<string>.None,
                [checkNames[1]] = Option<string>.Some("test comment for second check"),
                [checkNames[2]] = Option<string>.Some("test comment for third check")
            };

            var comments = new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                checkComments,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            );

            var propCheckComments = comments.CheckComments;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(checkComments.Count, propCheckComments.Count);

                Assert.AreEqual(checkComments["test_check_1"].IsNone, propCheckComments["test_check_1"].IsNone);
                Assert.AreEqual(checkComments["test_check_2"].IsNone, propCheckComments["test_check_2"].IsNone);
                Assert.AreEqual(checkComments["test_check_3"].IsNone, propCheckComments["test_check_3"].IsNone);

                Assert.AreEqual(checkComments["test_check_2"].UnwrapSome(), propCheckComments["test_check_2"].UnwrapSome());
                Assert.AreEqual(checkComments["test_check_3"].UnwrapSome(), propCheckComments["test_check_3"].UnwrapSome());
            });
        }

        [Test]
        public static void UniqueKeyComments_PropertyGetWhenCtorGivenEmptyDictionary_IsEmpty()
        {
            var comments = new RelationalDatabaseTableComments(
                "test_table_for_unique_keys",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            );

            var count = comments.UniqueKeyComments.Count;

            Assert.Zero(count);
        }

        [Test]
        public static void UniqueKeyComments_PropertyGetWhenCtorGivenDictionaryWithValues_MatchesKeys()
        {
            var uniqueKeyNames = new[]
            {
                new Identifier("test_uk_1"),
                new Identifier("test_uk_2"),
                new Identifier("test_uk_3")
            };
            var uniqueKeyComments = new Dictionary<Identifier, Option<string>>
            {
                [uniqueKeyNames[0]] = Option<string>.None,
                [uniqueKeyNames[1]] = Option<string>.None,
                [uniqueKeyNames[2]] = Option<string>.None
            };

            var comments = new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                uniqueKeyComments,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            );

            var propUniqueKeyComments = comments.UniqueKeyComments;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(uniqueKeyComments.Count, propUniqueKeyComments.Count);

                var seqEqual = uniqueKeyComments.Keys.SequenceEqual(propUniqueKeyComments.Keys);
                Assert.IsTrue(seqEqual);
            });
        }

        [Test]
        public static void UniqueKeyComments_PropertyGetWhenCtorGivenDictionaryWithValues_ContainsExpectedValues()
        {
            var uniqueKeyNames = new[]
            {
                new Identifier("test_uk_1"),
                new Identifier("test_uk_2"),
                new Identifier("test_uk_3")
            };
            var uniqueKeyComments = new Dictionary<Identifier, Option<string>>
            {
                [uniqueKeyNames[0]] = Option<string>.None,
                [uniqueKeyNames[1]] = Option<string>.Some("test comment for second uk"),
                [uniqueKeyNames[2]] = Option<string>.Some("test comment for third uk")
            };

            var comments = new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                uniqueKeyComments,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            );

            var propUniqueKeyComments = comments.UniqueKeyComments;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(uniqueKeyComments.Count, propUniqueKeyComments.Count);

                Assert.AreEqual(uniqueKeyComments["test_uk_1"].IsNone, propUniqueKeyComments["test_uk_1"].IsNone);
                Assert.AreEqual(uniqueKeyComments["test_uk_2"].IsNone, propUniqueKeyComments["test_uk_2"].IsNone);
                Assert.AreEqual(uniqueKeyComments["test_uk_3"].IsNone, propUniqueKeyComments["test_uk_3"].IsNone);

                Assert.AreEqual(uniqueKeyComments["test_uk_2"].UnwrapSome(), propUniqueKeyComments["test_uk_2"].UnwrapSome());
                Assert.AreEqual(uniqueKeyComments["test_uk_3"].UnwrapSome(), propUniqueKeyComments["test_uk_3"].UnwrapSome());
            });
        }

        [Test]
        public static void ForeignKeyComments_PropertyGetWhenCtorGivenEmptyDictionary_IsEmpty()
        {
            var comments = new RelationalDatabaseTableComments(
                "test_table_for_foreign_keys",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            );

            var count = comments.ForeignKeyComments.Count;

            Assert.Zero(count);
        }

        [Test]
        public static void ForeignKeyComments_PropertyGetWhenCtorGivenDictionaryWithValues_MatchesKeys()
        {
            var foreignKeyNames = new[]
            {
                new Identifier("test_fk_1"),
                new Identifier("test_fk_2"),
                new Identifier("test_fk_3")
            };
            var foreignKeyComments = new Dictionary<Identifier, Option<string>>
            {
                [foreignKeyNames[0]] = Option<string>.None,
                [foreignKeyNames[1]] = Option<string>.None,
                [foreignKeyNames[2]] = Option<string>.None
            };

            var comments = new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                foreignKeyComments,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            );

            var propForeignKeyComments = comments.ForeignKeyComments;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(foreignKeyComments.Count, propForeignKeyComments.Count);

                var seqEqual = foreignKeyComments.Keys.SequenceEqual(propForeignKeyComments.Keys);
                Assert.IsTrue(seqEqual);
            });
        }

        [Test]
        public static void ForeignKeyComments_PropertyGetWhenCtorGivenDictionaryWithValues_ContainsExpectedValues()
        {
            var foreignKeyNames = new[]
            {
                new Identifier("test_fk_1"),
                new Identifier("test_fk_2"),
                new Identifier("test_fk_3")
            };
            var foreignKeyComments = new Dictionary<Identifier, Option<string>>
            {
                [foreignKeyNames[0]] = Option<string>.None,
                [foreignKeyNames[1]] = Option<string>.Some("test comment for second fk"),
                [foreignKeyNames[2]] = Option<string>.Some("test comment for third fk")
            };

            var comments = new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                foreignKeyComments,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            );

            var propForeignKeyComments = comments.ForeignKeyComments;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(foreignKeyComments.Count, propForeignKeyComments.Count);

                Assert.AreEqual(foreignKeyComments["test_fk_1"].IsNone, propForeignKeyComments["test_fk_1"].IsNone);
                Assert.AreEqual(foreignKeyComments["test_fk_2"].IsNone, propForeignKeyComments["test_fk_2"].IsNone);
                Assert.AreEqual(foreignKeyComments["test_fk_3"].IsNone, propForeignKeyComments["test_fk_3"].IsNone);

                Assert.AreEqual(foreignKeyComments["test_fk_2"].UnwrapSome(), propForeignKeyComments["test_fk_2"].UnwrapSome());
                Assert.AreEqual(foreignKeyComments["test_fk_3"].UnwrapSome(), propForeignKeyComments["test_fk_3"].UnwrapSome());
            });
        }

        [Test]
        public static void IndexComments_PropertyGetWhenCtorGivenEmptyDictionary_IsEmpty()
        {
            var comments = new RelationalDatabaseTableComments(
                "test_table_for_indexes",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            );

            var count = comments.IndexComments.Count;

            Assert.Zero(count);
        }

        [Test]
        public static void IndexComments_PropertyGetWhenCtorGivenDictionaryWithValues_MatchesKeys()
        {
            var indexNames = new[]
            {
                new Identifier("test_ix_1"),
                new Identifier("test_ix_2"),
                new Identifier("test_ix_3")
            };
            var indexComments = new Dictionary<Identifier, Option<string>>
            {
                [indexNames[0]] = Option<string>.None,
                [indexNames[1]] = Option<string>.None,
                [indexNames[2]] = Option<string>.None
            };

            var comments = new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                indexComments,
                new Dictionary<Identifier, Option<string>>()
            );

            var propIndexComments = comments.IndexComments;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(indexComments.Count, propIndexComments.Count);

                var seqEqual = indexComments.Keys.SequenceEqual(propIndexComments.Keys);
                Assert.IsTrue(seqEqual);
            });
        }

        [Test]
        public static void IndexComments_PropertyGetWhenCtorGivenDictionaryWithValues_ContainsExpectedValues()
        {
            var indexNames = new[]
            {
                new Identifier("test_ix_1"),
                new Identifier("test_ix_2"),
                new Identifier("test_ix_3")
            };
            var indexComments = new Dictionary<Identifier, Option<string>>
            {
                [indexNames[0]] = Option<string>.None,
                [indexNames[1]] = Option<string>.Some("test comment for second index"),
                [indexNames[2]] = Option<string>.Some("test comment for third index")
            };

            var comments = new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                indexComments,
                new Dictionary<Identifier, Option<string>>()
            );

            var propIndexComments = comments.IndexComments;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(indexComments.Count, propIndexComments.Count);

                Assert.AreEqual(indexComments["test_ix_1"].IsNone, propIndexComments["test_ix_1"].IsNone);
                Assert.AreEqual(indexComments["test_ix_2"].IsNone, propIndexComments["test_ix_2"].IsNone);
                Assert.AreEqual(indexComments["test_ix_3"].IsNone, propIndexComments["test_ix_3"].IsNone);

                Assert.AreEqual(indexComments["test_ix_2"].UnwrapSome(), propIndexComments["test_ix_2"].UnwrapSome());
                Assert.AreEqual(indexComments["test_ix_3"].UnwrapSome(), propIndexComments["test_ix_3"].UnwrapSome());
            });
        }

        [Test]
        public static void TriggerComments_PropertyGetWhenCtorGivenEmptyDictionary_IsEmpty()
        {
            var comments = new RelationalDatabaseTableComments(
                "test_table_for_triggers",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            );

            var count = comments.TriggerComments.Count;

            Assert.Zero(count);
        }

        [Test]
        public static void TriggerComments_PropertyGetWhenCtorGivenDictionaryWithValues_MatchesKeys()
        {
            var triggerNames = new[]
            {
                new Identifier("test_trigger_1"),
                new Identifier("test_trigger_2"),
                new Identifier("test_trigger_3")
            };
            var triggerComments = new Dictionary<Identifier, Option<string>>
            {
                [triggerNames[0]] = Option<string>.None,
                [triggerNames[1]] = Option<string>.None,
                [triggerNames[2]] = Option<string>.None
            };

            var comments = new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                triggerComments
            );

            var propTriggerComments = comments.TriggerComments;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(triggerComments.Count, propTriggerComments.Count);

                var seqEqual = triggerComments.Keys.SequenceEqual(propTriggerComments.Keys);
                Assert.IsTrue(seqEqual);
            });
        }

        [Test]
        public static void TriggerComments_PropertyGetWhenCtorGivenDictionaryWithValues_ContainsExpectedValues()
        {
            var triggerNames = new[]
            {
                new Identifier("test_trigger_1"),
                new Identifier("test_trigger_2"),
                new Identifier("test_trigger_3")
            };
            var triggerComments = new Dictionary<Identifier, Option<string>>
            {
                [triggerNames[0]] = Option<string>.None,
                [triggerNames[1]] = Option<string>.Some("test comment for second trigger"),
                [triggerNames[2]] = Option<string>.Some("test comment for third trigger")
            };

            var comments = new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                triggerComments
            );

            var propTriggerComments = comments.TriggerComments;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(triggerComments.Count, propTriggerComments.Count);

                Assert.AreEqual(triggerComments["test_trigger_1"].IsNone, propTriggerComments["test_trigger_1"].IsNone);
                Assert.AreEqual(triggerComments["test_trigger_2"].IsNone, propTriggerComments["test_trigger_2"].IsNone);
                Assert.AreEqual(triggerComments["test_trigger_3"].IsNone, propTriggerComments["test_trigger_3"].IsNone);

                Assert.AreEqual(triggerComments["test_trigger_2"].UnwrapSome(), propTriggerComments["test_trigger_2"].UnwrapSome());
                Assert.AreEqual(triggerComments["test_trigger_3"].UnwrapSome(), propTriggerComments["test_trigger_3"].UnwrapSome());
            });
        }
    }
}
