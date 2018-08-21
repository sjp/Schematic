using NUnit.Framework;
using System;
using SJP.Schematic.Core.Caching;

namespace SJP.Schematic.Core.Tests.Caching
{
    // can't test much as this is basically just a hash function
    [TestFixture]
    internal static class DbCommandIdentityProviderTests
    {
        [Test]
        public static void Ctor_GivenNullCommand_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => DbCommandIdentityProvider.GetIdentity(null));
        }
    }
}
