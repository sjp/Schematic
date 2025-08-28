﻿using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class DatabaseRelationalKeyTests
{
    [Test]
    public static void Ctor_GivenNullChildTable_ThrowsArgumentNullException()
    {
        var childKey = Mock.Of<IDatabaseKey>();
        const string parentTableName = "parent_table";
        var parentKey = Mock.Of<IDatabaseKey>();
        const ReferentialAction deleteAction = ReferentialAction.NoAction;
        const ReferentialAction updateAction = ReferentialAction.NoAction;

        Assert.That(() => new DatabaseRelationalKey(null, childKey, parentTableName, parentKey, deleteAction, updateAction), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullChildKey_ThrowsArgumentNullException()
    {
        const string childTableName = "child_table";
        const string parentTableName = "parent_table";
        var parentKey = Mock.Of<IDatabaseKey>();
        const ReferentialAction deleteAction = ReferentialAction.NoAction;
        const ReferentialAction updateAction = ReferentialAction.NoAction;

        Assert.That(() => new DatabaseRelationalKey(childTableName, null, parentTableName, parentKey, deleteAction, updateAction), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullParentTable_ThrowsArgumentNullException()
    {
        const string childTableName = "child_table";
        var childKey = Mock.Of<IDatabaseKey>();
        var parentKey = Mock.Of<IDatabaseKey>();
        const ReferentialAction deleteAction = ReferentialAction.NoAction;
        const ReferentialAction updateAction = ReferentialAction.NoAction;

        Assert.That(() => new DatabaseRelationalKey(childTableName, childKey, null, parentKey, deleteAction, updateAction), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenNullParentKey_ThrowsArgumentNullException()
    {
        const string childTableName = "child_table";
        var childKey = Mock.Of<IDatabaseKey>();
        const string parentTableName = "parent_table";
        const ReferentialAction deleteAction = ReferentialAction.NoAction;
        const ReferentialAction updateAction = ReferentialAction.NoAction;

        Assert.That(() => new DatabaseRelationalKey(childTableName, childKey, parentTableName, null, deleteAction, updateAction), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenInvalidDeleteAction_ThrowsArgumentException()
    {
        const string childTableName = "child_table";
        var childKey = Mock.Of<IDatabaseKey>();
        const string parentTableName = "parent_table";
        var parentKey = Mock.Of<IDatabaseKey>();
        const ReferentialAction deleteAction = (ReferentialAction)55;
        const ReferentialAction updateAction = ReferentialAction.NoAction;

        Assert.That(() => new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenInvalidUpdateAction_ThrowsArgumentException()
    {
        const string childTableName = "child_table";
        var childKey = Mock.Of<IDatabaseKey>();
        const string parentTableName = "parent_table";
        var parentKey = Mock.Of<IDatabaseKey>();
        const ReferentialAction deleteAction = ReferentialAction.NoAction;
        const ReferentialAction updateAction = (ReferentialAction)55;

        Assert.That(() => new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction), Throws.ArgumentException);
    }

    [Test]
    public static void ChildTable_PropertyGet_EqualsCtorArg()
    {
        const string childTableName = "child_table";
        const string parentTableName = "parent_table";
        const ReferentialAction deleteAction = ReferentialAction.NoAction;
        const ReferentialAction updateAction = ReferentialAction.NoAction;

        var childKeyMock = new Mock<IDatabaseKey>(MockBehavior.Strict);
        childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
        var childKey = childKeyMock.Object;

        var parentKeyMock = new Mock<IDatabaseKey>(MockBehavior.Strict);
        parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
        var parentKey = parentKeyMock.Object;

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

        var childKeyMock = new Mock<IDatabaseKey>(MockBehavior.Strict);
        childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
        childKeyMock.Setup(k => k.Name).Returns(keyName);
        var childKey = childKeyMock.Object;

        var parentKeyMock = new Mock<IDatabaseKey>(MockBehavior.Strict);
        parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
        var parentKey = parentKeyMock.Object;

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

        var childKeyMock = new Mock<IDatabaseKey>(MockBehavior.Strict);
        childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
        var childKey = childKeyMock.Object;

        var parentKeyMock = new Mock<IDatabaseKey>(MockBehavior.Strict);
        parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
        var parentKey = parentKeyMock.Object;

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

        var childKeyMock = new Mock<IDatabaseKey>(MockBehavior.Strict);
        childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
        var childKey = childKeyMock.Object;

        var parentKey = new Mock<IDatabaseKey>(MockBehavior.Strict);
        parentKey.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
        parentKey.Setup(t => t.Name).Returns(keyName);
        var parentKeyArg = parentKey.Object;

        var relationalKey = new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKeyArg, deleteAction, updateAction);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(relationalKey.ParentKey.Name.UnwrapSome(), Is.EqualTo(keyName));
            Assert.That(relationalKey.ParentKey, Is.EqualTo(parentKeyArg));
        }
    }

    [Test]
    public static void DeleteAction_PropertyGet_EqualsCtorArg()
    {
        const string childTableName = "child_table";
        const string parentTableName = "parent_table";
        const ReferentialAction deleteAction = ReferentialAction.Cascade;
        const ReferentialAction updateAction = ReferentialAction.SetDefault;

        var childKeyMock = new Mock<IDatabaseKey>(MockBehavior.Strict);
        childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
        var childKey = childKeyMock.Object;

        var parentKeyMock = new Mock<IDatabaseKey>(MockBehavior.Strict);
        parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
        var parentKey = parentKeyMock.Object;

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

        var childKeyMock = new Mock<IDatabaseKey>(MockBehavior.Strict);
        childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
        var childKey = childKeyMock.Object;

        var parentKeyMock = new Mock<IDatabaseKey>(MockBehavior.Strict);
        parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
        var parentKey = parentKeyMock.Object;

        var relationalKey = new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction);

        Assert.That(relationalKey.UpdateAction, Is.EqualTo(updateAction));
    }

    [Test]
    public static void Ctor_GivenChildKeyNotForeignKey_ThrowsArgumentException()
    {
        const string childTableName = "child_table";
        const string parentTableName = "parent_table";

        var childKeyMock = new Mock<IDatabaseKey>(MockBehavior.Strict);
        childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
        var parentKeyMock = new Mock<IDatabaseKey>(MockBehavior.Strict);
        parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
        const ReferentialAction deleteAction = ReferentialAction.NoAction;
        const ReferentialAction updateAction = ReferentialAction.NoAction;

        Assert.That(() => new DatabaseRelationalKey(childTableName, childKeyMock.Object, parentTableName, parentKeyMock.Object, deleteAction, updateAction), Throws.ArgumentException);
    }

    [Test]
    public static void Ctor_GivenParentKeyNotCandidateKey_ThrowsArgumentException()
    {
        const string childTableName = "child_table";
        const string parentTableName = "parent_table";

        var childKeyMock = new Mock<IDatabaseKey>(MockBehavior.Strict);
        childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
        var parentKeyMock = new Mock<IDatabaseKey>(MockBehavior.Strict);
        parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
        const ReferentialAction deleteAction = ReferentialAction.NoAction;
        const ReferentialAction updateAction = ReferentialAction.NoAction;

        Assert.That(() => new DatabaseRelationalKey(childTableName, childKeyMock.Object, parentTableName, parentKeyMock.Object, deleteAction, updateAction), Throws.ArgumentException);
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

        var childKeyMock = new Mock<IDatabaseKey>(MockBehavior.Strict);
        childKeyMock.Setup(k => k.Name).Returns(childKeyIdentifier);
        childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);

        var parentKeyMock = new Mock<IDatabaseKey>(MockBehavior.Strict);
        parentKeyMock.Setup(k => k.Name).Returns(parentKeyIdentifier);
        parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);

        const ReferentialAction deleteAction = ReferentialAction.NoAction;
        const ReferentialAction updateAction = ReferentialAction.NoAction;

        var key = new DatabaseRelationalKey(childTableName, childKeyMock.Object, parentTableName, parentKeyMock.Object, deleteAction, updateAction);
        var result = key.ToString();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
}