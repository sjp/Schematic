using System;
using System.Collections.Generic;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

internal static class DatabaseKeyTests
{
    internal enum KeyKind
    {
        Primary,
        Unique,
        Foreign,
    }

    private static DatabaseKey CreateKey(KeyKind kind, Option<Identifier> name, IReadOnlyCollection<IDatabaseColumn> columns, bool isEnabled) => kind switch
    {
        KeyKind.Primary => new DatabaseKey.Primary(name, columns, isEnabled),
        KeyKind.Unique => new DatabaseKey.Unique(name, columns, isEnabled),
        KeyKind.Foreign => new DatabaseKey.Foreign(name, columns, isEnabled),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    [TestCase(KeyKind.Primary)]
    [TestCase(KeyKind.Unique)]
    [TestCase(KeyKind.Foreign)]
    public static void Ctor_GivenNoName_DoesNotThrow(KeyKind kind)
    {
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        Assert.That(() => CreateKey(kind, Option<Identifier>.None, columns, true), Throws.Nothing);
    }

    [TestCase(KeyKind.Primary)]
    [TestCase(KeyKind.Unique)]
    [TestCase(KeyKind.Foreign)]
    public static void Ctor_GivenNullColumnSet_ThrowsArgumentNullException(KeyKind kind)
    {
        Identifier keyName = "test_key";

        Assert.That(
            () => CreateKey(kind, keyName, null, true),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columns")
        );
    }

    [TestCase(KeyKind.Primary)]
    [TestCase(KeyKind.Unique)]
    [TestCase(KeyKind.Foreign)]
    public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentException(KeyKind kind)
    {
        Identifier keyName = "test_key";
        var columns = Array.Empty<IDatabaseColumn>();

        Assert.That(
            () => CreateKey(kind, keyName, columns, true),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columns")
        );
    }

    [TestCase(KeyKind.Primary)]
    [TestCase(KeyKind.Unique)]
    [TestCase(KeyKind.Foreign)]
    public static void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException(KeyKind kind)
    {
        Identifier keyName = "test_key";
        var columns = new IDatabaseColumn[] { null };

        Assert.That(
            () => CreateKey(kind, keyName, columns, true),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("columns")
        );
    }

    [TestCase(KeyKind.Primary)]
    [TestCase(KeyKind.Unique)]
    [TestCase(KeyKind.Foreign)]
    public static void Name_PropertyGet_EqualsCtorArg(KeyKind kind)
    {
        Identifier keyName = "test_key";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        var key = CreateKey(kind, keyName, columns, true);

        Assert.That(key.Name.UnwrapSome(), Is.EqualTo(keyName));
    }

    [TestCase(KeyKind.Primary)]
    [TestCase(KeyKind.Unique)]
    [TestCase(KeyKind.Foreign)]
    public static void Name_GivenQualifiedCtorArg_PropertyGetReturnsLocalNameOnly(KeyKind kind)
    {
        var keyName = Identifier.CreateQualifiedIdentifier("test_schema", "test_key");
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        var key = CreateKey(kind, keyName, columns, true);

        Assert.That(key.Name.UnwrapSome(), Is.EqualTo(Identifier.CreateQualifiedIdentifier("test_key")));
    }

    [TestCase(KeyKind.Primary, typeof(DatabaseKey.Primary))]
    [TestCase(KeyKind.Unique, typeof(DatabaseKey.Unique))]
    [TestCase(KeyKind.Foreign, typeof(DatabaseKey.Foreign))]
    public static void Value_PropertyGet_IsTheConstructedCase(KeyKind kind, Type expectedType)
    {
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        var key = CreateKey(kind, Option<Identifier>.None, columns, true);

        Assert.That(key.Value, Is.TypeOf(expectedType));
    }

    [TestCase(KeyKind.Primary)]
    [TestCase(KeyKind.Unique)]
    [TestCase(KeyKind.Foreign)]
    public static void Columns_PropertyGet_EqualsCtorArg(KeyKind kind)
    {
        Identifier keyName = "test_key";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        var key = CreateKey(kind, keyName, columns, true);

        Assert.That(key.Columns, Is.EqualTo(columns));
    }

    [TestCase(KeyKind.Primary, true)]
    [TestCase(KeyKind.Primary, false)]
    [TestCase(KeyKind.Unique, true)]
    [TestCase(KeyKind.Unique, false)]
    [TestCase(KeyKind.Foreign, true)]
    [TestCase(KeyKind.Foreign, false)]
    public static void IsEnabled_PropertyGet_EqualsCtorArg(KeyKind kind, bool enabled)
    {
        Identifier keyName = "test_key";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        var key = CreateKey(kind, keyName, columns, enabled);

        Assert.That(key.IsEnabled, Is.EqualTo(enabled));
    }

    [TestCase(KeyKind.Primary)]
    [TestCase(KeyKind.Unique)]
    [TestCase(KeyKind.Foreign)]
    public static void IsValidatedAndDeferrability_WhenNotProvidedInCtor_ReturnDefaults(KeyKind kind)
    {
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        var key = CreateKey(kind, Option<Identifier>.None, columns, true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(key.IsValidated, Is.True);
            Assert.That(key.Deferrability, Is.EqualTo(ConstraintDeferrability.NotDeferrable));
        }
    }

    [TestCase(KeyKind.Foreign, null, "Foreign Key")]
    [TestCase(KeyKind.Foreign, "test_foreign_key", "Foreign Key: test_foreign_key")]
    [TestCase(KeyKind.Primary, null, "Primary Key")]
    [TestCase(KeyKind.Primary, "test_primary_key", "Primary Key: test_primary_key")]
    [TestCase(KeyKind.Unique, null, "Unique Key")]
    [TestCase(KeyKind.Unique, "test_unique_key", "Unique Key: test_unique_key")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(KeyKind kind, string name, string expectedResult)
    {
        var keyName = !name.IsNullOrWhiteSpace()
            ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(name))
            : Option<Identifier>.None;
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        var key = CreateKey(kind, keyName, columns, true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(key.ToString(), Is.EqualTo(expectedResult));
            Assert.That(key.Value!.ToString(), Is.EqualTo(expectedResult));
        }
    }

    [Test]
    public static void Name_WhenDefaultValue_Throws()
    {
        var key = default(DatabaseKey);

        Assert.That(() => key.Name, Throws.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public static void ToString_WhenDefaultValue_ReturnsEmptyString()
    {
        var key = default(DatabaseKey);

        Assert.That(key.ToString(), Is.Empty);
    }

    [Test]
    public static void Ctor_GivenInvalidDeferrabilityForUniqueKey_ThrowsArgumentException()
    {
        Identifier keyName = "test_key";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };
        const ConstraintDeferrability deferrability = (ConstraintDeferrability)55;

        Assert.That(
            () => new DatabaseKey.Unique(keyName, columns, true, Option<IDatabaseIndex>.None, true, deferrability),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("deferrability")
        );
    }

    [Test]
    public static void Ctor_GivenInvalidDeferrabilityForForeignKey_ThrowsArgumentException()
    {
        Identifier keyName = "test_key";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };
        const ConstraintDeferrability deferrability = (ConstraintDeferrability)55;

        Assert.That(
            () => new DatabaseKey.Foreign(keyName, columns, true, true, deferrability),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("deferrability")
        );
    }

    [Test]
    public static void BackingIndex_WhenProvidedToUniqueKeyCtor_ReturnsGivenIndex()
    {
        Identifier keyName = "test_key";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };
        var backingIndex = Mock.Of<IDatabaseIndex>();

        var key = new DatabaseKey.Unique(keyName, columns, true, Option<IDatabaseIndex>.Some(backingIndex));

        Assert.That(key.BackingIndex.UnwrapSome(), Is.SameAs(backingIndex));
    }

