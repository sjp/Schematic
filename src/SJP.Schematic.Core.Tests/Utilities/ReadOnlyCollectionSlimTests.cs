using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Tests.Utilities
{
    [TestFixture]
    internal static class ReadOnlyCollectionSlimTests
    {
        [Test]
        public static void Ctor_GivenNegativeCount_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReadOnlyCollectionSlim<int>(-1, Array.Empty<int>()));
        }

        [Test]
        public static void Ctor_GivenNullCollection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ReadOnlyCollectionSlim<int>(1, null));
        }

        [Test]
        public static void Count_PropertyGet_EqualsCtorArg()
        {
            const int count = 111;
            var collection = new ReadOnlyCollectionSlim<int>(count, Array.Empty<int>());

            Assert.AreEqual(count, collection.Count);
        }

        [Test]
        public static void Collection_WhenEnumerated_EqualsInputData()
        {
            const int count = 111;
            var data = new[] { 2, 4, 6, 8, 10 };
            var collection = new ReadOnlyCollectionSlim<int>(count, data);

            var equalCollections = data.SequenceEqual(collection);

            Assert.IsTrue(equalCollections);
        }

        [Test]
        public static void Collection_WhenEnumeratedWithNonGenericEnumerator_EqualsInputData()
        {
            const int count = 111;
            var data = new[] { 2, 4, 6, 8, 10 };
            IEnumerable collection = new ReadOnlyCollectionSlim<int>(count, data);

            var result = true;
            var i = 0;
            foreach (int entry in collection)
            {
                var expected = data[i];
                if (expected != entry)
                {
                    result = false;
                    break;
                }
                i++;
            }

            Assert.IsTrue(result);
        }
    }
}
