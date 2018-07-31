using NUnit.Framework;
using System;
using SJP.Schematic.Core.Caching;

namespace SJP.Schematic.Core.Tests.Caching
{
    // can't test much as this is basically just a hash function
    [TestFixture]
    internal static class DataParameterIdentityTests
    {
        [Test]
        public static void Ctor_GivenNullDataParameter_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DataParameterIdentity(null));
        }
    }
}
