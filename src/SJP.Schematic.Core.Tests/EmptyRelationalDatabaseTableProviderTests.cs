using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class EmptyRelationalDatabaseTableProviderTests
    {
        [Test]
        public static void GetTable_GivenNullName_ThrowsArgumentNullException()
        {
            var provider = new EmptyRelationalDatabaseTableProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetTable(null));
        }

        [Test]
        public static async Task GetTable_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyRelationalDatabaseTableProvider();
            var table = provider.GetTable("table_name");
            var tableIsNone = await table.IsNone.ConfigureAwait(false);

            Assert.IsTrue(tableIsNone);
        }

        [Test]
        public static async Task GetAllTables_PropertyGet_HasCountOfZero()
        {
            var provider = new EmptyRelationalDatabaseTableProvider();
            var tables = await provider.GetAllTables().ConfigureAwait(false);

            Assert.Zero(tables.Count);
        }

        [Test]
        public static async Task GetAllTables_WhenEnumerated_ContainsNoValues()
        {
            var provider = new EmptyRelationalDatabaseTableProvider();
            var tables = await provider.GetAllTables().ConfigureAwait(false);
            var tablesList = tables.ToList();

            Assert.Zero(tablesList.Count);
        }
    }
}
