using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Lint.Tests.Fakes;

namespace SJP.Schematic.Lint.Tests.Rules
{
    [TestFixture]
    internal class PrimaryKeyNotIntegerRuleTests
    {
        [Test]
        public void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new PrimaryKeyNotIntegerRule(level));
        }

        [Test]
        public void AnalyseDatabase_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new PrimaryKeyNotIntegerRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabase(null));
        }

        [Test]
        public void AnalyseDatabase_GivenTableWithMissingPrimaryKey_ProducesNoMessages()
        {
            var rule = new PrimaryKeyNotIntegerRule(RuleLevel.Error);

            var database = CreateFakeDatabase();
            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn>(),
                null,
                Enumerable.Empty<IDatabaseKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                Enumerable.Empty<IDatabaseTableIndex>(),
                Enumerable.Empty<IDatabaseCheckConstraint>(),
                Enumerable.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseDatabase_GivenTableWithPrimaryKeyWithSingleIntegerColumn_ProducesNoMessages()
        {
            var rule = new PrimaryKeyNotIntegerRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var dataTypeMock = new Mock<IDbType>();
            dataTypeMock.Setup(t => t.DataType).Returns(DataType.Integer);

            var testColumn = new DatabaseTableColumn(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_column_1",
                dataTypeMock.Object,
                false,
                null,
                null
            );
            var testPrimaryKey = new DatabaseKey(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_primary_key",
                DatabaseKeyType.Primary,
                new[] { testColumn },
                true
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn> { testColumn },
                testPrimaryKey,
                Enumerable.Empty<IDatabaseKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                Enumerable.Empty<IDatabaseTableIndex>(),
                Enumerable.Empty<IDatabaseCheckConstraint>(),
                Enumerable.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseDatabase_GivenTableWithPrimaryKeyWithSingleNonIntegerColumn_ProducesMessages()
        {
            var rule = new PrimaryKeyNotIntegerRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var dataTypeMock = new Mock<IDbType>();
            dataTypeMock.Setup(t => t.DataType).Returns(DataType.Binary);

            var testColumn = new DatabaseTableColumn(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_column_1",
                dataTypeMock.Object,
                false,
                null,
                null
            );
            var testPrimaryKey = new DatabaseKey(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_primary_key",
                DatabaseKeyType.Primary,
                new[] { testColumn },
                true
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn> { testColumn },
                testPrimaryKey,
                Enumerable.Empty<IDatabaseKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                Enumerable.Empty<IDatabaseTableIndex>(),
                Enumerable.Empty<IDatabaseCheckConstraint>(),
                Enumerable.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public void AnalyseDatabase_GivenTableWithPrimaryKeyWithMultipleColumns_ProducesMessages()
        {
            var rule = new PrimaryKeyNotIntegerRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn1 = new DatabaseTableColumn(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_column_1",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testColumn2 = new DatabaseTableColumn(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_column_2",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testPrimaryKey = new DatabaseKey(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_primary_key",
                DatabaseKeyType.Primary,
                new[] { testColumn1, testColumn2 },
                true
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn> { testColumn1, testColumn2 },
                testPrimaryKey,
                Enumerable.Empty<IDatabaseKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                Enumerable.Empty<IDatabaseTableIndex>(),
                Enumerable.Empty<IDatabaseCheckConstraint>(),
                Enumerable.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.NotZero(messages.Count());
        }

        private static FakeRelationalDatabase CreateFakeDatabase()
        {
            var dialect = new FakeDatabaseDialect();
            var connection = Mock.Of<IDbConnection>();

            return new FakeRelationalDatabase(dialect, connection);
        }
    }
}
