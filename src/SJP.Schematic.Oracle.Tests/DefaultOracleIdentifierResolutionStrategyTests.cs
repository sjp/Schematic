using System.Linq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests;

[TestFixture]
internal static class DefaultOracleIdentifierResolutionStrategyTests
{
    [Test]
    public static void GetResolutionOrder_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();
        Assert.That(() => identifierResolver.GetResolutionOrder(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetResolutionOrder_GivenFullyUppercaseAndQualifiedIdentifier_ReturnsOneIdentifier()
    {
        var input = new Identifier("A", "B", "C", "D");
        var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result, Has.Exactly(1).Items);
    }

    [Test]
    public static void GetResolutionOrder_GivenFullyUppercaseAndQualifiedIdentifier_ReturnsIdentifierEqualToInput()
    {
        var input = new Identifier("A", "B", "C", "D");
        var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result[0], Is.EqualTo(input));
    }

    [Test]
    public static void GetResolutionOrder_GivenLowercaseServerIdentifier_ReturnsOneIdentifier()
    {
        var input = new Identifier("a", "B", "C", "D");
        var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result, Has.Exactly(1).Items);
    }

    [Test]
    public static void GetResolutionOrder_GivenLowercaseServerIdentifier_ReturnsIdentifierEqualToInput()
    {
        var input = new Identifier("a", "B", "C", "D");
        var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result[0], Is.EqualTo(input));
    }

    [Test]
    public static void GetResolutionOrder_GivenLowercaseDatabaseIdentifier_ReturnsOneIdentifier()
    {
        var input = new Identifier("A", "b", "C", "D");
        var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result, Has.Exactly(1).Items);
    }

    [Test]
    public static void GetResolutionOrder_GivenLowercaseDatabaseIdentifier_ReturnsUppercasedIdentifier()
    {
        var input = new Identifier("A", "b", "C", "D");
        var expectedResults = new[] { new Identifier("A", "B", "C", "D") };
        var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result, Is.EqualTo(expectedResults));
    }

    [Test]
    public static void GetResolutionOrder_GivenLowercaseSchemaIdentifier_ReturnsTwoIdentifiers()
    {
        var input = new Identifier("A", "B", "c", "D");
        var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result, Has.Exactly(2).Items);
    }

    [Test]
    public static void GetResolutionOrder_GivenLowercaseSchemaIdentifier_ReturnsUppercasedIdentifierAndIdentifierEqualToInput()
    {
        var input = new Identifier("A", "B", "c", "D");
        var expectedResults = new[]
        {
            new Identifier("A", "B", "C", "D"),
            input
        };
        var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result, Is.EqualTo(expectedResults));
    }

    [Test]
    public static void GetResolutionOrder_GivenLowercaseLocalNameIdentifier_ReturnsTwoIdentifiers()
    {
        var input = new Identifier("A", "B", "C", "d");
        var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result, Has.Exactly(2).Items);
    }

    [Test]
    public static void GetResolutionOrder_GivenLowercaseLocalNameIdentifier_ReturnsUppercasedIdentifierAndIdentifierEqualToInput()
    {
        var input = new Identifier("A", "B", "C", "d");
        var expectedResults = new[]
        {
            new Identifier("A", "B", "C", "D"),
            input
        };
        var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result, Is.EqualTo(expectedResults));
    }

    [Test]
    public static void GetResolutionOrder_GivenLowercaseSchemaAndLocalNameIdentifier_ReturnsFourIdentifiers()
    {
        var input = new Identifier("A", "B", "c", "d");
        var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result, Has.Exactly(4).Items);
    }

    [Test]
    public static void GetResolutionOrder_GivenLowercaseSchemaAndLocalNameIdentifier_ReturnsExpectedResults()
    {
        var input = new Identifier("A", "B", "c", "d");
        var expectedResults = new[]
        {
            new Identifier("A", "B", "C", "D"),
            new Identifier("A", "B", "C", "d"),
            new Identifier("A", "B", "c", "D"),
            input
        };
        var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

        var result = identifierResolver.GetResolutionOrder(input).ToList();

        Assert.That(result, Is.EqualTo(expectedResults));
    }
}