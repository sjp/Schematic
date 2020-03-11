using System.Threading;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions
{
    [TestFixture]
    internal static class ProcessExtensionsTests
    {
        [Test]
        public static void WaitForExitAsync_GivenNullProcess_ThrowsArgumentNullException()
        {
            Assert.That(() => ProcessExtensions.WaitForExitAsync(null, CancellationToken.None), Throws.ArgumentNullException);
        }
    }
}
