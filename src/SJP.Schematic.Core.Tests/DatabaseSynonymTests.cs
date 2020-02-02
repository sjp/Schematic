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
            Assert.That(() => new DatabaseSynonym(null, "test"), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullTarget_ThrowsArgNullException()
        {
            Assert.That(() => new DatabaseSynonym("test", null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Name_PropertyGet_ShouldEqualCtorArg()
        {
            const string synonymName = "synonym_test_synonym_1";
            var synonym = new DatabaseSynonym(synonymName, synonymName);

            Assert.That(synonym.Name.LocalName, Is.EqualTo(synonymName));
        }

        [Test]
        public static void Target_PropertyGetGivenNotNullCtorArg_ShouldEqualCtorArg()
        {
            const string synonymName = "synonym_test_synonym_1";
            var synonym = new DatabaseSynonym(synonymName, synonymName);

            Assert.That(synonym.Target.LocalName, Is.EqualTo(synonymName));
        }
    }
}
