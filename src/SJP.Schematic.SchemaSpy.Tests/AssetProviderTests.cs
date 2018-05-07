using System;
using System.Linq;
using NUnit.Framework;
using SJP.Schematic.SchemaSpy.Html;

namespace SJP.Schematic.SchemaSpy.Tests
{
    [TestFixture]
    public class AssetProviderTests
    {
        [Test]
        public void AssetResourceNames_PropertyGet_IsNotEmpty()
        {
            var provider = new AssetProvider();
            var hasResourceNames = provider.AssetResourceNames.Any();

            Assert.IsTrue(hasResourceNames);
        }
    }
}
