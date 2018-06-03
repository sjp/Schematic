using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Tests
{
    [TestFixture]
    internal class KeyForeignTests
    {
        [Test]
        public void Ctor_GivenNullColumns_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FakeForeignKey(null));
        }

        [Test]
        public void Ctor_GivenEmptyColumns_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FakeForeignKey(Array.Empty<IModelledColumn>()));
        }

        [Test]
        public void Ctor_GivenCollectionWithNullColumn_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FakeForeignKey(new List<IModelledColumn> { null }));
        }

        private class FakeForeignKey : Key.ForeignKey
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
