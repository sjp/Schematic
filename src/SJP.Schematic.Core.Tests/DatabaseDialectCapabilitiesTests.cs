using System;
using System.Collections.Generic;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class DatabaseDialectCapabilitiesTests
{
    private static DatabaseDialectCapabilities CreateCapabilities() => new()
    {
        SupportedReferentialActions = new HashSet<ReferentialAction> { ReferentialAction.NoAction },
        MaxIdentifierLength = 128,
    };

    [Test]
    public static void SupportedReferentialActions_GivenNullSet_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new DatabaseDialectCapabilities { SupportedReferentialActions = null, MaxIdentifierLength = 128 },
            Throws.ArgumentNullException);
    }

    [Test]
    public static void SupportedReferentialActions_GivenInvalidEnumValue_ThrowsArgumentException()
    {
        Assert.That(
            () => new DatabaseDialectCapabilities
            {
                SupportedReferentialActions = new HashSet<ReferentialAction> { ReferentialAction.NoAction, (ReferentialAction)55 },
                MaxIdentifierLength = 128,
            },
            Throws.ArgumentException);
    }

    [Test]
    public static void SupportedReferentialActions_GivenSetWithoutNoAction_ThrowsArgumentException()
    {
        Assert.That(
            () => new DatabaseDialectCapabilities
            {
                SupportedReferentialActions = new HashSet<ReferentialAction> { ReferentialAction.Cascade },
                MaxIdentifierLength = 128,
            },
            Throws.ArgumentException);
    }

    [Test]
    public static void SupportedReferentialActions_WhenSourceSetMutated_IsUnchanged()
    {
        var actions = new HashSet<ReferentialAction> { ReferentialAction.NoAction };
        var capabilities = new DatabaseDialectCapabilities
        {
            SupportedReferentialActions = actions,
            MaxIdentifierLength = 128,
        };

        actions.Add(ReferentialAction.Cascade);

        Assert.That(capabilities.SupportedReferentialActions, Has.Count.EqualTo(1));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public static void MaxIdentifierLength_GivenNonPositiveLength_ThrowsArgumentOutOfRangeException(int maxLength)
    {
        Assert.That(
            () => new DatabaseDialectCapabilities
            {
                SupportedReferentialActions = new HashSet<ReferentialAction> { ReferentialAction.NoAction },
                MaxIdentifierLength = maxLength,
            },
            Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public static void Properties_WhenNotGiven_DefaultToNoSupport()
    {
        var capabilities = CreateCapabilities();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(capabilities.SupportsSchemas, Is.False);
            Assert.That(capabilities.SupportsSequences, Is.False);
            Assert.That(capabilities.SupportsSynonyms, Is.False);
            Assert.That(capabilities.SupportsRoutines, Is.False);
            Assert.That(capabilities.SupportsMaterializedViews, Is.False);
            Assert.That(capabilities.SupportsComments, Is.False);
            Assert.That(capabilities.SupportsDeferrableConstraints, Is.False);
            Assert.That(capabilities.SupportsFilteredIndexes, Is.False);
            Assert.That(capabilities.SupportsIncludedIndexColumns, Is.False);
            Assert.That(capabilities.SupportsComputedColumns, Is.False);
            Assert.That(capabilities.SupportsIdentityColumns, Is.False);
            Assert.That(capabilities.FromLessSelectSuffix, OptionIs.None);
        }
    }

    [Test]
    public static void Properties_WhenConstructed_RetainGivenValues()
    {
        var capabilities = new DatabaseDialectCapabilities
        {
            SupportsSchemas = true,
            SupportsSequences = true,
            SupportsSynonyms = true,
            SupportsRoutines = true,
            SupportsMaterializedViews = true,
            SupportsComments = true,
            SupportsDeferrableConstraints = true,
            SupportsFilteredIndexes = true,
            SupportsIncludedIndexColumns = true,
            SupportsComputedColumns = true,
            SupportsIdentityColumns = true,
            SupportedReferentialActions = new HashSet<ReferentialAction> { ReferentialAction.NoAction, ReferentialAction.Cascade },
            FromLessSelectSuffix = "DUAL",
            MaxIdentifierLength = 30,
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(capabilities.SupportsSchemas, Is.True);
            Assert.That(capabilities.SupportsSequences, Is.True);
            Assert.That(capabilities.SupportsSynonyms, Is.True);
            Assert.That(capabilities.SupportsRoutines, Is.True);
            Assert.That(capabilities.SupportsMaterializedViews, Is.True);
            Assert.That(capabilities.SupportsComments, Is.True);
            Assert.That(capabilities.SupportsDeferrableConstraints, Is.True);
            Assert.That(capabilities.SupportsFilteredIndexes, Is.True);
            Assert.That(capabilities.SupportsIncludedIndexColumns, Is.True);
            Assert.That(capabilities.SupportsComputedColumns, Is.True);
            Assert.That(capabilities.SupportsIdentityColumns, Is.True);
            Assert.That(capabilities.SupportedReferentialActions, Is.EquivalentTo(new[] { ReferentialAction.NoAction, ReferentialAction.Cascade }));
            Assert.That(capabilities.FromLessSelectSuffix.UnwrapSome(), Is.EqualTo("DUAL"));
            Assert.That(capabilities.MaxIdentifierLength, Is.EqualTo(30));
        }
    }
}
