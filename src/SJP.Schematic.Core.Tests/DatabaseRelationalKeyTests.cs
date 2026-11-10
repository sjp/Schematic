using System;
using System.Collections.Generic;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

internal static class DatabaseRelationalKeyTests
{
    [Test]
    public static void Ctor_GivenNullChildTable_ThrowsArgumentNullException()
    {
        var childKey = TestKeys.Foreign();
        const string parentTableName = "parent_table";
        var parentKey = TestKeys.Primary();
        const ReferentialAction deleteAction = ReferentialAction.NoAction;
        const ReferentialAction updateAction = ReferentialAction.NoAction;

        Assert.That(
            () => new DatabaseRelationalKey(null, childKey, parentTableName, parentKey, deleteAction, updateAction),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("childTableName")
        );
    }

    [Test]
    public static void Ctor_GivenNullChildKey_ThrowsArgumentNullException()
    {
        const string childTableName = "child_table";
        const string parentTableName = "parent_table";
        var parentKey = TestKeys.Primary();
        const ReferentialAction deleteAction = ReferentialAction.NoAction;
        const ReferentialAction updateAction = ReferentialAction.NoAction;

        Assert.That(
            () => new DatabaseRelationalKey(childTableName, null, parentTableName, parentKey, deleteAction, updateAction),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("childKey")
        );
    }

    [Test]
    public static void Ctor_GivenNullParentTable_ThrowsArgumentNullException()
    {
        const string childTableName = "child_table";
        var childKey = TestKeys.Foreign();
        var parentKey = TestKeys.Primary();
        const ReferentialAction deleteAction = ReferentialAction.NoAction;
        const ReferentialAction updateAction = ReferentialAction.NoAction;

        Assert.That(
            () => new DatabaseRelationalKey(childTableName, childKey, null, parentKey, deleteAction, updateAction),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("parentTableName")
        );
    }

    [Test]
    public static void Ctor_GivenNullParentKey_ThrowsArgumentNullException()
    {
        const string childTableName = "child_table";
        var childKey = TestKeys.Foreign();
        const string parentTableName = "parent_table";
        const ReferentialAction deleteAction = ReferentialAction.NoAction;
        const ReferentialAction updateAction = ReferentialAction.NoAction;

        Assert.That(
            () => new DatabaseRelationalKey(childTableName, childKey, parentTableName, null, deleteAction, updateAction),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("parentKey")
        );
    }

    [Test]
    public static void Ctor_GivenInvalidDeleteAction_ThrowsArgumentException()
    {
        const string childTableName = "child_table";
        var childKey = TestKeys.Foreign();
        const string parentTableName = "parent_table";
        var parentKey = TestKeys.Primary();
        const ReferentialAction deleteAction = (ReferentialAction)55;
        const ReferentialAction updateAction = ReferentialAction.NoAction;

        Assert.That(
            () => new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("deleteAction")
        );
    }

    [Test]
    public static void Ctor_GivenInvalidUpdateAction_ThrowsArgumentException()
    {
        const string childTableName = "child_table";
        var childKey = TestKeys.Foreign();
        const string parentTableName = "parent_table";
        var parentKey = TestKeys.Primary();
        const ReferentialAction deleteAction = ReferentialAction.NoAction;
        const ReferentialAction updateAction = (ReferentialAction)55;

        Assert.That(
            () => new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("updateAction")
        );
    }

