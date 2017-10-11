using System;
using System.Data;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal class SqlServerRelationalKeyTests
    {
        [Test]
        public void Ctor_GivenNullChildKey_ThrowsArgumentNullException()
        {
            var parentKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = Rule.None;
            const Rule updateRule = Rule.None;

            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalKey(null, parentKey, deleteRule, updateRule));
        }

        [Test]
        public void Ctor_GivenNullParentKey_ThrowsArgumentNullException()
        {
            var childKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = Rule.None;
            const Rule updateRule = Rule.None;

            Assert.Throws<ArgumentNullException>(() => new SqlServerRelationalKey(childKey, null, deleteRule, updateRule));
        }

        [Test]
        public void Ctor_GivenInvalidDeleteRule_ThrowsArgumentException()
        {
            var childKey = Mock.Of<IDatabaseKey>();
            var parentKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = (Rule)55;
            const Rule updateRule = Rule.None;

            Assert.Throws<ArgumentException>(() => new SqlServerRelationalKey(childKey, parentKey, deleteRule, updateRule));
        }

        [Test]
        public void Ctor_GivenInvalidUpdateRule_ThrowsArgumentException()
        {
            var childKey = Mock.Of<IDatabaseKey>();
            var parentKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = Rule.None;
            const Rule updateRule = (Rule)55;

            Assert.Throws<ArgumentException>(() => new SqlServerRelationalKey(childKey, parentKey, deleteRule, updateRule));
        }

        [Test]
        public void ChildKey_PropertyGet_EqualsCtorArg()
        {
            var parentKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = Rule.Cascade;
            const Rule updateRule = Rule.SetDefault;

            Identifier keyName = "test_child_key";
            var childKey = new Mock<IDatabaseKey>();
            childKey.Setup(t => t.Name).Returns(keyName);
            var childKeyArg = childKey.Object;

            var relationalKey = new SqlServerRelationalKey(childKeyArg, parentKey, deleteRule, updateRule);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(keyName, relationalKey.ChildKey.Name);
                Assert.AreSame(childKeyArg, relationalKey.ChildKey);
            });
        }

        [Test]
        public void ParentKey_PropertyGet_EqualsCtorArg()
        {
            var childKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = Rule.Cascade;
            const Rule updateRule = Rule.SetDefault;

            Identifier keyName = "test_parent_key";
            var parentKey = new Mock<IDatabaseKey>();
            parentKey.Setup(t => t.Name).Returns(keyName);
            var parentKeyArg = parentKey.Object;

            var relationalKey = new SqlServerRelationalKey(childKey, parentKeyArg, deleteRule, updateRule);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(keyName, relationalKey.ParentKey.Name);
                Assert.AreSame(parentKeyArg, relationalKey.ParentKey);
            });
        }

        [Test]
        public void DeleteRule_PropertyGet_EqualsCtorArg()
        {
            var childKey = Mock.Of<IDatabaseKey>();
            var parentKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = Rule.Cascade;
            const Rule updateRule = Rule.SetDefault;

            var relationalKey = new SqlServerRelationalKey(childKey, parentKey, deleteRule, updateRule);

            Assert.AreEqual(deleteRule, relationalKey.DeleteRule);
        }

        [Test]
        public void UpdateRule_PropertyGet_EqualsCtorArg()
        {
            var childKey = Mock.Of<IDatabaseKey>();
            var parentKey = Mock.Of<IDatabaseKey>();
            const Rule deleteRule = Rule.Cascade;
            const Rule updateRule = Rule.SetDefault;

            var relationalKey = new SqlServerRelationalKey(childKey, parentKey, deleteRule, updateRule);

            Assert.AreEqual(updateRule, relationalKey.UpdateRule);
        }
    }
}
