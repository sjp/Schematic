using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schema.Modelled.Reflection.Model;
using SJP.Schema.Modelled.Reflection.Tests.Fakes;
using SJP.Schema.Modelled.Reflection.Tests.Fakes.ColumnTypes;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Tests
{
    [TestFixture]
    public class ReflectionTableTests
    {
        [Test]
        public void ReflectionDatabaseTThrowsArgumentExceptions()
        {
            Assert.Throws<ArgumentNullException>(() => new ReflectionRelationalDatabase<SampleDatabase>(null));
        }

        [Test]
        public void ReflectionDatabaseThrowsArgumentExceptions()
        {
            var dialect = new FakeDialect();
            var dbType = typeof(SampleDatabase);
            Assert.Throws<ArgumentNullException>(() => new ReflectionRelationalDatabase(null, dbType));
            Assert.Throws<ArgumentNullException>(() => new ReflectionRelationalDatabase(dialect, null));
        }

        [Test]
        public void ReflectionDatabaseTestTableExists()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var tableExists = db.TableExists("TestTable1");
            Assert.IsTrue(tableExists);
        }

        [Test]
        public void ReflectionDatabaseTestTableExistsWithCaseInsensitiveName()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var tableExists = db.TableExists("testtable1");
            Assert.IsTrue(tableExists);
        }

        [Test]
        public void ReflectionDatabaseTestTableExistsWithCaseInsensitiveNameAndDefaultSchemaSet()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var tableExists = db.TableExists(new Core.Identifier("testtable1"));
            Assert.IsTrue(tableExists);
        }

        [Test]
        public void ReflectionDatabaseReturnsTestTable()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var table = db.GetTable("TestTable1");
            Assert.NotNull(table);
        }

        [Test]
        public async Task ReflectionDatabaseTestTableExistsAsync()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var tableExists = await db.TableExistsAsync("TestTable1");
            Assert.IsTrue(tableExists);
        }

        [Test]
        public async Task ReflectionDatabaseReturnsTestTableAsync()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect());
            var table = await db.GetTableAsync("TestTable1");
            Assert.NotNull(table);
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
