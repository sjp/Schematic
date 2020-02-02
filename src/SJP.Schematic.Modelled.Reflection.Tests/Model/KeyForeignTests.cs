using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Tests
{
    [TestFixture]
    internal static class KeyForeignTests
    {
        [Test]
        public static void Ctor_GivenNullColumns_ThrowsArgumentNullException()
        {
            Assert.That(() => new FakeForeignKey(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyColumns_ThrowsArgumentNullException()
        {
            Assert.That(() => new FakeForeignKey(Array.Empty<IModelledColumn>()), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenCollectionWithNullColumn_ThrowsArgumentNullException()
        {
            Assert.That(() => new FakeForeignKey(new List<IModelledColumn> { null }), Throws.ArgumentNullException);
        }

        private sealed class FakeForeignKey : Key.ForeignKey
        {
            public FakeForeignKey(IEnumerable<IModelledColumn> columns)
                : base(columns)
            {
            }

            public override Type TargetType => null;

            public override Func<object, Key> KeySelector => null;
        }
    }
}
