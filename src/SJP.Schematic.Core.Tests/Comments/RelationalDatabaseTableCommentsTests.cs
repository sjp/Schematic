using System.Collections.Generic;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests.Comments
{
    [TestFixture]
    internal static class RelationalDatabaseTableCommentsTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            Assert.That(() => new RelationalDatabaseTableComments(
                null,
                Option<string>.None,
                Option<string>.None,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            ), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullColumnComments_ThrowsArgumentNullException()
        {
            Assert.That(() => new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                null,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            ), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullCheckComments_ThrowsArgumentNullException()
        {
            Assert.That(() => new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                Empty.CommentLookup,
                null,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            ), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullUniqueKeyComments_ThrowsArgumentNullException()
        {
            Assert.That(() => new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                Empty.CommentLookup,
                Empty.CommentLookup,
                null,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            ), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullForeignKeyComments_ThrowsArgumentNullException()
        {
            Assert.That(() => new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                null,
                Empty.CommentLookup,
                Empty.CommentLookup
            ), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIndexComments_ThrowsArgumentNullException()
        {
            Assert.That(() => new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                null,
                Empty.CommentLookup
            ), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullTriggerComments_ThrowsArgumentNullException()
        {
            Assert.That(() => new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                null
            ), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNoNullArguments_DoesNotThrow()
        {
            Assert.That(() => new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            ), Throws.Nothing);
        }

        [Test]
        public static void TableName_PropertyGet_EqualsCtorArg()
        {
            Identifier tableName = "test_table";
            var comments = new RelationalDatabaseTableComments(
                tableName,
                Option<string>.None,
                Option<string>.None,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            );

            Assert.That(comments.TableName, Is.EqualTo(tableName));
        }

        [Test]
        public static void Comment_PropertyGetWhenCtorGivenNone_IsNone()
        {
            var comments = new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            );

            Assert.That(comments.Comment, OptionIs.None);
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
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            );

            Assert.That(comments.Comment.UnwrapSome(), Is.EqualTo(commentText));
        }

        [Test]
        public static void PrimaryKeyComment_PropertyGetWhenCtorGivenNone_IsNone()
        {
            var comments = new RelationalDatabaseTableComments(
                "test_table_valid_name",
                Option<string>.None,
                Option<string>.None,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            );

            Assert.That(comments.Comment, OptionIs.None);
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
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            );

            Assert.That(comments.PrimaryKeyComment.UnwrapSome(), Is.EqualTo(commentText));
        }

        [Test]
        public static void ColumnComments_PropertyGetWhenCtorGivenEmptyDictionary_IsEmpty()
        {
            var comments = new RelationalDatabaseTableComments(
                "test_table_for_columns",
                Option<string>.None,
                Option<string>.None,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            );

            Assert.That(comments.ColumnComments, Is.Empty);
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
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            );

            var propColumnComments = comments.ColumnComments;

            Assert.That(propColumnComments.Keys, Is.EqualTo(columnComments.Keys));
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
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            );

            var propColumnComments = comments.ColumnComments;

            Assert.Multiple(() =>
            {
                Assert.That(columnComments.Keys, Is.EqualTo(propColumnComments.Keys));

                Assert.That(columnComments["test_column_1"].IsNone, Is.EqualTo(propColumnComments["test_column_1"].IsNone));
                Assert.That(columnComments["test_column_2"].IsNone, Is.EqualTo(propColumnComments["test_column_2"].IsNone));
                Assert.That(columnComments["test_column_3"].IsNone, Is.EqualTo(propColumnComments["test_column_3"].IsNone));

                Assert.That(columnComments["test_column_2"].UnwrapSome(), Is.EqualTo(propColumnComments["test_column_2"].UnwrapSome()));
                Assert.That(columnComments["test_column_3"].UnwrapSome(), Is.EqualTo(propColumnComments["test_column_3"].UnwrapSome()));
            });
        }

        [Test]
        public static void CheckComments_PropertyGetWhenCtorGivenEmptyDictionary_IsEmpty()
        {
            var comments = new RelationalDatabaseTableComments(
                "test_table_for_checks",
                Option<string>.None,
                Option<string>.None,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            );

            Assert.That(comments.CheckComments, Is.Empty);
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
                Empty.CommentLookup,
                checkComments,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            );

            var propCheckComments = comments.CheckComments;

            Assert.That(propCheckComments.Keys, Is.EqualTo(checkComments.Keys));
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
                Empty.CommentLookup,
                checkComments,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            );

            var propCheckComments = comments.CheckComments;

            Assert.Multiple(() =>
            {
                Assert.That(checkComments.Keys, Is.EqualTo(propCheckComments.Keys));

                Assert.That(checkComments["test_check_1"].IsNone, Is.EqualTo(propCheckComments["test_check_1"].IsNone));
                Assert.That(checkComments["test_check_2"].IsNone, Is.EqualTo(propCheckComments["test_check_2"].IsNone));
                Assert.That(checkComments["test_check_3"].IsNone, Is.EqualTo(propCheckComments["test_check_3"].IsNone));

                Assert.That(checkComments["test_check_2"].UnwrapSome(), Is.EqualTo(propCheckComments["test_check_2"].UnwrapSome()));
                Assert.That(checkComments["test_check_3"].UnwrapSome(), Is.EqualTo(propCheckComments["test_check_3"].UnwrapSome()));
            });
        }

        [Test]
        public static void UniqueKeyComments_PropertyGetWhenCtorGivenEmptyDictionary_IsEmpty()
        {
            var comments = new RelationalDatabaseTableComments(
                "test_table_for_unique_keys",
                Option<string>.None,
                Option<string>.None,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            );

            Assert.That(comments.UniqueKeyComments, Is.Empty);
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
                Empty.CommentLookup,
                Empty.CommentLookup,
                uniqueKeyComments,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            );

            var propUniqueKeyComments = comments.UniqueKeyComments;

            Assert.That(uniqueKeyComments.Keys, Is.EqualTo(propUniqueKeyComments.Keys));
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
                Empty.CommentLookup,
                Empty.CommentLookup,
                uniqueKeyComments,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            );

            var propUniqueKeyComments = comments.UniqueKeyComments;

            Assert.Multiple(() =>
            {
                Assert.That(uniqueKeyComments.Keys, Is.EqualTo(propUniqueKeyComments.Keys));

                Assert.That(uniqueKeyComments["test_uk_1"].IsNone, Is.EqualTo(propUniqueKeyComments["test_uk_1"].IsNone));
                Assert.That(uniqueKeyComments["test_uk_2"].IsNone, Is.EqualTo(propUniqueKeyComments["test_uk_2"].IsNone));
                Assert.That(uniqueKeyComments["test_uk_3"].IsNone, Is.EqualTo(propUniqueKeyComments["test_uk_3"].IsNone));

                Assert.That(uniqueKeyComments["test_uk_2"].UnwrapSome(), Is.EqualTo(propUniqueKeyComments["test_uk_2"].UnwrapSome()));
                Assert.That(uniqueKeyComments["test_uk_3"].UnwrapSome(), Is.EqualTo(propUniqueKeyComments["test_uk_3"].UnwrapSome()));
            });
        }

        [Test]
        public static void ForeignKeyComments_PropertyGetWhenCtorGivenEmptyDictionary_IsEmpty()
        {
            var comments = new RelationalDatabaseTableComments(
                "test_table_for_foreign_keys",
                Option<string>.None,
                Option<string>.None,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            );

            Assert.That(comments.ForeignKeyComments, Is.Empty);
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
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                foreignKeyComments,
                Empty.CommentLookup,
                Empty.CommentLookup
            );

            var propForeignKeyComments = comments.ForeignKeyComments;

            Assert.That(foreignKeyComments.Keys, Is.EqualTo(propForeignKeyComments.Keys));
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
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                foreignKeyComments,
                Empty.CommentLookup,
                Empty.CommentLookup
            );

            var propForeignKeyComments = comments.ForeignKeyComments;

            Assert.Multiple(() =>
            {
                Assert.That(foreignKeyComments.Keys, Is.EqualTo(propForeignKeyComments.Keys));

                Assert.That(foreignKeyComments["test_fk_1"].IsNone, Is.EqualTo(propForeignKeyComments["test_fk_1"].IsNone));
                Assert.That(foreignKeyComments["test_fk_2"].IsNone, Is.EqualTo(propForeignKeyComments["test_fk_2"].IsNone));
                Assert.That(foreignKeyComments["test_fk_3"].IsNone, Is.EqualTo(propForeignKeyComments["test_fk_3"].IsNone));

                Assert.That(foreignKeyComments["test_fk_2"].UnwrapSome(), Is.EqualTo(propForeignKeyComments["test_fk_2"].UnwrapSome()));
                Assert.That(foreignKeyComments["test_fk_3"].UnwrapSome(), Is.EqualTo(propForeignKeyComments["test_fk_3"].UnwrapSome()));
            });
        }

        [Test]
        public static void IndexComments_PropertyGetWhenCtorGivenEmptyDictionary_IsEmpty()
        {
            var comments = new RelationalDatabaseTableComments(
                "test_table_for_indexes",
                Option<string>.None,
                Option<string>.None,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            );

            Assert.That(comments.IndexComments, Is.Empty);
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
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                indexComments,
                Empty.CommentLookup
            );

            var propIndexComments = comments.IndexComments;

            Assert.That(propIndexComments.Keys, Is.EqualTo(indexComments.Keys));
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
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                indexComments,
                Empty.CommentLookup
            );

            var propIndexComments = comments.IndexComments;

            Assert.Multiple(() =>
            {
                Assert.That(indexComments.Keys, Is.EqualTo(propIndexComments.Keys));

                Assert.That(indexComments["test_ix_1"].IsNone, Is.EqualTo(propIndexComments["test_ix_1"].IsNone));
                Assert.That(indexComments["test_ix_2"].IsNone, Is.EqualTo(propIndexComments["test_ix_2"].IsNone));
                Assert.That(indexComments["test_ix_3"].IsNone, Is.EqualTo(propIndexComments["test_ix_3"].IsNone));

                Assert.That(indexComments["test_ix_2"].UnwrapSome(), Is.EqualTo(propIndexComments["test_ix_2"].UnwrapSome()));
                Assert.That(indexComments["test_ix_3"].UnwrapSome(), Is.EqualTo(propIndexComments["test_ix_3"].UnwrapSome()));
            });
        }

        [Test]
        public static void TriggerComments_PropertyGetWhenCtorGivenEmptyDictionary_IsEmpty()
        {
            var comments = new RelationalDatabaseTableComments(
                "test_table_for_triggers",
                Option<string>.None,
                Option<string>.None,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup
            );

            Assert.That(comments.TriggerComments, Is.Empty);
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
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                triggerComments
            );

            var propTriggerComments = comments.TriggerComments;

            Assert.That(triggerComments.Keys, Is.EqualTo(propTriggerComments.Keys));
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
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                Empty.CommentLookup,
                triggerComments
            );

            var propTriggerComments = comments.TriggerComments;

            Assert.Multiple(() =>
            {
                Assert.That(triggerComments.Keys, Is.EqualTo(propTriggerComments.Keys));

                Assert.That(triggerComments["test_trigger_1"].IsNone, Is.EqualTo(propTriggerComments["test_trigger_1"].IsNone));
                Assert.That(triggerComments["test_trigger_2"].IsNone, Is.EqualTo(propTriggerComments["test_trigger_2"].IsNone));
                Assert.That(triggerComments["test_trigger_3"].IsNone, Is.EqualTo(propTriggerComments["test_trigger_3"].IsNone));

                Assert.That(triggerComments["test_trigger_2"].UnwrapSome(), Is.EqualTo(propTriggerComments["test_trigger_2"].UnwrapSome()));
                Assert.That(triggerComments["test_trigger_3"].UnwrapSome(), Is.EqualTo(propTriggerComments["test_trigger_3"].UnwrapSome()));
            });
        }
    }
}
