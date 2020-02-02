using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model.Tests
{
    [TestFixture]
    internal static class KeyForeignTTests
    {
        [Test]
        public static void Ctor_GivenNullSelector_ThrowsArgumentNullException()
        {
            var testColumn = Mock.Of<IModelledColumn>();
            Assert.That(() => new Key.Foreign<TestTable1>(null, testColumn), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullColumn_ThrowsArgumentNullException()
        {
            IModelledColumn testColumn = null;
            Assert.That(() => new Key.Foreign<TestTable1>(t => t.PK_TARGET, testColumn), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullColumns_ThrowsArgumentNullException()
        {
            IEnumerable<IModelledColumn> testColumns = null;
            Assert.That(() => new Key.Foreign<TestTable1>(t => t.PK_TARGET, testColumns), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenCollectionWithNullColumns_ThrowsArgumentNullException()
        {
            var testColumns = new IModelledColumn[] { null };
            Assert.That(() => new Key.Foreign<TestTable1>(t => t.PK_TARGET, testColumns), Throws.ArgumentNullException);
        }

        [Test]
        public static void TargetType_PropertyGet_MatchesTypeArgument()
        {
            var testColumn = Mock.Of<IModelledColumn>();
            var foreignKey = new Key.Foreign<TestTable1>(t => t.PK_TARGET, testColumn);

            Assert.That(foreignKey.TargetType, Is.EqualTo(typeof(TestTable1)));
        }

        [Test]
        public static void KeySelector_WithSelectorToPrimaryKeyAndPropertyInfoNotSet_ThrowsInvalidOperationException()
        {
            var testColumn = Mock.Of<IModelledColumn>();
            var foreignKey = new Key.Foreign<TestTable1>(t => t.PK_TARGET, testColumn);

            var instance = new TestTable1();
            Assert.That(() => foreignKey.KeySelector.Invoke(instance), Throws.InvalidOperationException);
        }

        [Test]
        public static void KeySelector_WithSelectorUniqueKeyAndPropertyInfoNotSet_ThrowsInvalidOperationException()
        {
            var testColumn = Mock.Of<IModelledColumn>();
            var foreignKey = new Key.Foreign<TestTable1>(t => t.UK_TARGET, testColumn);

            var instance = new TestTable1();
            Assert.That(() => foreignKey.KeySelector.Invoke(instance), Throws.InvalidOperationException);
        }

        [Test]
        [Ignore("Disable until we can work out why CI is failing but not local.")]
        public static void KeySelector_WithSimpleFunctionToPrimaryKey_ReturnsCorrectKey()
        {
            var testColumn = Mock.Of<IModelledColumn>();
            var foreignKey = new Key.Foreign<TestTable1>(t => t.PK_TARGET, testColumn)
            {
                Property = typeof(TestTable1)
                    .GetProperties()
                    .Single(p => p.Name == nameof(TestTable1.FK_PK_SOURCE))
            };

            var instance = new TestTable1();
            var targetKey = foreignKey.KeySelector.Invoke(instance);

            Assert.That(targetKey.KeyType, Is.EqualTo(DatabaseKeyType.Primary));
        }

        [Test]
        [Ignore("Disable until we can work out why CI is failing but not local.")]
        public static void KeySelector_WithSimpleFunctionToUniqueKey_ReturnsCorrectKey()
        {
            var testColumn = Mock.Of<IModelledColumn>();
            var foreignKey = new Key.Foreign<TestTable1>(t => t.UK_TARGET, testColumn)
            {
                Property = typeof(TestTable1)
                    .GetProperties()
                    .Single(p => p.Name == nameof(TestTable1.FK_UK_SOURCE))
            };

            var instance = new TestTable1();
            var targetKey = foreignKey.KeySelector.Invoke(instance);

            Assert.That(targetKey.KeyType, Is.EqualTo(DatabaseKeyType.Unique));
        }

        [Test]
        public static void KeySelector_GivenWrongObject_ThrowsInvalidOperationException()
        {
            var testColumn = Mock.Of<IModelledColumn>();
            var foreignKey = new Key.Foreign<TestTable1>(t => t.PK_TARGET, testColumn);

            var instance = new object();
            Assert.That(() => foreignKey.KeySelector.Invoke(instance), Throws.InvalidOperationException);
        }

        [Test]
        public static void KeySelector_WhenCtorNotGivenSimpleSelector_ThrowsInvalidOperationException()
        {
            var testColumn = Mock.Of<IModelledColumn>();

            static Key selector(TestTable1 t)
            {
                const string testString = "test";
                var strLength = testString.Length;
                Debug.Assert(strLength > 0);
                return t.PK_TARGET;
            }
            var foreignKey = new Key.Foreign<TestTable1>(selector, testColumn);

            var instance = new TestTable1();
            Assert.That(() => foreignKey.KeySelector.Invoke(instance), Throws.InvalidOperationException);
        }

        public class TestTable1
        {
            private IModelledColumn TestColumn { get; } = Mock.Of<IModelledColumn>();

            public Key PK_TARGET => new Key.Primary(TestColumn);

            public Key UK_TARGET => new Key.Unique(TestColumn);

            public Key FK_PK_SOURCE => new Key.Foreign<TestTable1>(t => t.PK_TARGET, TestColumn);

            public Key FK_UK_SOURCE => new Key.Foreign<TestTable1>(t => t.UK_TARGET, TestColumn);
        }
    }
}
