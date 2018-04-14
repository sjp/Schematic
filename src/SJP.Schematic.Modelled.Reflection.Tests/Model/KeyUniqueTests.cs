using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SJP.Schematic.Modelled.Reflection.Model.Tests
{
    [TestFixture]
    internal class KeyUniqueTests
    {
        [Test]
        public void Ctor_GivenNullColumns_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Key.Unique((IModelledColumn[])null));
        }

        [Test]
        public void Ctor_GivenEmptyColumns_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Key.Unique(Enumerable.Empty<IModelledColumn>()));
        }

        [Test]
        public void Ctor_GivenCollectionWithNullColumn_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Key.Unique(new List<IModelledColumn> { null }));
        }
    }
}
