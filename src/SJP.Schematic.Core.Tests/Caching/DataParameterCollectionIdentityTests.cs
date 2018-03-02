using NUnit.Framework;
using System;
using SJP.Schematic.Core.Caching;

namespace SJP.Schematic.Core.Tests.Caching
{
    // can't test much as this is basically just a hash function
    [TestFixture]
    internal class DataParameterCollectionIdentityTests
    {
        [Test]
        public void Ctor_GivenNullDataParameterCollection_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DataParameterCollectionIdentity(null));
        }
    }
}
