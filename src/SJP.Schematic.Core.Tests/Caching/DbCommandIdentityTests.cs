using NUnit.Framework;
using System;
using SJP.Schematic.Core.Caching;

namespace SJP.Schematic.Core.Tests.Caching
{
    // can't test much as this is basically just a hash function
    [TestFixture]
    public class DbCommandIdentityTests
    {
        [Test]
        public void Ctor_GivenNullCommand_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DbCommandIdentity(null));
        }
    }
}
