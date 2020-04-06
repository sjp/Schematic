using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules
{
    [TestFixture]
    internal static class ForeignKeyIndexRuleTests
    {
        private static IDatabaseColumn GetColumn(Identifier columnName)
        {
            var columnMock = new Mock<IDatabaseColumn>();
            columnMock.Setup(c => c.Name).Returns(columnName);
            return columnMock.Object;
        }

        private static IDatabaseIndexColumn GetIndexColumn(Identifier columnName)
        {
            var indexColumnMock = new Mock<IDatabaseIndexColumn>();
            indexColumnMock.Setup(c => c.DependentColumns).Returns(new[] { GetColumn(columnName) });
            return indexColumnMock.Object;
        }

        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.That(() => new ForeignKeyIndexRule(level), Throws.ArgumentException);
        }

        [Test]
        public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new ForeignKeyIndexRule(RuleLevel.Error);
            Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
        }

        [Test]
        public static async Task AnalyseTables_GivenTableWithIndexWithMoreColumnsThanKey_ProducesNoMessages()
        {
            var rule = new ForeignKeyIndexRule(RuleLevel.Error);

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>
                {
                    GetColumn("a"),
                    GetColumn("b"),
                    GetColumn("c")
                },
                null,
                Array.Empty<IDatabaseKey>(),
                new[]
                {
                    new DatabaseRelationalKey(
                        "test",
                        new DatabaseKey(Option<Identifier>.Some("test_fk_1"), DatabaseKeyType.Foreign, new[] { GetColumn("b") }, true),
                        "test_parent",
                        new DatabaseKey(Option<Identifier>.Some("test_pk_1"), DatabaseKeyType.Primary, new[] { GetColumn("b") }, true),
                        ReferentialAction.Cascade,
                        ReferentialAction.Cascade
                    )
                },
                Array.Empty<IDatabaseRelationalKey>(),
                new[]
                {
                    new DatabaseIndex(
                        "test_index_1",
                        false,
                        new[] { GetIndexColumn("b"), GetIndexColumn("c") },
                        Array.Empty<IDatabaseColumn>(),
                        true
                    )
                },
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { table };

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.That(hasMessages, Is.False);
        }

        [Test]
        public static async Task AnalyseTables_GivenTableWithIndexWithMoreColumnsThanKeyInWrongOrder_ProducesMessages()
        {
            var rule = new ForeignKeyIndexRule(RuleLevel.Error);

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>
                {
                    GetColumn("a"),
                    GetColumn("b"),
                    GetColumn("c")
                },
                null,
                Array.Empty<IDatabaseKey>(),
                new[]
                {
                    new DatabaseRelationalKey(
                        "test",
                        new DatabaseKey(Option<Identifier>.Some("test_fk_1"), DatabaseKeyType.Foreign, new[] { GetColumn("b") }, true),
                        "test_parent",
                        new DatabaseKey(Option<Identifier>.Some("test_pk_1"), DatabaseKeyType.Primary, new[] { GetColumn("b") }, true),
                        ReferentialAction.Cascade,
                        ReferentialAction.Cascade
                    )
                },
                Array.Empty<IDatabaseRelationalKey>(),
                new[]
                {
                    new DatabaseIndex(
                        "test_index_1",
                        false,
                        new[] { GetIndexColumn("c"), GetIndexColumn("b") },
                        Array.Empty<IDatabaseColumn>(),
                        true
                    )
                },
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { table };

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.That(hasMessages, Is.True);
        }

        [Test]
        public static async Task AnalyseTables_GivenTableWithIndexWithIncludedColumnThatContainKey_ProducesNoMessages()
        {
            var rule = new ForeignKeyIndexRule(RuleLevel.Error);

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>
                {
                    GetColumn("a"),
                    GetColumn("b"),
                    GetColumn("c")
                },
                null,
                Array.Empty<IDatabaseKey>(),
                new[]
                {
                    new DatabaseRelationalKey(
                        "test",
                        new DatabaseKey(Option<Identifier>.Some("test_fk_1"), DatabaseKeyType.Foreign, new[] { GetColumn("b"), GetColumn("c") }, true),
                        "test_parent",
                        new DatabaseKey(Option<Identifier>.Some("test_pk_1"), DatabaseKeyType.Primary, new[] { GetColumn("b"), GetColumn("c") }, true),
                        ReferentialAction.Cascade,
                        ReferentialAction.Cascade
                    )
                },
                Array.Empty<IDatabaseRelationalKey>(),
                new[]
                {
                    new DatabaseIndex(
                        "test_index_1",
                        false,
                        new[] { GetIndexColumn("b") },
                        new[] { GetColumn("c") },
                        true
                    )
                },
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { table };

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.That(hasMessages, Is.False);
        }

        [Test]
        public static async Task AnalyseTables_GivenTableWithIndexWithIncludedColumnThatDoesNotContainKey_ProducesNoMessages()
        {
            var rule = new ForeignKeyIndexRule(RuleLevel.Error);

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>
                {
                    GetColumn("a"),
                    GetColumn("b"),
                    GetColumn("c")
                },
                null,
                Array.Empty<IDatabaseKey>(),
                new[]
                {
                    new DatabaseRelationalKey(
                        "test",
                        new DatabaseKey(Option<Identifier>.Some("test_fk_1"), DatabaseKeyType.Foreign, new[] { GetColumn("b"), GetColumn("c") }, true),
                        "test_parent",
                        new DatabaseKey(Option<Identifier>.Some("test_pk_1"), DatabaseKeyType.Primary, new[] { GetColumn("b"), GetColumn("c") }, true),
                        ReferentialAction.Cascade,
                        ReferentialAction.Cascade
                    )
                },
                Array.Empty<IDatabaseRelationalKey>(),
                new[]
                {
                    new DatabaseIndex(
                        "test_index_1",
                        false,
                        new[] { GetIndexColumn("b") },
                        new[] { GetColumn("a") },
                        true
                    )
                },
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { table };

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.That(hasMessages, Is.True);
        }

        [Test]
        public static async Task AnalyseTables_GivenTableWithIndexWithIncludedColumnWithExtraColumnsThatContainKey_ProducesNoMessages()
        {
            var rule = new ForeignKeyIndexRule(RuleLevel.Error);

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>
                {
                    GetColumn("a"),
                    GetColumn("b"),
                    GetColumn("c")
                },
                null,
                Array.Empty<IDatabaseKey>(),
                new[]
                {
                    new DatabaseRelationalKey(
                        "test",
                        new DatabaseKey(Option<Identifier>.Some("test_fk_1"), DatabaseKeyType.Foreign, new[] { GetColumn("b"), GetColumn("c") }, true),
                        "test_parent",
                        new DatabaseKey(Option<Identifier>.Some("test_pk_1"), DatabaseKeyType.Primary, new[] { GetColumn("b"), GetColumn("c") }, true),
                        ReferentialAction.Cascade,
                        ReferentialAction.Cascade
                    )
                },
                Array.Empty<IDatabaseRelationalKey>(),
                new[]
                {
                    new DatabaseIndex(
                        "test_index_1",
                        false,
                        new[] { GetIndexColumn("b") },
                        new[] { GetColumn("a"), GetColumn("c") },
                        true
                    )
                },
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { table };

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.That(hasMessages, Is.False);
        }

        [Test]
        public static async Task AnalyseTables_GivenTableWithIndexWithExtraColumnsWithIncludedColumn_ProducesMessages()
        {
            var rule = new ForeignKeyIndexRule(RuleLevel.Error);

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>
                {
                    GetColumn("a"),
                    GetColumn("b"),
                    GetColumn("c")
                },
                null,
                Array.Empty<IDatabaseKey>(),
                new[]
                {
                    new DatabaseRelationalKey(
                        "test",
                        new DatabaseKey(Option<Identifier>.Some("test_fk_1"), DatabaseKeyType.Foreign, new[] { GetColumn("b"), GetColumn("c") }, true),
                        "test_parent",
                        new DatabaseKey(Option<Identifier>.Some("test_pk_1"), DatabaseKeyType.Primary, new[] { GetColumn("b"), GetColumn("c") }, true),
                        ReferentialAction.Cascade,
                        ReferentialAction.Cascade
                    )
                },
                Array.Empty<IDatabaseRelationalKey>(),
                new[]
                {
                    new DatabaseIndex(
                        "test_index_1",
                        false,
                        new[] { GetIndexColumn("b"), GetIndexColumn("a") },
                        new[] { GetColumn("c") },
                        true
                    )
                },
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { table };

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.That(hasMessages, Is.True);
        }
    }
}
