using System;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class DatabaseSynonymTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DatabaseSynonym(null, "test"));
        }

        [Test]
        public static void Ctor_GivenNullTarget_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DatabaseSynonym("test", null));
        }

        [Test]
        public static void Name_PropertyGet_ShouldEqualCtorArg()
        {
            const string synonymName = "synonym_test_synonym_1";
            var synonym = new DatabaseSynonym(synonymName, synonymName);

            Assert.AreEqual(synonymName, synonym.Name.LocalName);
        }

        [Test]
        public static void Target_PropertyGetGivenNotNullCtorArg_ShouldEqualCtorArg()
        {
            const string synonymName = "synonym_test_synonym_1";
            var synonym = new DatabaseSynonym(synonymName, synonymName);

            Assert.AreEqual(synonymName, synonym.Target.LocalName);
        }
    }
}
