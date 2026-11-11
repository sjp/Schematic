using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

internal static class DatabaseObjectTests
{
    private static readonly Identifier ObjectName = Identifier.CreateQualifiedIdentifier("app", "test_object");

    private static IEnumerable<TestCaseData> EveryCase()
    {
        yield return new TestCaseData((DatabaseObject)Mock.Of<IRelationalDatabaseTable>(t => t.Name == ObjectName), "table").SetArgDisplayNames("Table");
        yield return new TestCaseData((DatabaseObject)Mock.Of<IDatabaseView>(v => v.Name == ObjectName), "view").SetArgDisplayNames("View");
        yield return new TestCaseData((DatabaseObject)Mock.Of<IDatabaseMaterializedView>(v => v.Name == ObjectName), "view").SetArgDisplayNames("MaterializedView");
        yield return new TestCaseData((DatabaseObject)Mock.Of<IDatabaseSequence>(s => s.Name == ObjectName), "sequence").SetArgDisplayNames("Sequence");
        yield return new TestCaseData((DatabaseObject)new DatabaseSynonym(ObjectName, "target"), "synonym").SetArgDisplayNames("Synonym");
        yield return new TestCaseData((DatabaseObject)Mock.Of<IDatabaseRoutine>(r => r.Name == ObjectName), "routine").SetArgDisplayNames("Routine");
        yield return new TestCaseData((DatabaseObject)(DatabaseUserDefinedType)new DatabaseUserDefinedType.Unknown(ObjectName), "user-defined type").SetArgDisplayNames("UserDefinedType");
    }

    private static string GetKind(DatabaseObject databaseObject) => databaseObject switch
    {
        IRelationalDatabaseTable => "table",
        IDatabaseView => "view",
        IDatabaseSequence => "sequence",
        IDatabaseSynonym => "synonym",
        IDatabaseRoutine => "routine",
        DatabaseUserDefinedType => "user-defined type",
    };

    [TestCaseSource(nameof(EveryCase))]
    public static void Switch_GivenObject_MatchesItsKind(DatabaseObject databaseObject, string expectedKind)
    {
        Assert.That(GetKind(databaseObject), Is.EqualTo(expectedKind));
    }

    [TestCaseSource(nameof(EveryCase))]
    public static void Name_WhenReadThroughUnion_EqualsObjectName(DatabaseObject databaseObject, string expectedKind)
    {
        _ = expectedKind;

        Assert.That(databaseObject.Name, Is.EqualTo(ObjectName));
    }

    [TestCaseSource(nameof(EveryCase))]
    public static void Name_WhenReadThroughDatabaseEntity_EqualsObjectName(DatabaseObject databaseObject, string expectedKind)
    {
        _ = expectedKind;
        IDatabaseEntity entity = databaseObject;

        Assert.That(entity.Name, Is.EqualTo(ObjectName));
    }

    [Test]
    public static void Value_GivenTable_IsSameInstance()
    {
        var table = Mock.Of<IRelationalDatabaseTable>(t => t.Name == ObjectName);

        DatabaseObject databaseObject = new(table);

        Assert.That(databaseObject.Value, Is.SameAs(table));
    }

    [Test]
    public static void Value_GivenUserDefinedType_HoldsSameType()
    {
        var unknownType = new DatabaseUserDefinedType.Unknown(ObjectName);
        DatabaseUserDefinedType userDefinedType = unknownType;

        DatabaseObject databaseObject = userDefinedType;

        Assert.That(databaseObject.Value, Is.TypeOf<DatabaseUserDefinedType>());
        Assert.That(((DatabaseUserDefinedType)databaseObject.Value!).Value, Is.SameAs(unknownType));
    }

    [Test]
    public static void ToString_GivenSynonym_ReturnsSynonymString()
    {
        var synonym = new DatabaseSynonym(ObjectName, "target");

        DatabaseObject databaseObject = synonym;

        Assert.That(databaseObject.ToString(), Is.EqualTo(synonym.ToString()));
    }

    [Test]
    public static void ToString_GivenDefault_ReturnsEmptyString()
    {
        Assert.That(default(DatabaseObject).ToString(), Is.Empty);
    }

    [Test]
    public static void Name_GivenDefault_Throws()
    {
        Assert.That(() => default(DatabaseObject).Name, Throws.Exception);
    }
}
