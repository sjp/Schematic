using System;
using NUnit.Framework;
using SJP.Schematic.Core;
using System.Linq;

namespace SJP.Schematic.Oracle.Tests
{
    [TestFixture]
    internal static class DefaultOracleIdentifierResolutionStrategyTests
    {
        [Test]
        public static void GetResolutionOrder_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();
            Assert.Throws<ArgumentNullException>(() => identifierResolver.GetResolutionOrder(null));
        }

        [Test]
        public static void GetResolutionOrder_GivenFullyUppercaseAndQualifiedIdentifier_ReturnsOneIdentifier()
        {
            var input = new Identifier("A", "B", "C", "D");
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var result = identifierResolver.GetResolutionOrder(input).ToList();

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public static void GetResolutionOrder_GivenFullyUppercaseAndQualifiedIdentifier_ReturnsIdentifierEqualToInput()
        {
            var input = new Identifier("A", "B", "C", "D");
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var result = identifierResolver.GetResolutionOrder(input).ToList();

            Assert.AreEqual(input, result[0]);
        }

        [Test]
        public static void GetResolutionOrder_GivenLowercaseServerIdentifier_ReturnsOneIdentifier()
        {
            var input = new Identifier("a", "B", "C", "D");
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var result = identifierResolver.GetResolutionOrder(input).ToList();

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public static void GetResolutionOrder_GivenLowercaseServerIdentifier_ReturnsIdentifierEqualToInput()
        {
            var input = new Identifier("a", "B", "C", "D");
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var result = identifierResolver.GetResolutionOrder(input).ToList();

            Assert.AreEqual(input, result[0]);
        }

        [Test]
        public static void GetResolutionOrder_GivenLowercaseDatabaseIdentifier_ReturnsOneIdentifier()
        {
            var input = new Identifier("A", "b", "C", "D");
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var result = identifierResolver.GetResolutionOrder(input).ToList();

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public static void GetResolutionOrder_GivenLowercaseDatabaseIdentifier_ReturnsUppercasedIdentifier()
        {
            var input = new Identifier("A", "b", "C", "D");
            var expectedResults = new[] { new Identifier("A", "B", "C", "D") };
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var result = identifierResolver.GetResolutionOrder(input).ToList();
            var equalResults = expectedResults.SequenceEqual(result);

            Assert.IsTrue(equalResults);
        }

        [Test]
        public static void GetResolutionOrder_GivenLowercaseSchemaIdentifier_ReturnsTwoIdentifiers()
        {
            var input = new Identifier("A", "B", "c", "D");
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var result = identifierResolver.GetResolutionOrder(input).ToList();

            Assert.AreEqual(2, result.Count);
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
            var equalResults = expectedResults.SequenceEqual(result);

            Assert.IsTrue(equalResults);
        }

        [Test]
        public static void GetResolutionOrder_GivenLowercaseLocalNameIdentifier_ReturnsTwoIdentifiers()
        {
            var input = new Identifier("A", "B", "C", "d");
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var result = identifierResolver.GetResolutionOrder(input).ToList();

            Assert.AreEqual(2, result.Count);
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
            var equalResults = expectedResults.SequenceEqual(result);

            Assert.IsTrue(equalResults);
        }

        [Test]
        public static void GetResolutionOrder_GivenLowercaseSchemaAndLocalNameIdentifier_ReturnsFourIdentifiers()
        {
            var input = new Identifier("A", "B", "c", "d");
            var identifierResolver = new DefaultOracleIdentifierResolutionStrategy();

            var result = identifierResolver.GetResolutionOrder(input).ToList();

            Assert.AreEqual(4, result.Count);
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
            var equalResults = expectedResults.SequenceEqual(result);

            Assert.IsTrue(equalResults);
        }
    }
}
