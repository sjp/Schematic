using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests.Integration;

internal sealed partial class OracleRelationalDatabaseTableProviderTests : OracleTest
{
    [Test]
    public async Task Checks_WhenGivenTableWithNoChecks_ReturnsEmptyCollection()
    {
        var table = await GetTableAsync("table_test_table_1");

        Assert.That(table.Checks, Is.Empty);
    }

    [Test]
    public async Task Checks_WhenGivenTableWithCheck_ReturnsContraintWithCorrectName()
    {
        const string expectedCheckName = "CK_TEST_TABLE_14";

        var table = await GetTableAsync("table_test_table_14");
        var check = table.Checks.Single();

        Assert.That(check.Name.UnwrapSome().LocalName, Is.EqualTo(expectedCheckName));
    }

    [Test]
    public async Task Checks_WhenGivenTableWithCheck_ReturnsContraintWithDefinition()
    {
        var table = await GetTableAsync("table_test_table_14");
        var check = table.Checks.Single();

        Assert.That(check.Definition, Is.EqualTo("test_column > 1"));
    }

    [Test]
    public async Task Checks_WhenGivenTableWithEnabledCheck_ReturnsIsEnabledTrue()
    {
        var table = await GetTableAsync("table_test_table_14");
        var check = table.Checks.Single();

        Assert.That(check.IsEnabled, Is.True);
    }

    [Test]
    public async Task Checks_WhenGivenTableWithDisabledCheck_ReturnsIsEnabledFalse()
    {
        var table = await GetTableAsync("table_test_table_32");
        var check = table.Checks.Single();

        Assert.That(check.IsEnabled, Is.False);
    }

    [Test]
    public async Task Checks_WhenGivenTableWithSystemNamedNotNullConstraint_DoesNotReturnConstraint()
    {
        var table = await GetTableAsync("table_test_table_33");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.Checks, Is.Empty);
            Assert.That(table.Columns.Single().IsNullable, Is.False);
        }
    }

    [Test]
    public async Task Checks_WhenGivenTableWithUserNamedNotNullConstraint_ReturnsConstraint()
    {
        var table = await GetTableAsync("table_test_table_41");
        var check = table.Checks.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(check.Name.UnwrapSome().LocalName, Is.EqualTo("NN_TEST_TABLE_41"));
            Assert.That(check.Definition, Is.EqualTo("\"TEST_COLUMN\" IS NOT NULL"));
            Assert.That(check.IsEnabled, Is.True);
            Assert.That(table.Columns.Single().IsNullable, Is.False);
        }
    }

    // Oracle only reports a column as NOT NULL while its constraint is enabled and validated, so a
    // disabled NOT NULL leaves the column nullable. The constraint is still surfaced as a check,
    // otherwise it would be invisible in the model.
    [Test]
    public async Task Checks_WhenGivenTableWithDisabledNotNullConstraint_ReturnsConstraintAndNullableColumn()
    {
        var table = await GetTableAsync("table_test_table_42");
        var check = table.Checks.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(check.Name.UnwrapSome().LocalName, Is.EqualTo("NN_TEST_TABLE_42"));
            Assert.That(check.IsEnabled, Is.False);
            Assert.That(table.Columns.Single().IsNullable, Is.True);
        }
    }

    [Test]
    public async Task Checks_WhenGivenNovalidateCheck_ReturnsIsValidatedFalse()
    {
        var table = await GetTableAsync("constraint_state_child");
        var check = table.Checks.Single(c => c.Name.UnwrapSome().LocalName == "CK_CONSTRAINT_STATE_CHILD");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(check.IsEnabled, Is.True);
            Assert.That(check.IsValidated, Is.False);
            Assert.That(check.Deferrability, Is.EqualTo(ConstraintDeferrability.NotDeferrable));
        }
    }
}
