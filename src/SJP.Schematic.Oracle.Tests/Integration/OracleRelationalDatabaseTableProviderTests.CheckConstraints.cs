﻿using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests.Integration;

internal sealed partial class OracleRelationalDatabaseTableProviderTests : OracleTest
{
    [Test]
    public async Task Checks_WhenGivenTableWithNoChecks_ReturnsEmptyCollection()
    {
        var table = await GetTableAsync("table_test_table_1").ConfigureAwait(false);

        Assert.That(table.Checks, Is.Empty);
    }

    [Test]
    public async Task Checks_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
    {
        const string expectedCheckName = "CK_TEST_TABLE_14";

        var table = await GetTableAsync("table_test_table_14").ConfigureAwait(false);
        var check = table.Checks.Single();

        Assert.That(check.Name.UnwrapSome().LocalName, Is.EqualTo(expectedCheckName));
    }

    [Test]
    public async Task Checks_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
    {
        var table = await GetTableAsync("table_test_table_14").ConfigureAwait(false);
        var check = table.Checks.Single();

        Assert.That(check.Definition, Is.EqualTo("test_column > 1"));
    }

    [Test]
    public async Task Checks_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
    {
        var table = await GetTableAsync("table_test_table_14").ConfigureAwait(false);
        var check = table.Checks.Single();

        Assert.That(check.IsEnabled, Is.True);
    }

    [Test]
    public async Task Checks_WhenGivenTableWithDisabledCheck_ReturnsIsEnabledFalse()
    {
        var table = await GetTableAsync("table_test_table_32").ConfigureAwait(false);
        var check = table.Checks.Single();

        Assert.That(check.IsEnabled, Is.False);
    }
}