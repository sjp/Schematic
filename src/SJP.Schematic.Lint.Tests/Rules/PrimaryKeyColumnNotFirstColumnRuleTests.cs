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
    internal static class PrimaryKeyColumnNotFirstColumnRuleTests
    {
        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new PrimaryKeyColumnNotFirstColumnRule(level));
        }

        [Test]
        public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new PrimaryKeyColumnNotFirstColumnRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseTables(null));
        }

        [Test]
        public static async Task AnalyseTables_GivenTableWithMissingPrimaryKey_ProducesNoMessages()
        {
            var rule = new PrimaryKeyColumnNotFirstColumnRule(RuleLevel.Error);

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { table };

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.IsFalse(hasMessages);
        }

        [Test]
        public static async Task AnalyseTables_GivenTableWithPrimaryKeyWithMultipleColumns_ProducesNoMessages()
        {
            var rule = new PrimaryKeyColumnNotFirstColumnRule(RuleLevel.Error);

            var testColumn1 = new DatabaseColumn(
                "test_column_1",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testColumn2 = new DatabaseColumn(
                "test_column_2",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testPrimaryKey = new DatabaseKey(
                Option<Identifier>.Some("test_primary_key"),
                DatabaseKeyType.Primary,
                new[] { testColumn1, testColumn2 },
                true
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn> { testColumn1, testColumn2 },
                testPrimaryKey,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { table };

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.IsFalse(hasMessages);
        }

        [Test]
        public static async Task AnalyseTables_GivenTableWithPrimaryKeyWithSingleColumnAsFirstColumn_ProducesNoMessages()
        {
            var rule = new PrimaryKeyColumnNotFirstColumnRule(RuleLevel.Error);

            var testColumn1 = new DatabaseColumn(
                "test_column_1",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testColumn2 = new DatabaseColumn(
                "test_column_2",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testPrimaryKey = new DatabaseKey(
                Option<Identifier>.Some("test_primary_key"),
                DatabaseKeyType.Primary,
                new[] { testColumn1 },
                true
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn> { testColumn1, testColumn2 },
                testPrimaryKey,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { table };

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.IsFalse(hasMessages);
        }

        [Test]
        public static async Task AnalyseTables_GivenTableWithPrimaryKeyWithSingleColumnNotFirstColumn_ProducesMessages()
        {
            var rule = new PrimaryKeyColumnNotFirstColumnRule(RuleLevel.Error);

            var testColumn1 = new DatabaseColumn(
                "test_column_1",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testColumn2 = new DatabaseColumn(
                "test_column_2",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testPrimaryKey = new DatabaseKey(
                Option<Identifier>.Some("test_primary_key"),
                DatabaseKeyType.Primary,
                new[] { testColumn2 },
                true
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn> { testColumn1, testColumn2 },
                testPrimaryKey,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            var tables = new[] { table };

            var hasMessages = await rule.AnalyseTables(tables).AnyAsync().ConfigureAwait(false);

            Assert.IsTrue(hasMessages);
        }
    }
}
