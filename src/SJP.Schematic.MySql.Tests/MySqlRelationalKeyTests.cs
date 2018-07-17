using System;
using System.Data;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Tests
{
    [TestFixture]
    internal static class MySqlRelationalKeyTests
    {
        [Test]
        public static void Ctor_GivenNullChildKey_ThrowsArgumentNullException()
        {
            var parentKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = Rule.None;
            const Rule updateRule = Rule.None;

            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalKey(null, parentKey, deleteRule, updateRule));
        }

        [Test]
        public static void Ctor_GivenNullParentKey_ThrowsArgumentNullException()
        {
            var childKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = Rule.None;
            const Rule updateRule = Rule.None;

            Assert.Throws<ArgumentNullException>(() => new MySqlRelationalKey(childKey, null, deleteRule, updateRule));
        }

        [Test]
        public static void Ctor_GivenInvalidDeleteRule_ThrowsArgumentException()
        {
            var childKey = Mock.Of<IDatabaseKey>();
            var parentKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = (Rule)55;
            const Rule updateRule = Rule.None;

            Assert.Throws<ArgumentException>(() => new MySqlRelationalKey(childKey, parentKey, deleteRule, updateRule));
        }

        [Test]
        public static void Ctor_GivenInvalidUpdateRule_ThrowsArgumentException()
        {
            var childKey = Mock.Of<IDatabaseKey>();
            var parentKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = Rule.None;
            const Rule updateRule = (Rule)55;

            Assert.Throws<ArgumentException>(() => new MySqlRelationalKey(childKey, parentKey, deleteRule, updateRule));
        }

        [Test]
        public static void ChildKey_PropertyGet_EqualsCtorArg()
        {
            const Rule deleteRule = Rule.Cascade;
            const Rule updateRule = Rule.SetNull;
            Identifier keyName = "test_child_key";

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            childKeyMock.Setup(k => k.Name).Returns(keyName);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new MySqlRelationalKey(childKey, parentKey, deleteRule, updateRule);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(keyName, relationalKey.ChildKey.Name);
                Assert.AreSame(childKey, relationalKey.ChildKey);
            });
        }

        [Test]
        public static void ParentKey_PropertyGet_EqualsCtorArg()
        {
            const Rule deleteRule = Rule.Cascade;
            const Rule updateRule = Rule.SetNull;
            Identifier keyName = "test_parent_key";

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            parentKeyMock.Setup(t => t.Name).Returns(keyName);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new MySqlRelationalKey(childKey, parentKey, deleteRule, updateRule);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(keyName, relationalKey.ParentKey.Name);
                Assert.AreSame(parentKey, relationalKey.ParentKey);
            });
        }

        [Test]
        public static void DeleteRule_PropertyGet_EqualsCtorArg()
        {
            const Rule deleteRule = Rule.Cascade;
            const Rule updateRule = Rule.SetNull;

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new MySqlRelationalKey(childKey, parentKey, deleteRule, updateRule);

            Assert.AreEqual(deleteRule, relationalKey.DeleteRule);
        }

        [Test]
        public static void UpdateRule_PropertyGet_EqualsCtorArg()
        {
            const Rule deleteRule = Rule.Cascade;
            const Rule updateRule = Rule.SetNull;

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new MySqlRelationalKey(childKey, parentKey, deleteRule, updateRule);

            Assert.AreEqual(updateRule, relationalKey.UpdateRule);
        }

        [Test]
        public static void Ctor_GivenChildKeyNotForeignKey_ThrowsArgumentException()
        {
            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            const Rule deleteRule = Rule.None;
            const Rule updateRule = Rule.None;

            Assert.Throws<ArgumentException>(() => new MySqlRelationalKey(childKeyMock.Object, parentKeyMock.Object, deleteRule, updateRule));
        }

        [Test]
        public static void Ctor_GivenParentKeyNotCandidateKey_ThrowsArgumentException()
        {
            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            const Rule deleteRule = Rule.None;
            const Rule updateRule = Rule.None;

            Assert.Throws<ArgumentException>(() => new MySqlRelationalKey(childKeyMock.Object, parentKeyMock.Object, deleteRule, updateRule));
        }

        [Test]
        public static void Ctor_GivenSetDefaultUpdateRule_ThrowsArgumentException()
        {
            var childKey = Mock.Of<IDatabaseKey>();
            var parentKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = Rule.None;
            const Rule updateRule = Rule.SetDefault;

            Assert.Throws<ArgumentException>(() => new MySqlRelationalKey(childKey, parentKey, deleteRule, updateRule));
        }

        [Test]
        public static void Ctor_GivenSetDefaultDeleteRule_ThrowsArgumentException()
        {
            var childKey = Mock.Of<IDatabaseKey>();
            var parentKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = Rule.SetDefault;
            const Rule updateRule = Rule.None;

            Assert.Throws<ArgumentException>(() => new MySqlRelationalKey(childKey, parentKey, deleteRule, updateRule));
        }
    }
}
