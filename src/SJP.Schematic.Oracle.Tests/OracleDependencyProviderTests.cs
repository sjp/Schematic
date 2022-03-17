using System.Linq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests;

[TestFixture]
internal static class OracleDependencyProviderTests
{
    [Test]
    public static void Ctor_GivenNullComparer_CreatesWithoutError()
    {
        Assert.That(() => new OracleDependencyProvider(null), Throws.Nothing);
    }

    [Test]
    public static void GetDependencies_GivenNullObjectName_ThrowsArgumentsNullException()
    {
        var provider = new OracleDependencyProvider();

        Assert.That(() => provider.GetDependencies(null, "test"), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void GetDependencies_GivenNullOrWhiteSpaceExpression_ThrowsArgumentsNullException(string expression)
    {
        var provider = new OracleDependencyProvider();
        Identifier objectName = "test";

        Assert.That(() => provider.GetDependencies(objectName, expression), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetDependencies_GivenExpressionWithSameObjectAsTable_ReturnsEmptyCollection()
    {
        var provider = new OracleDependencyProvider();
        Identifier objectName = "test";
        const string expression = "select * from test";

        var dependencies = provider.GetDependencies(objectName, expression);

        Assert.That(dependencies, Is.Empty);
    }

    [Test]
    public static void GetDependencies_GivenExpressionWithSameObjectAsFunction_ReturnsEmptyCollection()
    {
        var provider = new OracleDependencyProvider();
        Identifier objectName = "test";
        const string expression = "select test(1)";

        var dependencies = provider.GetDependencies(objectName, expression);

        Assert.That(dependencies, Is.Empty);
    }

    [Test]
    public static void GetDependencies_GivenExpressionPointingToOtherTable_ReturnsOtherTable()
    {
        var provider = new OracleDependencyProvider();
        Identifier objectName = "test";
        const string expression = "select * from [other_table]";

        var dependencies = provider.GetDependencies(objectName, expression);
        var dependency = dependencies.Single();

        Assert.That(dependency.LocalName, Is.EqualTo("other_table"));
    }

    [Test]
    public static void GetDependencies_GivenExpressionPointingToOtherFunction_ReturnsOtherFunction()
    {
        var provider = new OracleDependencyProvider();
        Identifier objectName = "test";
        const string expression = "select other_function(1)";

        var dependencies = provider.GetDependencies(objectName, expression);
        var dependency = dependencies.Single();

        Assert.That(dependency.LocalName, Is.EqualTo("other_function"));
    }

    [Test]
    public static void GetDependencies_GivenExpressionPointingToOtherColumns_ReturnsColumnNames()
    {
        var provider = new OracleDependencyProvider();
        Identifier objectName = "test";
        const string expression = "([first_name] + ' ' + [last_name])";

        var dependencies = provider.GetDependencies(objectName, expression);
        var expectedNames = new[] { new Identifier("first_name"), new Identifier("last_name") };

        Assert.That(dependencies, Is.EqualTo(expectedNames));
    }

    [Test]
    public static void GetDependencies_GivenExpressionForViewPointingToTableAndFunction_ReturnsColumnsTablesAndFunctions()
    {
        var provider = new OracleDependencyProvider();
        Identifier objectName = "test_view";
        const string expression = @"
CREATE VIEW [test_view] AS
SELECT 'test' AS FIRST_COL, 1 AS SECOND_COL
FROM FIRST_TABLE
UNION
SELECT * from client.FunctionName('test')
";

        var dependencies = provider.GetDependencies(objectName, expression);
        var expectedNames = new[]
        {
            new Identifier("FIRST_COL"),
            new Identifier("SECOND_COL"),
            new Identifier("FIRST_TABLE"),
            new Identifier("client", "FunctionName")
        };

        Assert.That(dependencies, Is.EqualTo(expectedNames));
    }

    [Test]
    public static void GetDependencies_GivenExpressionForViewWithDuplicateNames_ReturnsUniqueDependencies()
    {
        var provider = new OracleDependencyProvider();
        Identifier objectName = "test_view";
        const string expression = @"
CREATE VIEW [test_view] AS
SELECT 'test' AS FIRST_COL, 1 AS SECOND_COL
FROM FIRST_TABLE
UNION
SELECT FIRST_COL, SECOND_COL from client.FunctionName('test')
";

        var dependencies = provider.GetDependencies(objectName, expression);
        var expectedNames = new[]
        {
            new Identifier("FIRST_COL"),
            new Identifier("SECOND_COL"),
            new Identifier("FIRST_TABLE"),
            new Identifier("client", "FunctionName")
        };

        Assert.That(dependencies, Is.EqualTo(expectedNames));
    }
}