    [Test]
    public static void BackingIndex_WhenNotProvidedInCtor_ReturnsNone()
    {
        Identifier keyName = "test_key";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        var key = new DatabaseKey.Primary(keyName, columns, true);

        Assert.That(key.BackingIndex, OptionIs.None);
    }

    [Test]
    public static void BackingIndex_WhenProvidedInCtor_ReturnsGivenIndex()
    {
        Identifier keyName = "test_key";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };
        var backingIndex = Mock.Of<IDatabaseIndex>();

        var key = new DatabaseKey.Primary(keyName, columns, true, Option<IDatabaseIndex>.Some(backingIndex));

        Assert.That(key.BackingIndex.UnwrapSome(), Is.SameAs(backingIndex));
    }

    [Test]
    public static void Ctor_GivenInvalidDeferrability_ThrowsArgumentException()
    {
        Identifier keyName = "test_key";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };
        const ConstraintDeferrability deferrability = (ConstraintDeferrability)55;

        Assert.That(
            () => new DatabaseKey.Primary(keyName, columns, true, Option<IDatabaseIndex>.None, true, deferrability),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("deferrability")
        );
    }

    [Test]
    public static void IsValidated_WhenNotProvidedInCtor_ReturnsTrue()
    {
        Identifier keyName = "test_key";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        var key = new DatabaseKey.Primary(keyName, columns, true);

        Assert.That(key.IsValidated, Is.True);
    }

    [Test]
    public static void IsValidated_WhenProvidedInCtor_ReturnsGivenValue()
    {
        Identifier keyName = "test_key";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        var key = new DatabaseKey.Foreign(keyName, columns, true, false, ConstraintDeferrability.NotDeferrable);

        Assert.That(key.IsValidated, Is.False);
    }

    [Test]
    public static void Deferrability_WhenNotProvidedInCtor_ReturnsNotDeferrable()
    {
        Identifier keyName = "test_key";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        var key = new DatabaseKey.Primary(keyName, columns, true);

        Assert.That(key.Deferrability, Is.EqualTo(ConstraintDeferrability.NotDeferrable));
    }

    [Test]
    public static void Deferrability_WhenProvidedInCtor_ReturnsGivenValue()
    {
        Identifier keyName = "test_key";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };

        var key = new DatabaseKey.Primary(keyName, columns, true, Option<IDatabaseIndex>.None, true, ConstraintDeferrability.DeferrableInitiallyDeferred);

        Assert.That(key.Deferrability, Is.EqualTo(ConstraintDeferrability.DeferrableInitiallyDeferred));
    }

    [Test]
    public static void Columns_WhenSourceCollectionMutatedAfterConstruction_RemainsUnchanged()
    {
        Identifier keyName = "test_key";
        var columns = new List<IDatabaseColumn> { Mock.Of<IDatabaseColumn>() };

        var key = new DatabaseKey.Primary(keyName, columns, true);

        columns.Add(Mock.Of<IDatabaseColumn>());

        Assert.That(key.Columns, Has.Count.EqualTo(1));
    }
}