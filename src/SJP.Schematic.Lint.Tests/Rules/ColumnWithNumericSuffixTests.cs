using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Rules
{
    [TestFixture]
    internal static class ColumnWithNumericSuffixTests
    {
        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.That(() => new ColumnWithNumericSuffix(level), Throws.ArgumentException);
        }

        [Test]
        public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
        {
            var rule = new ColumnWithNumericSuffix(RuleLevel.Error);
            Assert.That(() => rule.AnalyseTables(null), Throws.ArgumentNullException);
        }

        [Test]
        public static async Task AnalyseTables_GivenTableWithoutColumnsContainingNumericSuffix_ProducesNoMessages()
        {
            var rule = new ColumnWithNullDefaultValueRule(RuleLevel.Error);

            var testColumn = new DatabaseColumn(
                "test_column",
                Mock.Of<IDbType>(),
                true,
                null,
                null
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn> { testColumn },
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

            Assert.That(hasMessages, Is.False);
        }

        [Test]
        public static async Task AnalyseTables_GivenTableWithColumnsContainingNumericSuffix_ProducesMessages()
        {
            var rule = new ColumnWithNullDefaultValueRule(RuleLevel.Error);

            var testColumn = new DatabaseColumn(
                "test_column_1",
                Mock.Of<IDbType>(),
                true,
                "null",
                null
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn> { testColumn },
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

            Assert.That(hasMessages, Is.True);
        }
    }
}
