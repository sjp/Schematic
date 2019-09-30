using System;
using System.Threading.Tasks;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Modelled.Reflection.Model;
using SJP.Schematic.Modelled.Reflection.Tests.Fakes;
using SJP.Schematic.Modelled.Reflection.Tests.Fakes.ColumnTypes;

namespace SJP.Schematic.Modelled.Reflection.Tests
{
    [TestFixture]
    internal static class ReflectionRelationalDatabaseTests
    {
        private static IIdentifierDefaults IdentifierDefaults { get; } = new IdentifierDefaults(null, null, "schema");

        [Test]
        public static void CtorT_GivenNullDialect_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ReflectionRelationalDatabase<SampleDatabase>(null, IdentifierDefaults));
        }

        [Test]
        public static void CtorT_GivenNullDefaults_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect(), null));
        }

        [Test]
        public static void Ctor_GivenNullArguments_ThrowsArgumentNullException()
        {
            var dialect = new FakeDialect();
            var dbType = typeof(SampleDatabase);

            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentNullException>(() => new ReflectionRelationalDatabase(null, dbType, IdentifierDefaults));
                Assert.Throws<ArgumentNullException>(() => new ReflectionRelationalDatabase(dialect, null, IdentifierDefaults));
                Assert.Throws<ArgumentNullException>(() => new ReflectionRelationalDatabase(dialect, dbType, null));
            });
        }

        [Test]
        public static void GetTable_GivenNullName_ThrowsArgumentNullException()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect(), IdentifierDefaults);
            Assert.Throws<ArgumentNullException>(() => db.GetTable(null));
        }

        [Test]
        public static async Task GetTable_WhenTablePresent_ReturnsTable()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect(), IdentifierDefaults);
            var lookupName = Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Schema, nameof(SampleDatabase.TestTable1));
            var tableIsSome = await db.GetTable(lookupName).IsSome.ConfigureAwait(false);

            Assert.IsTrue(tableIsSome);
        }

        [Test]
        public static async Task GetTable_WhenTableMissing_ReturnsNone()
        {
            var db = new ReflectionRelationalDatabase<SampleDatabase>(new FakeDialect(), IdentifierDefaults);
            var tableIsNone =  await db.GetTable("table_that_doesnt_exist").IsNone.ConfigureAwait(false);

            Assert.IsTrue(tableIsNone);
        }

        private class SampleDatabase
        {
            public Table<TestTable1> FirstTestTable { get; }

            public class TestTable1
            {
                public Column<BigInteger> TEST_TABLE_ID { get; }

                public Column<Option<Varchar200>> TEST_STRING { get; }

                public Key PK_TEST_TABLE => new Key.Primary(TEST_TABLE_ID);

                public ComputedColumn TEST_COMPUTED => new ComputedColumn("@TestString + @TestString", new { TestString = TEST_STRING });
            }
        }
    }
}
