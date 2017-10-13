using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Modelled.Reflection.Model;
using SJP.Schematic.Modelled.Reflection.Tests.Fakes;
using SJP.Schematic.Modelled.Reflection.Tests.Fakes.ColumnTypes;

namespace SJP.Schematic.Modelled.Reflection.Tests
{
    // TODO: test protected methods

    [TestFixture]
    public class ReflectionTableTests
    {
        [Test]
        public void Ctor_GivenNullDatabase_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ReflectionTable(null, typeof(object)));
        }

        [Test]
        public void Ctor_GivenNullTableType_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            Assert.Throws<ArgumentNullException>(() => new ReflectionTable(database, null));
        }

        [Test]
        public void Ctor_GivenTableTypeWithNoDefaultCtor_ThrowsArgumentException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            Assert.Throws<ArgumentException>(() => new ReflectionTable(database, typeof(TableTypeWithBadCtor)));
        }

        [Test]
        public void Ctor_GivenTableTypeWithNotAutoProperty_ThrowsArgumentException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            Assert.Throws<ArgumentException>(() => new ReflectionTable(database, typeof(TableTypeWithBadColumns)));
        }

        [Test]
        public void Database_PropertyGet_MatchesCtorArgument()
        {
            var database = new ReflectionRelationalDatabase<TestDatabase1>(new FakeDialect());
            var table = new ReflectionTable(database, typeof(TestTable1));

            Assert.AreEqual(database, table.Database);
        }

        [Test]
        public void Name_PropertyGetOnSimpleClass_ReturnsSameNameAsClass()
        {
            var database = new ReflectionRelationalDatabase<TestDatabase1>(new FakeDialect());
            var table = new ReflectionTable(database, typeof(TestTable1));
            Identifier expectedName = nameof(TestTable1);

            Assert.AreEqual(expectedName, table.Name);
        }

        [Test]
        public void Column_GivenTableWithNoColumns_ReturnsEmptyResult()
        {
            var database = new ReflectionRelationalDatabase<TestDatabase2>(new FakeDialect());
            var table = new ReflectionTable(database, typeof(TestTable2));
            var columnLookup = table.Column;

            Assert.Zero(columnLookup.Count);
        }

        [Test]
        public void Columns_GivenTableWithNoColumns_ReturnsEmptyResult()
        {
            var database = new ReflectionRelationalDatabase<TestDatabase2>(new FakeDialect());
            var table = new ReflectionTable(database, typeof(TestTable2));
            var columns = table.Columns;
            var count = columns.Count;

            Assert.Zero(count);
        }

        [Test]
        public async Task ColumnAsync_GivenTableWithNoColumns_ReturnsEmptyResult()
        {
            var database = new ReflectionRelationalDatabase<TestDatabase2>(new FakeDialect());
            var table = new ReflectionTable(database, typeof(TestTable2));
            var columnLookup = await table.ColumnAsync().ConfigureAwait(false);

            Assert.Zero(columnLookup.Count);
        }

        [Test]
        public async Task ColumnsAsync_GivenTableWithNoColumns_ReturnsEmptyResult()
        {
            var database = new ReflectionRelationalDatabase<TestDatabase2>(new FakeDialect());
            var table = new ReflectionTable(database, typeof(TestTable2));
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var count = columns.Count;

            Assert.Zero(count);
        }

        [Test]
        public void Column_GivenTableWithOneColumn_ReturnsOneResult()
        {
            var database = new ReflectionRelationalDatabase<TestDatabase1>(new FakeDialect());
            var table = new ReflectionTable(database, typeof(TestTable1));
            var columnLookup = table.Column;

            Assert.AreEqual(1, columnLookup.Count);
        }

        [Test]
        public void Columns_GivenTableWithOneColumn_ReturnsOneResult()
        {
            var database = new ReflectionRelationalDatabase<TestDatabase1>(new FakeDialect());
            var table = new ReflectionTable(database, typeof(TestTable1));
            var columns = table.Columns;
            var count = columns.Count;

            Assert.AreEqual(1, count);
        }

        [Test]
        public async Task ColumnAsync_GivenTableWithOneColumn_ReturnsOneResult()
        {
            var database = new ReflectionRelationalDatabase<TestDatabase1>(new FakeDialect());
            var table = new ReflectionTable(database, typeof(TestTable1));
            var columnLookup = await table.ColumnAsync().ConfigureAwait(false);

            Assert.AreEqual(1, columnLookup.Count);
        }

        [Test]
        public async Task ColumnsAsync_GivenTableWithOneColumn_ReturnsOneResult()
        {
            var database = new ReflectionRelationalDatabase<TestDatabase1>(new FakeDialect());
            var table = new ReflectionTable(database, typeof(TestTable1));
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var count = columns.Count;

            Assert.AreEqual(1, count);
        }

        [Test]
        public void Column_GivenTableWithOneColumn_ReturnsColumnWithCorrectName()
        {
            var database = new ReflectionRelationalDatabase<TestDatabase1>(new FakeDialect());
            var table = new ReflectionTable(database, typeof(TestTable1));
            var columnLookup = table.Column;
            var column = columnLookup.Values.Single();
            Identifier expectedName = "TEST_COLUMN_1";

            Assert.AreEqual(expectedName, column.Name);
        }

        [Test]
        public void Columns_GivenTableWithOneColumn_ReturnsColumnWithCorrectName()
        {
            var database = new ReflectionRelationalDatabase<TestDatabase1>(new FakeDialect());
            var table = new ReflectionTable(database, typeof(TestTable1));
            var columns = table.Columns;
            var column = columns.Single();
            Identifier expectedName = "TEST_COLUMN_1";

            Assert.AreEqual(expectedName, column.Name);
        }

        [Test]
        public async Task ColumnAsync_GivenTableWithOneColumn_ReturnsColumnWithCorrectName()
        {
            var database = new ReflectionRelationalDatabase<TestDatabase1>(new FakeDialect());
            var table = new ReflectionTable(database, typeof(TestTable1));
            var columnLookup = await table.ColumnAsync().ConfigureAwait(false);
            var column = columnLookup.Values.Single();
            Identifier expectedName = "TEST_COLUMN_1";

            Assert.AreEqual(expectedName, column.Name);
        }

        [Test]
        public async Task ColumnsAsync_GivenTableWithOneColumn_ReturnsColumnWithCorrectName()
        {
            var database = new ReflectionRelationalDatabase<TestDatabase1>(new FakeDialect());
            var table = new ReflectionTable(database, typeof(TestTable1));
            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var column = columns.Single();
            Identifier expectedName = "TEST_COLUMN_1";

            Assert.AreEqual(expectedName, column.Name);
        }

        private class TableTypeWithBadCtor
        {
            public TableTypeWithBadCtor(string tableName)
            {
                TableName = tableName;
            }

            public string TableName { get; }
        }

        private class TableTypeWithBadColumns
        {
            public Column<BigInteger> TEST_COLUMN_1 => _column;

            private readonly Column<BigInteger> _column = new Column<BigInteger>();
        }

        private class TestDatabase1
        {
            public Table<TestTable1> TestTable1 { get; }
        }

        private class TestTable1
        {
            public Column<BigInteger> TEST_COLUMN_1 { get; }
        }

        private class TestDatabase2
        {
            public Table<TestTable2> TestTable2 { get; }
        }

        private class TestTable2
        {
        }
    }
}
