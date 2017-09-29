using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Tests
{
    [TestFixture]
    internal class SqliteRelationalKeyTests
    {
        [Test]
        public void Ctor_GivenNullChildKey_ThrowsArgumentNullException()
        {
            var parentKey = Mock.Of<IDatabaseKey>();
            const RelationalKeyUpdateAction deleteAction = RelationalKeyUpdateAction.NoAction;
            const RelationalKeyUpdateAction updateAction = RelationalKeyUpdateAction.NoAction;

            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalKey(null, parentKey, deleteAction, updateAction));
        }

        [Test]
        public void Ctor_GivenNullParentKey_ThrowsArgumentNullException()
        {
            var childKey = Mock.Of<IDatabaseKey>();
            const RelationalKeyUpdateAction deleteAction = RelationalKeyUpdateAction.NoAction;
            const RelationalKeyUpdateAction updateAction = RelationalKeyUpdateAction.NoAction;

            Assert.Throws<ArgumentNullException>(() => new SqliteRelationalKey(childKey, null, deleteAction, updateAction));
        }

        [Test]
        public void Ctor_GivenInvalidDeleteAction_ThrowsArgumentException()
        {
            var childKey = Mock.Of<IDatabaseKey>();
            var parentKey = Mock.Of<IDatabaseKey>();
            const RelationalKeyUpdateAction deleteAction = (RelationalKeyUpdateAction)55;
            const RelationalKeyUpdateAction updateAction = RelationalKeyUpdateAction.NoAction;

            Assert.Throws<ArgumentException>(() => new SqliteRelationalKey(childKey, parentKey, deleteAction, updateAction));
        }

        [Test]
        public void Ctor_GivenInvalidUpdateAction_ThrowsArgumentException()
        {
            var childKey = Mock.Of<IDatabaseKey>();
            var parentKey = Mock.Of<IDatabaseKey>();
            const RelationalKeyUpdateAction deleteAction = RelationalKeyUpdateAction.NoAction;
            const RelationalKeyUpdateAction updateAction = (RelationalKeyUpdateAction)55;

            Assert.Throws<ArgumentException>(() => new SqliteRelationalKey(childKey, parentKey, deleteAction, updateAction));
        }

        [Test]
        public void ChildKey_PropertyGet_EqualsCtorArg()
        {
            var parentKey = Mock.Of<IDatabaseKey>();
            const RelationalKeyUpdateAction deleteAction = RelationalKeyUpdateAction.Cascade;
            const RelationalKeyUpdateAction updateAction = RelationalKeyUpdateAction.SetDefault;

            Identifier keyName = "test_child_key";
            var childKey = new Mock<IDatabaseKey>();
            childKey.Setup(t => t.Name).Returns(keyName);
            var childKeyArg = childKey.Object;

            var relationalKey = new SqliteRelationalKey(childKeyArg, parentKey, deleteAction, updateAction);

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
            const RelationalKeyUpdateAction deleteAction = RelationalKeyUpdateAction.Cascade;
            const RelationalKeyUpdateAction updateAction = RelationalKeyUpdateAction.SetDefault;

            Identifier keyName = "test_parent_key";
            var parentKey = new Mock<IDatabaseKey>();
            parentKey.Setup(t => t.Name).Returns(keyName);
            var parentKeyArg = parentKey.Object;

            var relationalKey = new SqliteRelationalKey(childKey, parentKeyArg, deleteAction, updateAction);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(keyName, relationalKey.ParentKey.Name);
                Assert.AreSame(parentKeyArg, relationalKey.ParentKey);
            });
        }

        [Test]
        public void DeleteAction_PropertyGet_EqualsCtorArg()
        {
            var childKey = Mock.Of<IDatabaseKey>();
            var parentKey = Mock.Of<IDatabaseKey>();
            const RelationalKeyUpdateAction deleteAction = RelationalKeyUpdateAction.Cascade;
            const RelationalKeyUpdateAction updateAction = RelationalKeyUpdateAction.SetDefault;

            var relationalKey = new SqliteRelationalKey(childKey, parentKey, deleteAction, updateAction);

            Assert.AreEqual(deleteAction, relationalKey.DeleteAction);
        }

        [Test]
        public void UpdateAction_PropertyGet_EqualsCtorArg()
        {
            var childKey = Mock.Of<IDatabaseKey>();
            var parentKey = Mock.Of<IDatabaseKey>();
            const RelationalKeyUpdateAction deleteAction = RelationalKeyUpdateAction.Cascade;
            const RelationalKeyUpdateAction updateAction = RelationalKeyUpdateAction.SetDefault;

            var relationalKey = new SqliteRelationalKey(childKey, parentKey, deleteAction, updateAction);

            Assert.AreEqual(updateAction, relationalKey.UpdateAction);
        }
    }
}
