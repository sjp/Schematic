using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests;

internal static class PostgreSqlDependencyProviderTests
{
    [Test]
    public static void Ctor_GivenNullComparer_CreatesWithoutError()
    {
        Assert.That(() => new PostgreSqlDependencyProvider(null), Throws.Nothing);
    }

    [Test]
    public static void GetDependencies_GivenNullObjectName_ThrowsArgumentsNullException()
    {
        var provider = new PostgreSqlDependencyProvider();

        Assert.That(
            () => provider.GetDependencies(null, "test"),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("objectName")
        );
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void GetDependencies_GivenNullOrWhiteSpaceExpression_ThrowsArgumentsException(string expression)
    {
        var provider = new PostgreSqlDependencyProvider();
        Identifier objectName = "test";

        Assert.That(
            () => provider.GetDependencies(objectName, expression),
            Throws.InstanceOf<ArgumentException>().With.Property(nameof(ArgumentException.ParamName)).EqualTo("expression")
        );
    }

    [Test]
    public static void GetDependencies_GivenExpressionWithSameObjectAsTable_ReturnsEmptyCollection()
    {
        var provider = new PostgreSqlDependencyProvider();
        Identifier objectName = "test";
        const string expression = "select * from test";

        var dependencies = provider.GetDependencies(objectName, expression);

        Assert.That(dependencies, Is.Empty);
    }

    [Test]
    public static void GetDependencies_GivenExpressionWithSameObjectAsFunction_ReturnsEmptyCollection()
    {
        var provider = new PostgreSqlDependencyProvider();
        Identifier objectName = "test";
        const string expression = "select test(1)";

        var dependencies = provider.GetDependencies(objectName, expression);

        Assert.That(dependencies, Is.Empty);
    }

    [Test]
    public static void GetDependencies_GivenExpressionPointingToOtherTable_ReturnsOtherTable()
    {
        var provider = new PostgreSqlDependencyProvider();
        Identifier objectName = "test";
        const string expression = "select * from \"other_table\"";

        var dependencies = provider.GetDependencies(objectName, expression);
        var dependency = dependencies.Single();

        Assert.That(dependency.LocalName, Is.EqualTo("other_table"));
    }

    [Test]
    public static void GetDependencies_GivenExpressionPointingToOtherFunction_ReturnsOtherFunction()
    {
        var provider = new PostgreSqlDependencyProvider();
        Identifier objectName = "test";
        const string expression = "select other_function(1)";

        var dependencies = provider.GetDependencies(objectName, expression);
        var dependency = dependencies.Single();

        Assert.That(dependency.LocalName, Is.EqualTo("other_function"));
    }

    [Test]
    public static void GetDependencies_GivenComputedColumnExpressionPointingToOtherColumns_ReturnsColumnNames()
    {
        var provider = new PostgreSqlDependencyProvider();
        Identifier objectName = "test";
        const string expression = "(\"first_name\" || ' ' || \"last_name\")";

        var dependencies = provider.GetDependencies(objectName, expression);
        var expectedNames = new[] { new Identifier("first_name"), new Identifier("last_name") };

        Assert.That(dependencies, Is.EqualTo(expectedNames));
    }

    [Test]
    public static void GetDependencies_GivenViewBodyPointingToTableAndFunction_ReturnsColumnsTablesAndFunctions()
    {
        var provider = new PostgreSqlDependencyProvider();
        Identifier objectName = "test_view";
        const string expression = @"
SELECT 'test' AS FIRST_COL, 1 AS SECOND_COL
FROM FIRST_TABLE
UNION
SELECT * from public.FunctionName('test')
";

        var dependencies = provider.GetDependencies(objectName, expression);
        var expectedNames = new[]
        {
            new Identifier("FIRST_COL"),
            new Identifier("SECOND_COL"),
            new Identifier("FIRST_TABLE"),
            new Identifier("public", "FunctionName"),
        };

        Assert.That(dependencies, Is.EqualTo(expectedNames));
    }

    [Test]
    public static void GetDependencies_GivenExpressionWithNonReservedKeywordAsName_ReturnsNameAsDependency()
    {
        // 'value' is a non-reserved keyword in PostgreSQL and so is valid as an object name when unquoted.
        var provider = new PostgreSqlDependencyProvider();
        Identifier objectName = "test";
        const string expression = "select * from value";

        var dependencies = provider.GetDependencies(objectName, expression);
        var dependency = dependencies.Single();

        Assert.That(dependency.LocalName, Is.EqualTo("value"));
    }

    [Test]
    public static void GetDependencies_GivenDoubleQuotedIdentifier_ReturnsUnquotedName()
    {
        // PostgreSQL delimited identifiers are double-quoted and preserve case/spacing.
        var provider = new PostgreSqlDependencyProvider();
        Identifier objectName = "test";
        const string expression = "select * from \"Other Table\"";

        var dependencies = provider.GetDependencies(objectName, expression);
        var dependency = dependencies.Single();

        Assert.That(dependency.LocalName, Is.EqualTo("Other Table"));
    }

    [Test]
    public static void GetDependencies_GivenQualifiedName_ReturnsQualifiedDependency()
    {
        var provider = new PostgreSqlDependencyProvider();
        Identifier objectName = "test";
        const string expression = "select * from \"my_schema\".\"my_table\"";

        var dependencies = provider.GetDependencies(objectName, expression);
        var dependency = dependencies.Single();
        var expected = Identifier.CreateQualifiedIdentifier("my_schema", "my_table");

        Assert.That(dependency, Is.EqualTo(expected));
    }

    [Test]
    public static void GetDependencies_GivenViewBodyWithDuplicateNames_ReturnsUniqueDependencies()
    {
        var provider = new PostgreSqlDependencyProvider();
        Identifier objectName = "test_view";
        const string expression = @"
SELECT 'test' AS FIRST_COL, 1 AS SECOND_COL
FROM FIRST_TABLE
UNION
SELECT FIRST_COL, SECOND_COL from public.FunctionName('test')
";

        var dependencies = provider.GetDependencies(objectName, expression);
        var expectedNames = new[]
        {
            new Identifier("FIRST_COL"),
            new Identifier("SECOND_COL"),
            new Identifier("FIRST_TABLE"),
            new Identifier("public", "FunctionName"),
        };

        Assert.That(dependencies, Is.EqualTo(expectedNames));
    }

    [Test]
    public static void GetDependencies_GivenRoutineDefinitionWithDollarQuotedBody_ReturnsNamesFromTheBody()
    {
        var provider = new PostgreSqlDependencyProvider();
        Identifier objectName = Identifier.CreateQualifiedIdentifier("app", "film_count");
        const string expression = @"
CREATE OR REPLACE FUNCTION app.film_count()
 RETURNS integer
 LANGUAGE plpgsql
AS $function$
begin
  return (select count(*) from public.film);
end;
$function$
";

        var dependencies = provider.GetDependencies(objectName, expression);

        Assert.That(dependencies, Contains.Item(Identifier.CreateQualifiedIdentifier("public", "film")));
    }

    [Test]
    public static void GetDependencies_GivenBodyQuotingAnotherBody_ReturnsNamesFromBoth()
    {
        var provider = new PostgreSqlDependencyProvider();
        Identifier objectName = "outer_function";
        const string expression = @"
CREATE FUNCTION outer_function() RETURNS void LANGUAGE plpgsql AS $outer$
begin
  execute $inner$ select * from inner_table $inner$;
  perform count(*) from outer_table;
end;
$outer$
";

        var dependencies = provider.GetDependencies(objectName, expression);

        Assert.That(dependencies, Contains.Item(new Identifier("outer_table")));
        Assert.That(dependencies, Contains.Item(new Identifier("inner_table")));
    }

    [Test]
    public static void GetDependencies_GivenBodyThatIsNotSql_ReturnsNamesFromTheSignature()
    {
        // A body may be written in any language the server has installed, so what cannot be
        // tokenized must not fail the definition surrounding it.
        var provider = new PostgreSqlDependencyProvider();
        Identifier objectName = "python_function";
        const string expression = @"
CREATE FUNCTION python_function(other_table_count integer) RETURNS integer LANGUAGE plpython3u AS $$
return other_table_count # don't count on it
$$
";

        IReadOnlyCollection<Identifier> dependencies = null;

        Assert.That(() => dependencies = provider.GetDependencies(objectName, expression), Throws.Nothing);
        Assert.That(dependencies, Contains.Item(new Identifier("other_table_count")));
    }

    [Test]
    public static void GetDependencies_WhenInvokedConcurrently_ReturnsConsistentResults()
    {
        // Guards against sharing a non-thread-safe lexer across concurrent callers,
        // which the reporting layer does when rendering views in parallel.
        var provider = new PostgreSqlDependencyProvider();
        Identifier objectName = "test_view";
        const string expression = @"
SELECT 'test' AS FIRST_COL, 1 AS SECOND_COL
FROM FIRST_TABLE
UNION
SELECT * from public.FunctionName('test')
";
        var expectedNames = new[]
        {
            new Identifier("FIRST_COL"),
            new Identifier("SECOND_COL"),
            new Identifier("FIRST_TABLE"),
            new Identifier("public", "FunctionName"),
        };

        var results = new ConcurrentBag<IReadOnlyCollection<Identifier>>();
        Parallel.For(0, 200, _ => results.Add(provider.GetDependencies(objectName, expression)));

        Assert.That(results, Has.Count.EqualTo(200));
        Assert.That(results, Has.All.EqualTo(expectedNames));
    }
}
