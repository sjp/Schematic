using System;
using System.Data;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerRelationalKeyTests
    {
        [Test]
        public static void Ctor_GivenNullChildTable_ThrowsArgumentNullException()
        {
            var childKey = Mock.Of<IDatabaseKey>();
            const string parentTableName = "parent_table";
            var parentKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = Rule.None;
            const Rule updateRule = Rule.None;

            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalKey(null, childKey, parentTableName, parentKey, deleteRule, updateRule));
        }

        [Test]
        public static void Ctor_GivenNullChildKey_ThrowsArgumentNullException()
        {
            const string childTableName = "child_table";
            var parentKey = Mock.Of<IDatabaseKey>();
            const string parentTableName = "parent_table";
            const Rule deleteRule = Rule.None;
            const Rule updateRule = Rule.None;

            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalKey(childTableName, null, parentTableName, parentKey, deleteRule, updateRule));
        }

        [Test]
        public static void Ctor_GivenNullParentTable_ThrowsArgumentNullException()
        {
            const string childTableName = "child_table";
            var childKey = Mock.Of<IDatabaseKey>();
            var parentKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = Rule.None;
            const Rule updateRule = Rule.None;

            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalKey(childTableName, childKey, null, parentKey, deleteRule, updateRule));
        }

        [Test]
        public static void Ctor_GivenNullParentKey_ThrowsArgumentNullException()
        {
            const string childTableName = "child_table";
            var childKey = Mock.Of<IDatabaseKey>();
            const string parentTableName = "parent_table";
            const Rule deleteRule = Rule.None;
            const Rule updateRule = Rule.None;

            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalKey(childTableName, childKey, parentTableName, null, deleteRule, updateRule));
        }

        [Test]
        public static void Ctor_GivenInvalidDeleteRule_ThrowsArgumentException()
        {
            const string childTableName = "child_table";
            var childKey = Mock.Of<IDatabaseKey>();
            const string parentTableName = "parent_table";
            var parentKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = (Rule)55;
            const Rule updateRule = Rule.None;

            Assert.Throws<ArgumentException>(() => new SqlServerRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteRule, updateRule));
        }

        [Test]
        public static void Ctor_GivenInvalidUpdateRule_ThrowsArgumentException()
        {
            const string childTableName = "child_table";
            var childKey = Mock.Of<IDatabaseKey>();
            const string parentTableName = "parent_table";
            var parentKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = Rule.None;
            const Rule updateRule = (Rule)55;

            Assert.Throws<ArgumentException>(() => new SqlServerRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteRule, updateRule));
        }

        [Test]
        public static void ChildTable_PropertyGet_EqualsCtorArg()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";
            const Rule deleteRule = Rule.None;
            const Rule updateRule = Rule.None;

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new SqlServerRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteRule, updateRule);
            Assert.AreEqual(new Identifier(childTableName), relationalKey.ChildTable);
        }

        [Test]
        public static void ChildKey_PropertyGet_EqualsCtorArg()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";
            const Rule deleteRule = Rule.Cascade;
            const Rule updateRule = Rule.SetDefault;
            Identifier keyName = "test_child_key";

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            childKeyMock.Setup(k => k.Name).Returns(keyName);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new SqlServerRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteRule, updateRule);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(keyName, relationalKey.ChildKey.Name);
                Assert.AreSame(childKey, relationalKey.ChildKey);
            });
        }

        [Test]
        public static void ParentTable_PropertyGet_EqualsCtorArg()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";
            const Rule deleteRule = Rule.None;
            const Rule updateRule = Rule.None;

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new SqlServerRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteRule, updateRule);
            Assert.AreEqual(new Identifier(parentTableName), relationalKey.ParentTable);
        }

        [Test]
        public static void ParentKey_PropertyGet_EqualsCtorArg()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";
            const Rule deleteRule = Rule.Cascade;
            const Rule updateRule = Rule.SetDefault;
            Identifier keyName = "test_parent_key";

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            parentKeyMock.Setup(t => t.Name).Returns(keyName);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new SqlServerRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteRule, updateRule);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(keyName, relationalKey.ParentKey.Name);
                Assert.AreSame(parentKey, relationalKey.ParentKey);
            });
        }

        [Test]
        public static void DeleteRule_PropertyGet_EqualsCtorArg()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";
            const Rule deleteRule = Rule.Cascade;
            const Rule updateRule = Rule.SetDefault;

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new SqlServerRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteRule, updateRule);

            Assert.AreEqual(deleteRule, relationalKey.DeleteRule);
        }

        [Test]
        public static void UpdateRule_PropertyGet_EqualsCtorArg()
        {
            const string childTableName = "child_table";
            const string parentTableName = "parent_table";
            const Rule deleteRule = Rule.Cascade;
            const Rule updateRule = Rule.SetDefault;

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new SqlServerRelationalKey(childTableName, childKey, parentTableName, parentKey, deleteRule, updateRule);

            Assert.AreEqual(updateRule, relationalKey.UpdateRule);
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
            const Rule deleteRule = Rule.None;
            const Rule updateRule = Rule.None;

            Assert.Throws<ArgumentException>(() => new SqlServerRelationalKey(childTableName, childKeyMock.Object, parentTableName, parentKeyMock.Object, deleteRule, updateRule));
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
            const Rule deleteRule = Rule.None;
            const Rule updateRule = Rule.None;

            Assert.Throws<ArgumentException>(() => new SqlServerRelationalKey(childTableName, childKeyMock.Object, parentTableName, parentKeyMock.Object, deleteRule, updateRule));
        }
    }
}
