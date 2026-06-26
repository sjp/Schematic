using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Tests;

[TestFixture]
internal static class MySqlDependencyProviderTests
{
    [Test]
    public static void Ctor_GivenNullComparer_CreatesWithoutError()
    {
        Assert.That(() => new MySqlDependencyProvider(null), Throws.Nothing);
    }

    [Test]
    public static void GetDependencies_GivenNullObjectName_ThrowsArgumentsNullException()
    {
        var provider = new MySqlDependencyProvider();

        Assert.That(() => provider.GetDependencies(null, "test"), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void GetDependencies_GivenNullOrWhiteSpaceExpression_ThrowsArgumentsException(string expression)
    {
        var provider = new MySqlDependencyProvider();
        Identifier objectName = "test";

        Assert.That(() => provider.GetDependencies(objectName, expression), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void GetDependencies_GivenExpressionWithSameObjectAsTable_ReturnsEmptyCollection()
    {
        var provider = new MySqlDependencyProvider();
        Identifier objectName = "test";
        const string expression = "select * from test";

        var dependencies = provider.GetDependencies(objectName, expression);

        Assert.That(dependencies, Is.Empty);
    }

    [Test]
    public static void GetDependencies_GivenExpressionWithSameObjectAsFunction_ReturnsEmptyCollection()
    {
        var provider = new MySqlDependencyProvider();
        Identifier objectName = "test";
        const string expression = "select test(1)";

        var dependencies = provider.GetDependencies(objectName, expression);

        Assert.That(dependencies, Is.Empty);
    }

    [Test]
    public static void GetDependencies_GivenExpressionPointingToOtherTable_ReturnsOtherTable()
    {
        var provider = new MySqlDependencyProvider();
        Identifier objectName = "test";
        const string expression = "select * from `other_table`";

        var dependencies = provider.GetDependencies(objectName, expression);
        var dependency = dependencies.Single();

        Assert.That(dependency.LocalName, Is.EqualTo("other_table"));
    }

    [Test]
    public static void GetDependencies_GivenExpressionPointingToOtherFunction_ReturnsOtherFunction()
    {
        var provider = new MySqlDependencyProvider();
        Identifier objectName = "test";
        const string expression = "select other_function(1)";

        var dependencies = provider.GetDependencies(objectName, expression);
        var dependency = dependencies.Single();

        Assert.That(dependency.LocalName, Is.EqualTo("other_function"));
    }

    [Test]
    public static void GetDependencies_GivenComputedColumnExpressionPointingToOtherColumns_ReturnsColumnNames()
    {
        var provider = new MySqlDependencyProvider();
        Identifier objectName = "test";
        const string expression = "(`first_name` + ' ' + `last_name`)";

        var dependencies = provider.GetDependencies(objectName, expression);
        var expectedNames = new[] { new Identifier("first_name"), new Identifier("last_name") };

        Assert.That(dependencies, Is.EqualTo(expectedNames));
    }

    [Test]
    public static void GetDependencies_GivenViewBodyPointingToTableAndFunction_ReturnsColumnsTablesAndFunctions()
    {
        var provider = new MySqlDependencyProvider();
        Identifier objectName = "test_view";
        const string expression = @"
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
            new Identifier("client", "FunctionName"),
        };

        Assert.That(dependencies, Is.EqualTo(expectedNames));
    }

    [Test]
    public static void GetDependencies_GivenExpressionWithNonReservedKeywordAsName_ReturnsNameAsDependency()
    {
        // 'client' is a non-reserved keyword in MySQL and so is valid as an object name when unquoted.
        var provider = new MySqlDependencyProvider();
        Identifier objectName = "test";
        const string expression = "select * from client";

        var dependencies = provider.GetDependencies(objectName, expression);
        var dependency = dependencies.Single();

        Assert.That(dependency.LocalName, Is.EqualTo("client"));
    }

    [Test]
    public static void GetDependencies_GivenBacktickQuotedIdentifier_ReturnsUnquotedName()
    {
        var provider = new MySqlDependencyProvider();
        Identifier objectName = "test";
        const string expression = "select * from `Other Table`";

        var dependencies = provider.GetDependencies(objectName, expression);
        var dependency = dependencies.Single();

        Assert.That(dependency.LocalName, Is.EqualTo("Other Table"));
    }

    [Test]
    public static void GetDependencies_GivenDoubleQuotedIdentifier_ReturnsUnquotedName()
    {
        // The lexer is configured with ANSI_QUOTES, so a double-quoted name is a delimited identifier.
        var provider = new MySqlDependencyProvider();
        Identifier objectName = "test";
        const string expression = "select * from \"Other_Table\"";

        var dependencies = provider.GetDependencies(objectName, expression);
        var dependency = dependencies.Single();

        Assert.That(dependency.LocalName, Is.EqualTo("Other_Table"));
    }

    [Test]
    public static void GetDependencies_GivenQualifiedName_ReturnsQualifiedDependency()
    {
        var provider = new MySqlDependencyProvider();
        Identifier objectName = "test";
        const string expression = "select * from `my_db`.`my_table`";

        var dependencies = provider.GetDependencies(objectName, expression);
        var dependency = dependencies.Single();
        var expected = Identifier.CreateQualifiedIdentifier("my_db", "my_table");

        Assert.That(dependency, Is.EqualTo(expected));
    }

    [Test]
    public static void GetDependencies_GivenViewBodyWithDuplicateNames_ReturnsUniqueDependencies()
    {
        var provider = new MySqlDependencyProvider();
        Identifier objectName = "test_view";
        const string expression = @"
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
            new Identifier("client", "FunctionName"),
        };

        Assert.That(dependencies, Is.EqualTo(expectedNames));
    }

    [Test]
    public static void GetDependencies_WhenInvokedConcurrently_ReturnsConsistentResults()
    {
        // Guards against sharing a non-thread-safe lexer across concurrent callers,
        // which the reporting layer does when rendering views in parallel.
        var provider = new MySqlDependencyProvider();
        Identifier objectName = "test_view";
        const string expression = @"
SELECT 'test' AS FIRST_COL, 1 AS SECOND_COL
FROM FIRST_TABLE
UNION
SELECT * from client.FunctionName('test')
";
        var expectedNames = new[]
        {
            new Identifier("FIRST_COL"),
            new Identifier("SECOND_COL"),
            new Identifier("FIRST_TABLE"),
            new Identifier("client", "FunctionName"),
        };

        var results = new ConcurrentBag<IReadOnlyCollection<Identifier>>();
        Parallel.For(0, 200, _ => results.Add(provider.GetDependencies(objectName, expression)));

        Assert.That(results, Has.Count.EqualTo(200));
        Assert.That(results, Has.All.EqualTo(expectedNames));
    }
}