    [Test]
    public static void Ctor_GivenInvalidMatchType_ThrowsArgumentException()
    {
        const string childTableName = "child_table";
        var childKey = TestKeys.Foreign();
        const string parentTableName = "parent_table";
        var parentKey = TestKeys.Primary();
        const ForeignKeyMatchType matchType = (ForeignKeyMatchType)55;

        Assert.That(
            () => new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, ReferentialAction.NoAction, ReferentialAction.NoAction, matchType, []),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("matchType")
        );
    }

    [Test]
    public static void Ctor_GivenNullSetNullColumns_ThrowsArgumentNullException()
    {
        Assert.That(
            () => CreateRelationalKey(ForeignKeyMatchType.Simple, null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("setNullColumns")
        );
    }

    [Test]
    public static void MatchType_WhenNotProvidedInCtor_ReturnsSimple()
    {
        var relationalKey = CreateRelationalKeyWithDefaults();

        Assert.That(relationalKey.MatchType, Is.EqualTo(ForeignKeyMatchType.Simple));
    }

    [Test]
    public static void SetNullColumns_WhenNotProvidedInCtor_ReturnsEmptyCollection()
    {
        var relationalKey = CreateRelationalKeyWithDefaults();

        Assert.That(relationalKey.SetNullColumns, Is.Empty);
    }

    [Test]
    public static void MatchTypeAndSetNullColumns_WhenProvidedInCtor_ReturnGivenValues()
    {
        var setNullColumn = Mock.Of<IDatabaseColumn>();
        var relationalKey = CreateRelationalKey(ForeignKeyMatchType.Full, [setNullColumn]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(relationalKey.MatchType, Is.EqualTo(ForeignKeyMatchType.Full));
            Assert.That(relationalKey.SetNullColumns, Is.EquivalentTo(new[] { setNullColumn }));
        }
    }

    private static DatabaseRelationalKey CreateRelationalKey(ForeignKeyMatchType matchType, IReadOnlyCollection<IDatabaseColumn> setNullColumns)
    {
        const string childTableName = "child_table";
        const string parentTableName = "parent_table";

        var childKey = TestKeys.Foreign();
        var parentKey = TestKeys.Primary();

        return new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, ReferentialAction.NoAction, ReferentialAction.NoAction, matchType, setNullColumns);
    }

    private static DatabaseRelationalKey CreateRelationalKeyWithDefaults()
    {
        const string childTableName = "child_table";
        const string parentTableName = "parent_table";

        var childKey = TestKeys.Foreign();
        var parentKey = TestKeys.Primary();

        return new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, ReferentialAction.NoAction, ReferentialAction.NoAction);
    }

    [Test]
    public static void ChildTable_PropertyGet_EqualsCtorArg()
    {
        const string childTableName = "child_table";
        const string parentTableName = "parent_table";
        const ReferentialAction deleteAction = ReferentialAction.NoAction;
        const ReferentialAction updateAction = ReferentialAction.NoAction;

        var childKey = TestKeys.Foreign();
        var parentKey = TestKeys.Primary();

        var relationalKey = new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction);

        Assert.That(relationalKey.ChildTable, Is.EqualTo(new Identifier(childTableName)));
    }

    [Test]
    public static void ChildKey_PropertyGet_EqualsCtorArg()
    {
        const string childTableName = "child_table";
        const string parentTableName = "parent_table";
        const ReferentialAction deleteAction = ReferentialAction.Cascade;
        const ReferentialAction updateAction = ReferentialAction.SetDefault;
        Identifier keyName = "test_child_key";

        var childKey = TestKeys.Foreign(keyName);
        var parentKey = TestKeys.Primary();

        var relationalKey = new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(relationalKey.ChildKey.Name.UnwrapSome(), Is.EqualTo(keyName));
            Assert.That(relationalKey.ChildKey, Is.EqualTo(childKey));
        }
    }

    [Test]
    public static void ParentTable_PropertyGet_EqualsCtorArg()
    {
        const string childTableName = "child_table";
        const string parentTableName = "parent_table";
        const ReferentialAction deleteAction = ReferentialAction.NoAction;
        const ReferentialAction updateAction = ReferentialAction.NoAction;

        var childKey = TestKeys.Foreign();
        var parentKey = TestKeys.Primary();

        var relationalKey = new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction);

        Assert.That(relationalKey.ParentTable, Is.EqualTo(new Identifier(parentTableName)));
    }

    [Test]
    public static void ParentKey_PropertyGet_EqualsCtorArg()
    {
        const string childTableName = "child_table";
        const string parentTableName = "parent_table";
        const ReferentialAction deleteAction = ReferentialAction.Cascade;
        const ReferentialAction updateAction = ReferentialAction.SetDefault;
        Identifier keyName = "test_parent_key";

        var childKey = TestKeys.Foreign();
        var parentKey = TestKeys.Primary(keyName);

        var relationalKey = new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(relationalKey.ParentKey.Name.UnwrapSome(), Is.EqualTo(keyName));
            Assert.That(relationalKey.ParentKey, Is.EqualTo(parentKey));
        }
    }

    [Test]
    public static void DeleteAction_PropertyGet_EqualsCtorArg()
    {
        const string childTableName = "child_table";
        const string parentTableName = "parent_table";
        const ReferentialAction deleteAction = ReferentialAction.Cascade;
        const ReferentialAction updateAction = ReferentialAction.SetDefault;

        var childKey = TestKeys.Foreign();
        var parentKey = TestKeys.Primary();

        var relationalKey = new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction);

        Assert.That(relationalKey.DeleteAction, Is.EqualTo(deleteAction));
    }

    [Test]
    public static void UpdateAction_PropertyGet_EqualsCtorArg()
    {
        const string childTableName = "child_table";
        const string parentTableName = "parent_table";
        const ReferentialAction deleteAction = ReferentialAction.Cascade;
        const ReferentialAction updateAction = ReferentialAction.SetDefault;

        var childKey = TestKeys.Foreign();
        var parentKey = TestKeys.Primary();

        var relationalKey = new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction);

        Assert.That(relationalKey.UpdateAction, Is.EqualTo(updateAction));
    }

    [Test]
    public static void Ctor_GivenChildKeyNotForeignKey_ThrowsArgumentException()
    {
        const string childTableName = "child_table";
        const string parentTableName = "parent_table";

        var childKey = TestKeys.Primary();
        var parentKey = TestKeys.Primary();
        const ReferentialAction deleteAction = ReferentialAction.NoAction;
        const ReferentialAction updateAction = ReferentialAction.NoAction;

        Assert.That(
            () => new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("childKey")
        );
    }

    [Test]
    public static void Ctor_GivenParentKeyNotCandidateKey_ThrowsArgumentException()
    {
        const string childTableName = "child_table";
        const string parentTableName = "parent_table";

        var childKey = TestKeys.Foreign();
        var parentKey = TestKeys.Foreign();
        const ReferentialAction deleteAction = ReferentialAction.NoAction;
        const ReferentialAction updateAction = ReferentialAction.NoAction;

        Assert.That(
            () => new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("parentKey")
        );
    }

    [Test]
    public static void Ctor_GivenKeysWithDifferentColumnCounts_ThrowsArgumentException()
    {
        const string childTableName = "child_table";
        const string parentTableName = "parent_table";
        Identifier childKeyName = "test_child_key";
        Identifier parentKeyName = "test_parent_key";

        var childKey = TestKeys.Foreign(childKeyName, [Mock.Of<IDatabaseColumn>(), Mock.Of<IDatabaseColumn>()]);
        var parentKey = TestKeys.Primary(parentKeyName, [Mock.Of<IDatabaseColumn>()]);

        const ReferentialAction deleteAction = ReferentialAction.NoAction;
        const ReferentialAction updateAction = ReferentialAction.NoAction;

        Assert.That(
            () => new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("childKey")
        );
    }

    [TestCase(null, "test_table_1", null, null, "test_table_2", null, "Relational Key: test_table_1 -> test_table_2")]
    [TestCase("child_schema", "test_table_1", null, null, "test_table_2", null, "Relational Key: child_schema.test_table_1 -> test_table_2")]
    [TestCase(null, "test_table_1", null, "parent_schema", "test_table_2", null, "Relational Key: test_table_1 -> parent_schema.test_table_2")]
    [TestCase("child_schema", "test_table_1", null, "parent_schema", "test_table_2", null, "Relational Key: child_schema.test_table_1 -> parent_schema.test_table_2")]
    [TestCase(null, "test_table_1", null, null, "test_table_2", "pk_parent_1", "Relational Key: test_table_1 -> test_table_2 (pk_parent_1)")]
    [TestCase(null, "test_table_1", "fk_child_1", null, "test_table_2", null, "Relational Key: test_table_1 (fk_child_1) -> test_table_2")]
    [TestCase(null, "test_table_1", "fk_child_1", null, "test_table_2", "pk_parent_1", "Relational Key: test_table_1 (fk_child_1) -> test_table_2 (pk_parent_1)")]
    [TestCase("child_schema", "test_table_1", "fk_child_1", null, "test_table_2", "pk_parent_1", "Relational Key: child_schema.test_table_1 (fk_child_1) -> test_table_2 (pk_parent_1)")]
    [TestCase(null, "test_table_1", "fk_child_1", "parent_schema", "test_table_2", "pk_parent_1", "Relational Key: test_table_1 (fk_child_1) -> parent_schema.test_table_2 (pk_parent_1)")]
    [TestCase("child_schema", "test_table_1", "fk_child_1", "parent_schema", "test_table_2", "pk_parent_1", "Relational Key: child_schema.test_table_1 (fk_child_1) -> parent_schema.test_table_2 (pk_parent_1)")]
    public static void ToString_WhenInvoked_ReturnsExpectedValues(
        string childTableSchema,
        string childTableLocal,
        string childKeyName,
        string parentTableSchema,
        string parentTableLocal,
        string parentKeyName,
        string expectedResult
    )
    {
        var childTableName = Identifier.CreateQualifiedIdentifier(childTableSchema, childTableLocal);
        var childKeyIdentifier = !childKeyName.IsNullOrWhiteSpace()
            ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(childKeyName))
            : Option<Identifier>.None;

        var parentTableName = Identifier.CreateQualifiedIdentifier(parentTableSchema, parentTableLocal);
        var parentKeyIdentifier = !parentKeyName.IsNullOrWhiteSpace()
            ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(parentKeyName))
            : Option<Identifier>.None;

        var childKey = TestKeys.Foreign(childKeyIdentifier);
        var parentKey = TestKeys.Primary(parentKeyIdentifier);

        const ReferentialAction deleteAction = ReferentialAction.NoAction;
        const ReferentialAction updateAction = ReferentialAction.NoAction;

        var key = new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction);
        var result = key.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}