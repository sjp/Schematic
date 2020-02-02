using System.Linq;
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
    internal static class ReflectionTableTests
    {
        private static IIdentifierDefaults IdentifierDefaults { get; } = new IdentifierDefaults("server", "database", "schema");

        [Test]
        public static void Ctor_GivenNullDatabase_ThrowsArgumentNullException()
        {
            Assert.That(() => new ReflectionTable(null, typeof(object)), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullTableType_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            Assert.That(() => new ReflectionTable(database, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenTableTypeWithNoDefaultCtor_ThrowsArgumentException()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            databaseMock.Setup(db => db.Dialect).Returns(new FakeDialect());
            databaseMock.Setup(db => db.IdentifierDefaults).Returns(new IdentifierDefaultsBuilder().Build());

            Assert.That(() => new ReflectionTable(databaseMock.Object, typeof(TableTypeWithBadCtor)), Throws.ArgumentException);
        }

        [Test]
        public static void Ctor_GivenTableTypeWithNotAutoProperty_ThrowsArgumentException()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            databaseMock.Setup(db => db.Dialect).Returns(new FakeDialect());
            databaseMock.Setup(db => db.IdentifierDefaults).Returns(new IdentifierDefaultsBuilder().Build());

            Assert.That(() => new ReflectionTable(databaseMock.Object, typeof(TableTypeWithBadColumns)), Throws.ArgumentException);
        }

        [Test]
        public static void Name_PropertyGetOnSimpleClass_ReturnsSameNameAsClass()
        {
            var database = new ReflectionRelationalDatabase<TestDatabase1>(new FakeDialect(), IdentifierDefaults);
            var table = new ReflectionTable(database, typeof(TestTable1));
            const string expectedName = nameof(TestTable1);

            Assert.That(table.Name.LocalName, Is.EqualTo(expectedName));
        }

        [Test]
        public static void Columns_GivenTableWithNoColumns_ReturnsEmptyResult()
        {
            var database = new ReflectionRelationalDatabase<TestDatabase2>(new FakeDialect(), IdentifierDefaults);
            var table = new ReflectionTable(database, typeof(TestTable2));
            var columns = table.Columns;

            Assert.That(columns, Is.Empty);
        }

        [Test]
        public static void Columns_GivenTableWithOneColumn_ReturnsOneResult()
        {
            var database = new ReflectionRelationalDatabase<TestDatabase1>(new FakeDialect(), IdentifierDefaults);
            var table = new ReflectionTable(database, typeof(TestTable1));
            var columns = table.Columns;

            Assert.That(columns, Has.Exactly(1).Items);
        }

        [Test]
        public static void Columns_GivenTableWithOneColumn_ReturnsColumnWithCorrectName()
        {
            var database = new ReflectionRelationalDatabase<TestDatabase1>(new FakeDialect(), IdentifierDefaults);
            var table = new ReflectionTable(database, typeof(TestTable1));
            var columns = table.Columns;
            var column = columns.Single();
            Identifier expectedName = "TEST_COLUMN_1";

            Assert.That(column.Name, Is.EqualTo(expectedName));
        }

        private sealed class TableTypeWithBadCtor
        {
            public TableTypeWithBadCtor(string tableName)
            {
                TableName = tableName;
            }

            public string TableName { get; }
        }

        private sealed class TableTypeWithBadColumns
        {
            public Column<BigInteger> TEST_COLUMN_1 => _column;

            private readonly Column<BigInteger> _column = new Column<BigInteger>();
        }

        private sealed class TestDatabase1
        {
            public Table<TestTable1> TestTable1 { get; }
        }

        private sealed class TestTable1
        {
            public Column<BigInteger> TEST_COLUMN_1 { get; }
        }

        private sealed class TestDatabase2
        {
            public Table<TestTable2> TestTable2 { get; }
        }

        private sealed class TestTable2
        {
        }
    }
}
