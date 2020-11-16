using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Tests.Utilities
{
    [TestFixture]
    internal static class AsyncCacheTests
    {
        [Test]
        public static void Ctor_GivenNullFactory_ThrowsArgumentNullException()
        {
            Assert.That(() => new AsyncCache<object, object, object>(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetByKeyAsync_GivenNullKey_ThrowsArgumentNullException()
        {
            var cache = new AsyncCache<object, object, object>((object _, object __, CancellationToken ___) => Task.FromResult(new object()));

            Assert.That(() => cache.GetByKeyAsync(null, new object()), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetByKeyAsync_GivenNullCache_ThrowsArgumentNullException()
        {
            var cache = new AsyncCache<object, object, object>((object _, object __, CancellationToken ___) => Task.FromResult(new object()));

            Assert.That(() => cache.GetByKeyAsync(new object(), null), Throws.ArgumentNullException);
        }

        [Test]
        public static async Task GetByKeyAsync_WhenCalledTwiceWithSameKey_InvokesFactoryOnlyOnce()
        {
            var counter = 0;
            var cache = new AsyncCache<string, string, string>((string _, string __, CancellationToken ___) =>
            {
                counter++;
                return Task.FromResult(string.Empty);
            });

            await cache.GetByKeyAsync("a", "cache_ignore").ConfigureAwait(false);
            await cache.GetByKeyAsync("a", "cache_ignore").ConfigureAwait(false);

            Assert.That(counter, Is.EqualTo(1));
        }
    }
}
