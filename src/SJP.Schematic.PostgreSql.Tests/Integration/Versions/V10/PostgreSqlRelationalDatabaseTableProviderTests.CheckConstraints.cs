using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V10
{
    internal partial class PostgreSqlRelationalDatabaseTableProviderTests : PostgreSql10Test
    {
        [Test]
        public async Task Checks_WhenGivenTableWithNoChecks_ReturnsEmptyCollection()
        {
            var table = await GetTableAsync("v10_table_test_table_1").ConfigureAwait(false);
            var count = table.Checks.Count;

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task Checks_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            var table = await GetTableAsync("v10_table_test_table_14").ConfigureAwait(false);
            var check = table.Checks.Single();

            Assert.AreEqual("ck_test_table_14", check.Name.UnwrapSome().LocalName);
        }

        [Test]
        public async Task Checks_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            var table = await GetTableAsync("v10_table_test_table_14").ConfigureAwait(false);
            var check = table.Checks.Single();

            Assert.AreEqual("(test_column > 1)", check.Definition);
        }

        [Test]
        public async Task Checks_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
        {
            var table = await GetTableAsync("v10_table_test_table_14").ConfigureAwait(false);
            var check = table.Checks.Single();

            Assert.IsTrue(check.IsEnabled);
        }
    }
}