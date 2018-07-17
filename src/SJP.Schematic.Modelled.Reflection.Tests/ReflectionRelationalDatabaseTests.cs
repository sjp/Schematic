using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Modelled.Reflection.Model;
using SJP.Schematic.Modelled.Reflection.Tests.Fakes;
using SJP.Schematic.Modelled.Reflection.Tests.Fakes.ColumnTypes;

namespace SJP.Schematic.Modelled.Reflection.Tests
{
    [TestFixture]
    internal static class ReflectionRelationalDatabaseTests
    {
        [Test]
        public static void CtorT_GivenNullDialect_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ReflectionRelationalDatabase<SampleDatabase>(null));
        }

        [Test]
        public static void Ctor_GivenNullArguments_ThrowsArgumentNullException()
        {
            var dialect = new FakeDialect();
            var dbType = typeof(SampleDatabase);

            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => new ReflectionRelationalDatabase(null, dbType));
                Assert.Throws<ArgumentNullException>(() => new ReflectionRelationalDatabase(dialect, null));
            });
        }

        [Test]
        public static void TableExists_GivenNullName_ThrowsArgumentNullException()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            Assert.Throws<ArgumentNullException>(() => db.TableExists(null));
        }

        [Test]
        public static void TableExists_WhenTablePresent_ReturnsTrue()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var tableExists = db.TableExists("TestTable1");
            Assert.IsTrue(tableExists);
        }

        [Test]
        public static void TableExists_WhenTableMissing_ReturnsFalse()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var tableExists = db.TableExists("table_that_doesnt_exist");
            Assert.IsFalse(tableExists);
        }

        [Test]
        public static void TableExists_WhenTablePresentWithDifferentCase_ReturnsTable()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var tableExists = db.TableExists("testtable1");
            Assert.IsTrue(tableExists);
        }

        [Test]
        public static void GetTable_GivenNullName_ThrowsArgumentNullException()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            Assert.Throws<ArgumentNullException>(() => db.GetTable(null));
        }

        [Test]
        public static void GetTable_WhenTablePresent_ReturnsTable()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var table = db.GetTable("TestTable1");
            Assert.NotNull(table);
        }

        [Test]
        public static void GetTable_WhenTableMissing_ReturnsNull()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var table = db.GetTable("table_that_doesnt_exist");
            Assert.IsNull(table);
        }

        [Test]
        public static void TableExistsAsync_GivenNullName_ThrowsArgumentNullException()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            Assert.Throws<ArgumentNullException>(() => db.TableExistsAsync(null));
        }

        [Test]
        public static async Task TableExistsAsync_WhenTablePresent_ReturnsTrue()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var tableExists = await db.TableExistsAsync("TestTable1").ConfigureAwait(false);
            Assert.IsTrue(tableExists);
        }

        [Test]
        public static async Task TableExistsAsync_WhenTableMissing_ReturnsFalse()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var tableExists = await db.TableExistsAsync("table_that_doesnt_exist").ConfigureAwait(false);
            Assert.IsFalse(tableExists);
        }

        [Test]
        public static async Task TableExistsAsync_WhenTablePresentWithDifferentCase_ReturnsTable()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var tableExists = await db.TableExistsAsync("testtable1").ConfigureAwait(false);
            Assert.IsTrue(tableExists);
        }

        [Test]
        public static void GetTableAsync_GivenNullName_ThrowsArgumentNullException()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            Assert.Throws<ArgumentNullException>(() => db.GetTableAsync(null));
        }

        [Test]
        public static async Task GetTableAsync_WhenTablePresent_ReturnsTable()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var table = await db.GetTableAsync("TestTable1").ConfigureAwait(false);
            Assert.NotNull(table);
        }

        [Test]
        public static async Task GetTableAsync_WhenTableMissing_ReturnsNull()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var table =  await db.GetTableAsync("table_that_doesnt_exist").ConfigureAwait(false);
            Assert.IsNull(table);
        }

        private class SampleDatabase
        {
            public Table<TestTable1> FirstTestTable { get; }

            public class TestTable1
            {
                public Column<BigInteger> TEST_TABLE_ID { get; }

                public Column<Varchar200> TEST_STRING { get; }

                public Key PK_TEST_TABLE => new Key.Primary(TEST_TABLE_ID);

                public ComputedColumn TEST_COMPUTED => new ComputedColumn("@TestString + @TestString", new { TestString = TEST_STRING });
            }
        }
    }
}
