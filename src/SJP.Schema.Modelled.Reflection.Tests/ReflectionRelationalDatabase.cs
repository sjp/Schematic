using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schema.Modelled.Reflection.Model;
using SJP.Schema.Modelled.Reflection.Tests.Fakes;
using SJP.Schema.Modelled.Reflection.Tests.Fakes.ColumnTypes;

namespace SJP.Schema.Modelled.Reflection.Tests
{
    [TestFixture]
    public class ReflectionRelationalDatabaseTests
    {
        [Test]
        public void CtorT_GivenNullDialect_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ReflectionRelationalDatabase<SampleDatabase>(null));
        }

        [Test]
        public void Ctor_GivenNullArguments_ThrowsArgumentNullException()
        {
            var dialect = new FakeDialect();
            var dbType = typeof(SampleDatabase);
            Assert.Throws<ArgumentNullException>(() => new ReflectionRelationalDatabase(null, dbType));
            Assert.Throws<ArgumentNullException>(() => new ReflectionRelationalDatabase(dialect, null));
        }

        [Test]
        public void TableExists_GivenNullName_ThrowsArgumentNullException()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            Assert.Throws<ArgumentNullException>(() => db.TableExists(null));
        }

        [Test]
        public void TableExists_WhenTablePresent_ReturnsTrue()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var tableExists = db.TableExists("TestTable1");
            Assert.IsTrue(tableExists);
        }

        [Test]
        public void TableExists_WhenTableMissing_ReturnsFalse()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var tableExists = db.TableExists("table_that_doesnt_exist");
            Assert.IsFalse(tableExists);
        }

        [Test]
        public void TableExists_WhenTablePresentWithDifferentCase_ReturnsTable()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var tableExists = db.TableExists("testtable1");
            Assert.IsTrue(tableExists);
        }

        [Test]
        public void GetTable_GivenNullName_ThrowsArgumentNullException()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            Assert.Throws<ArgumentNullException>(() => db.GetTable(null));
        }

        [Test]
        public void GetTable_WhenTablePresent_ReturnsTable()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var table = db.GetTable("TestTable1");
            Assert.NotNull(table);
        }

        [Test]
        public void GetTable_WhenTableMissing_ReturnsNull()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var table = db.GetTable("table_that_doesnt_exist");
            Assert.IsNull(table);
        }

        [Test]
        public void TableExistsAsync_GivenNullName_ThrowsArgumentNullException()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            Assert.ThrowsAsync<ArgumentNullException>(async () => await db.TableExistsAsync(null));
        }

        [Test]
        public async Task TableExistsAsync_WhenTablePresent_ReturnsTrue()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var tableExists = await db.TableExistsAsync("TestTable1");
            Assert.IsTrue(tableExists);
        }

        [Test]
        public async Task TableExistsAsync_WhenTableMissing_ReturnsFalse()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var tableExists = await db.TableExistsAsync("table_that_doesnt_exist");
            Assert.IsFalse(tableExists);
        }

        [Test]
        public async Task TableExistsAsync_WhenTablePresentWithDifferentCase_ReturnsTable()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var tableExists = await db.TableExistsAsync("testtable1");
            Assert.IsTrue(tableExists);
        }

        [Test]
        public void GetTableAsync_GivenNullName_ThrowsArgumentNullException()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            Assert.ThrowsAsync<ArgumentNullException>(async () => await db.GetTableAsync(null));
        }

        [Test]
        public async Task GetTableAsync_WhenTablePresent_ReturnsTable()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var table = await db.GetTableAsync("TestTable1");
            Assert.NotNull(table);
        }

        [Test]
        public async Task GetTableAsync_WhenTableMissing_ReturnsNull()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var table =  await db.GetTableAsync("table_that_doesnt_exist");
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
