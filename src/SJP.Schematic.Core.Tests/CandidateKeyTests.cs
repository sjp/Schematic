using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

internal static class CandidateKeyTests
{
    [Test]
    public static void Members_GivenPrimaryKey_ReturnPrimaryKeyMembers()
    {
        Identifier keyName = "test_pk";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };
        var backingIndex = Mock.Of<IDatabaseIndex>();
        var primaryKey = new DatabaseKey.Primary(keyName, columns, false, Option<IDatabaseIndex>.Some(backingIndex), false, ConstraintDeferrability.DeferrableInitiallyDeferred);

        CandidateKey key = primaryKey;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(key.Value, Is.SameAs(primaryKey));
            Assert.That(key.Name.UnwrapSome(), Is.EqualTo(keyName));
            Assert.That(key.Columns, Is.EqualTo(columns));
            Assert.That(key.BackingIndex.UnwrapSome(), Is.SameAs(backingIndex));
            Assert.That(key.IsEnabled, Is.False);
            Assert.That(key.IsValidated, Is.False);
            Assert.That(key.Deferrability, Is.EqualTo(ConstraintDeferrability.DeferrableInitiallyDeferred));
            Assert.That(key.ToString(), Is.EqualTo("Primary Key: test_pk"));
        }
    }

    [Test]
    public static void Members_GivenUniqueKey_ReturnUniqueKeyMembers()
    {
        Identifier keyName = "test_uk";
        var columns = new[] { Mock.Of<IDatabaseColumn>() };
        var uniqueKey = new DatabaseKey.Unique(keyName, columns, true);

        CandidateKey key = uniqueKey;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(key.Value, Is.SameAs(uniqueKey));
            Assert.That(key.Name.UnwrapSome(), Is.EqualTo(keyName));
            Assert.That(key.Columns, Is.EqualTo(columns));
            Assert.That(key.BackingIndex, OptionIs.None);
            Assert.That(key.IsEnabled, Is.True);
            Assert.That(key.ToString(), Is.EqualTo("Unique Key: test_uk"));
        }
    }

    [Test]
    public static void ImplicitConversionToDatabaseKey_GivenPrimaryKey_HoldsSameKey()
    {
        var primaryKey = TestKeys.Primary();
        CandidateKey candidateKey = primaryKey;

        DatabaseKey key = candidateKey;

        Assert.That(key.Value, Is.SameAs(primaryKey));
    }

    [Test]
    public static void ToDatabaseKey_GivenUniqueKey_HoldsSameKey()
    {
        var uniqueKey = TestKeys.Unique();
        CandidateKey candidateKey = uniqueKey;

        var key = candidateKey.ToDatabaseKey();

        Assert.That(key.Value, Is.SameAs(uniqueKey));
    }
}
