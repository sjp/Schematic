using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Data;

namespace SJP.Schematic.Sqlite.Tests
{
    [TestFixture]
    internal class SqliteRelationalKeyTests
    {
        [Test]
        public void Ctor_GivenNullChildKey_ThrowsArgumentNullException()
        {
            var parentKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = Rule.None;
            const Rule updateRule = Rule.None;

            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalKey(null, parentKey, deleteRule, updateRule));
        }

        [Test]
        public void Ctor_GivenNullParentKey_ThrowsArgumentNullException()
        {
            var childKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = Rule.None;
            const Rule updateRule = Rule.None;

            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalKey(childKey, null, deleteRule, updateRule));
        }

        [Test]
        public void Ctor_GivenInvalidDeleteRule_ThrowsArgumentException()
        {
            var childKey = Mock.Of<IDatabaseKey>();
            var parentKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = (Rule)55;
            const Rule updateRule = Rule.None;

            Assert.Throws<ArgumentException>(() => new SqliteRelationalKey(childKey, parentKey, deleteRule, updateRule));
        }

        [Test]
        public void Ctor_GivenInvalidUpdateRule_ThrowsArgumentException()
        {
            var childKey = Mock.Of<IDatabaseKey>();
            var parentKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = Rule.None;
            const Rule updateRule = (Rule)55;

            Assert.Throws<ArgumentException>(() => new SqliteRelationalKey(childKey, parentKey, deleteRule, updateRule));
        }

        [Test]
        public void ChildKey_PropertyGet_EqualsCtorArg()
        {
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

            var relationalKey = new SqliteRelationalKey(childKey, parentKey, deleteRule, updateRule);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(keyName, relationalKey.ChildKey.Name);
                Assert.AreSame(childKey, relationalKey.ChildKey);
            });
        }

        [Test]
        public void ParentKey_PropertyGet_EqualsCtorArg()
        {
            const Rule deleteRule = Rule.Cascade;
            const Rule updateRule = Rule.SetDefault;
            Identifier keyName = "test_parent_key";

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var childKey = childKeyMock.Object;

            var parentKey = new Mock<IDatabaseKey>();
            parentKey.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            parentKey.Setup(t => t.Name).Returns(keyName);
            var parentKeyArg = parentKey.Object;

            var relationalKey = new SqliteRelationalKey(childKey, parentKeyArg, deleteRule, updateRule);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(keyName, relationalKey.ParentKey.Name);
                Assert.AreSame(parentKeyArg, relationalKey.ParentKey);
            });
        }

        [Test]
        public void DeleteRule_PropertyGet_EqualsCtorArg()
        {
            const Rule deleteRule = Rule.Cascade;
            const Rule updateRule = Rule.SetDefault;

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new SqliteRelationalKey(childKey, parentKey, deleteRule, updateRule);

            Assert.AreEqual(deleteRule, relationalKey.DeleteRule);
        }

        [Test]
        public void UpdateRule_PropertyGet_EqualsCtorArg()
        {
            const Rule deleteRule = Rule.Cascade;
            const Rule updateRule = Rule.SetDefault;

            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var childKey = childKeyMock.Object;

            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKey = parentKeyMock.Object;

            var relationalKey = new SqliteRelationalKey(childKey, parentKey, deleteRule, updateRule);

            Assert.AreEqual(updateRule, relationalKey.UpdateRule);
        }

        [Test]
        public void Ctor_GivenChildKeyNotForeignKey_ThrowsArgumentException()
        {
            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Primary);
            const Rule deleteRule = Rule.None;
            const Rule updateRule = Rule.None;

            Assert.Throws<ArgumentException>(() => new SqliteRelationalKey(childKeyMock.Object, parentKeyMock.Object, deleteRule, updateRule));
        }

        [Test]
        public void Ctor_GivenParentKeyNotCandidateKey_ThrowsArgumentException()
        {
            var childKeyMock = new Mock<IDatabaseKey>();
            childKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            var parentKeyMock = new Mock<IDatabaseKey>();
            parentKeyMock.Setup(k => k.KeyType).Returns(DatabaseKeyType.Foreign);
            const Rule deleteRule = Rule.None;
            const Rule updateRule = Rule.None;

            Assert.Throws<ArgumentException>(() => new SqliteRelationalKey(childKeyMock.Object, parentKeyMock.Object, deleteRule, updateRule));
        }
    }
}
