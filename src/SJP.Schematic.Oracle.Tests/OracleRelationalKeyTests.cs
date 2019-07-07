using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Tests
{
    [TestFixture]
    internal static class OracleRelationalKeyTests
    {
        [Test]
        public static void Ctor_GivenNullChildTable_ThrowsArgumentNullException()
        {
            var childKey = Mock.Of<IDatabaseKey>();
            const string parentTableName = "parent_table";
            var parentKey = Mock.Of<IDatabaseKey>();
            const ReferentialAction deleteAction = ReferentialAction.NoAction;

            Assert.Throws<ArgumentNullException>(() => new OracleRelationalKey(null, childKey, parentTableName, parentKey, deleteAction));
        }

        [Test]
        public static void Ctor_GivenNullChildKey_ThrowsArgumentNullException()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";
            var parentKey = Mock.Of<IDatabaseKey>();
            const ReferentialAction deleteAction = ReferentialAction.NoAction;

            Assert.Throws<ArgumentNullException>(() => new OracleRelationalKey(childTableName, null, parentTableName, parentKey, deleteAction));
        }

        [Test]
        public static void Ctor_GivenNullParentTable_ThrowsArgumentNullException()
        {
            const string childTableName = "child_table";
            var childKey = Mock.Of<IDatabaseKey>();
            var parentKey = Mock.Of<IDatabaseKey>();
            const ReferentialAction deleteAction = ReferentialAction.NoAction;

            Assert.Throws<ArgumentNullException>(() => new OracleRelationalKey(childTableName, childKey, null, parentKey, deleteAction));
        }

        [Test]
        public static void Ctor_GivenNullParentKey_ThrowsArgumentNullException()
        {
            const string childTableName = "child_table";
            var childKey = Mock.Of<IDatabaseKey>();
            const string parentTableName = "parent_table";
            const ReferentialAction deleteAction = ReferentialAction.NoAction;

            Assert.Throws<ArgumentNullException>(() => new OracleRelationalKey(childTableName, childKey, parentTableName, null, deleteAction));
        }

        [Test]
        public static void Ctor_GivenInvalidDeleteAction_ThrowsArgumentException()
        {
            const string childTableName = "child_table";
            var childKey = Mock.Of<IDatabaseKey>();
            const string parentTableName = "parent_table";
            var parentKey = Mock.Of<IDatabaseKey>();
            const ReferentialAction deleteAction = (ReferentialAction)55;

            Assert.Throws<ArgumentException>(() => new OracleRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction));
        }

        [Test]
        public static void ChildTable_PropertyGet_MatchesCtorArg()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";
            const ReferentialAction deleteAction = ReferentialAction.NoAction;

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new OracleRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction);
            Assert.AreEqual(new Identifier(childTableName), relationalKey.ChildTable);
        }

        [Test]
        public static void ChildKey_PropertyGet_EqualsCtorArg()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";

            const ReferentialAction deleteAction = ReferentialAction.Cascade;
            Identifier keyName = "test_child_key";

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            childKeyMock.Setup(k => k.Name).Returns(keyName);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new OracleRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(keyName, relationalKey.ChildKey.Name.UnwrapSome());
                Assert.AreSame(childKey, relationalKey.ChildKey);
            });
        }

        [Test]
        public static void ParentTable_PropertyGet_MatchesCtorArg()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";
            const ReferentialAction deleteAction = ReferentialAction.NoAction;

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new OracleRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction);
            Assert.AreEqual(new Identifier(parentTableName), relationalKey.ParentTable);
        }

        [Test]
        public static void ParentKey_PropertyGet_EqualsCtorArg()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";

            const ReferentialAction deleteAction = ReferentialAction.Cascade;
            Identifier keyName = "test_parent_key";

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            parentKeyMock.Setup(t => t.Name).Returns(keyName);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new OracleRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(keyName, relationalKey.ParentKey.Name.UnwrapSome());
                Assert.AreSame(parentKey, relationalKey.ParentKey);
            });
        }

        [Test]
        public static void DeleteAction_PropertyGet_EqualsCtorArg()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";
            const ReferentialAction deleteAction = ReferentialAction.Cascade;

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new OracleRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction);

            Assert.AreEqual(deleteAction, relationalKey.DeleteAction);
        }

        [Test]
        public static void UpdateAction_PropertyGet_EqualsNone()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";
            const ReferentialAction deleteAction = ReferentialAction.Cascade;

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new OracleRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteAction);

            Assert.AreEqual(ReferentialAction.NoAction, relationalKey.UpdateAction);
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

            Assert.Throws<ArgumentException>(() => new OracleRelationalKey(childTableName, childKeyMock.Object, parentTableName, parentKeyMock.Object, deleteAction));
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

            Assert.Throws<ArgumentException>(() => new OracleRelationalKey(childTableName, childKeyMock.Object, parentTableName, parentKeyMock.Object, deleteAction));
        }
    }
}
