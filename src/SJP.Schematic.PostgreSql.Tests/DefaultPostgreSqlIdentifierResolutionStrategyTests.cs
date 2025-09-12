using System.Linq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests;

[TestFixture]
internal static class DefaultPostgreSqlIdentifierResolutionStrategyTests
{
    [Test]
    public static void GetResolutionOrder_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();
        Assert.That(() => identifierResolver.GetResolutionOrder(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetResolutionOrder_GivenFullyLowercaseAndQualifiedIdentifier_ReturnsOneIdentifier()
    {
        var input = new Identifier("a", "b", "c", "d");
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result, Has.Exactly(1).Items);
    }

    [Test]
    public static void GetResolutionOrder_GivenFullyLowercaseAndQualifiedIdentifier_ReturnsIdentifierEqualToInput()
    {
        var input = new Identifier("a", "b", "c", "d");
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result[0], Is.EqualTo(input));
    }

    [Test]
    public static void GetResolutionOrder_GivenUppercaseServerIdentifier_ReturnsOneIdentifier()
    {
        var input = new Identifier("A", "b", "c", "d");
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result, Has.Exactly(1).Items);
    }

    [Test]
    public static void GetResolutionOrder_GivenUppercaseServerIdentifier_ReturnsIdentifierEqualToInput()
    {
        var input = new Identifier("A", "b", "c", "d");
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result[0], Is.EqualTo(input));
    }

    [Test]
    public static void GetResolutionOrder_GivenUppercaseDatabaseIdentifier_ReturnsOneIdentifier()
    {
        var input = new Identifier("a", "B", "c", "d");
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result, Has.Exactly(1).Items);
    }

    [Test]
    public static void GetResolutionOrder_GivenUppercaseDatabaseIdentifier_ReturnsLowercasedIdentifier()
    {
        var input = new Identifier("a", "B", "c", "d");
        var expectedResults = new[] { new Identifier("a", "b", "c", "d") };
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result, Is.EqualTo(expectedResults));
    }

    [Test]
    public static void GetResolutionOrder_GivenUppercaseSchemaIdentifier_ReturnsTwoIdentifiers()
    {
        var input = new Identifier("a", "b", "C", "d");
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result, Has.Exactly(2).Items);
    }

    [Test]
    public static void GetResolutionOrder_GivenUppercaseSchemaIdentifier_ReturnsLowercasedIdentifierAndIdentifierEqualToInput()
    {
        var input = new Identifier("a", "b", "C", "d");
        var expectedResults = new[]
        {
            new Identifier("a", "b", "c", "d"),
            input,
        };
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result, Is.EqualTo(expectedResults));
    }

    [Test]
    public static void GetResolutionOrder_GivenUppercaseLocalNameIdentifier_ReturnsTwoIdentifiers()
    {
        var input = new Identifier("a", "b", "c", "D");
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result, Has.Exactly(2).Items);
    }

    [Test]
    public static void GetResolutionOrder_GivenUppercaseLocalNameIdentifier_ReturnsLowercasedIdentifierAndIdentifierEqualToInput()
    {
        var input = new Identifier("a", "b", "c", "D");
        var expectedResults = new[]
        {
            new Identifier("a", "b", "c", "d"),
            input,
        };
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result, Is.EqualTo(expectedResults));
    }

    [Test]
    public static void GetResolutionOrder_GivenUppercaseSchemaAndLocalNameIdentifier_ReturnsFourIdentifiers()
    {
        var input = new Identifier("a", "b", "C", "D");
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result, Has.Exactly(4).Items);
    }

    [Test]
    public static void GetResolutionOrder_GivenUppercaseSchemaAndLocalNameIdentifier_ReturnsExpectedResults()
    {
        var input = new Identifier("a", "b", "C", "D");
        var expectedResults = new[]
        {
            new Identifier("a", "b", "c", "d"),
            new Identifier("a", "b", "c", "D"),
            new Identifier("a", "b", "C", "d"),
            input,
        };
        var identifierResolver = new DefaultPostgreSqlIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result, Is.EqualTo(expectedResults));
    }
}