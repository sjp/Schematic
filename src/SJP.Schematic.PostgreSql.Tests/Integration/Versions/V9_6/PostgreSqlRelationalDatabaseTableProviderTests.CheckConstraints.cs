using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V9_6
{
    internal sealed partial class PostgreSqlRelationalDatabaseTableProviderTests : PostgreSql96Test
    {
        [Test]
        public async Task Checks_WhenGivenTableWithNoChecks_ReturnsEmptyCollection()
        {
            var table = await GetTableAsync("v96_table_test_table_1").ConfigureAwait(false);

            Assert.That(table.Checks, Is.Empty);
        }

        [Test]
        public async Task Checks_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
        {
            var table = await GetTableAsync("v96_table_test_table_14").ConfigureAwait(false);
            var check = table.Checks.Single();

            Assert.That(check.Name.UnwrapSome().LocalName, Is.EqualTo("ck_test_table_14"));
        }

        [Test]
        public async Task Checks_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
        {
            var table = await GetTableAsync("v96_table_test_table_14").ConfigureAwait(false);
            var check = table.Checks.Single();

            Assert.That(check.Definition, Is.EqualTo("(test_column > 1)"));
        }

        [Test]
        public async Task Checks_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
        {
            var table = await GetTableAsync("v96_table_test_table_14").ConfigureAwait(false);
            var check = table.Checks.Single();

            Assert.That(check.IsEnabled, Is.True);
        }
    }
}