using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests
{
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

            Assert.Throws<ArgumentNullException>(() => new DatabaseRelationalKey(null, childKey, parentTableName, parentKey, deleteAction, updateAction));
        }

        [Test]
        public static void Ctor_GivenNullChildKey_ThrowsArgumentNullException()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";
            var parentKey = Mock.Of<IDatabaseKey>();
            const ReferentialAction deleteAction = ReferentialAction.NoAction;
            const ReferentialAction updateAction = ReferentialAction.NoAction;

            Assert.Throws<ArgumentNullException>(() => new DatabaseRelationalKey(childTableName, null, parentTableName, parentKey, deleteAction, updateAction));
        }

        [Test]
        public static void Ctor_GivenNullParentTable_ThrowsArgumentNullException()
        {
            const string childTableName = "child_table";
            var childKey = Mock.Of<IDatabaseKey>();
            var parentKey = Mock.Of<IDatabaseKey>();
            const ReferentialAction deleteAction = ReferentialAction.NoAction;
            const ReferentialAction updateAction = ReferentialAction.NoAction;

            Assert.Throws<ArgumentNullException>(() => new DatabaseRelationalKey(childTableName, childKey, null, parentKey, deleteAction, updateAction));
        }

        [Test]
        public static void Ctor_GivenNullParentKey_ThrowsArgumentNullException()
        {
            const string childTableName = "child_table";
            var childKey = Mock.Of<IDatabaseKey>();
            const string parentTableName = "parent_table";
            const ReferentialAction deleteAction = ReferentialAction.NoAction;
            const ReferentialAction updateAction = ReferentialAction.NoAction;

            Assert.Throws<ArgumentNullException>(() => new DatabaseRelationalKey(childTableName, childKey, parentTableName, null, deleteAction, updateAction));
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

            Assert.Throws<ArgumentException>(() => new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction));
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

            Assert.Throws<ArgumentException>(() => new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction));
        }

        [Test]
        public static void ChildTable_PropertyGet_EqualsCtorArg()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";
            const ReferentialAction deleteAction = ReferentialAction.NoAction;
            const ReferentialAction updateAction = ReferentialAction.NoAction;

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction);

            Assert.AreEqual(new Identifier(childTableName), relationalKey.ChildTable);
        }

        [Test]
        public static void ChildKey_PropertyGet_EqualsCtorArg()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";
            const ReferentialAction deleteAction = ReferentialAction.Cascade;
            const ReferentialAction updateAction = ReferentialAction.SetDefault;
            Identifier keyName = "test_child_key";

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            childKeyMock.Setup(k => k.Name).Returns(keyName);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(keyName, relationalKey.ChildKey.Name.UnwrapSome());
                Assert.AreSame(childKey, relationalKey.ChildKey);
            });
        }

        [Test]
        public static void ParentTable_PropertyGet_EqualsCtorArg()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";
            const ReferentialAction deleteAction = ReferentialAction.NoAction;
            const ReferentialAction updateAction = ReferentialAction.NoAction;

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction);

            Assert.AreEqual(new Identifier(parentTableName), relationalKey.ParentTable);
        }

        [Test]
        public static void ParentKey_PropertyGet_EqualsCtorArg()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";
            const ReferentialAction deleteAction = ReferentialAction.Cascade;
            const ReferentialAction updateAction = ReferentialAction.SetDefault;
            Identifier keyName = "test_parent_key";

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var childKey = childKeyMock.Object;

            var parentKey = new Mock<IDatabaseKey>();
            parentKey.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            parentKey.Setup(t => t.Name).Returns(keyName);
            var parentKeyArg = parentKey.Object;

            var relationalKey = new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKeyArg, deleteAction, updateAction);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(keyName, relationalKey.ParentKey.Name.UnwrapSome());
                Assert.AreSame(parentKeyArg, relationalKey.ParentKey);
            });
        }

        [Test]
        public static void DeleteAction_PropertyGet_EqualsCtorArg()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";
            const ReferentialAction deleteAction = ReferentialAction.Cascade;
            const ReferentialAction updateAction = ReferentialAction.SetDefault;

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction);

            Assert.AreEqual(deleteAction, relationalKey.DeleteAction);
        }

        [Test]
        public static void UpdateAction_PropertyGet_EqualsCtorArg()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";
            const ReferentialAction deleteAction = ReferentialAction.Cascade;
            const ReferentialAction updateAction = ReferentialAction.SetDefault;

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new DatabaseRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction, updateAction);

            Assert.AreEqual(updateAction, relationalKey.UpdateAction);
        }

        [Test]
        public static void Ctor_GivenChildKeyNotForeignKey_ThrowsArgumentException()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            const ReferentialAction deleteAction = ReferentialAction.NoAction;
            const ReferentialAction updateAction = ReferentialAction.NoAction;

            Assert.Throws<ArgumentException>(() => new DatabaseRelationalKey(childTableName, childKeyMock.Object, parentTableName, parentKeyMock.Object, deleteAction, updateAction));
        }

        [Test]
        public static void Ctor_GivenParentKeyNotCandidateKey_ThrowsArgumentException()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            const ReferentialAction deleteAction = ReferentialAction.NoAction;
            const ReferentialAction updateAction = ReferentialAction.NoAction;

            Assert.Throws<ArgumentException>(() => new DatabaseRelationalKey(childTableName, childKeyMock.Object, parentTableName, parentKeyMock.Object, deleteAction, updateAction));
        }
    }
}
