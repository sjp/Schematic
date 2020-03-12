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

        [TestCase("", "test_synonym", "", "test_target", "Synonym: test_synonym -> test_target")]
        [TestCase("test_schema", "test_synonym", "", "test_target", "Synonym: test_schema.test_synonym -> test_target")]
        [TestCase("", "test_synonym", "target_schema", "test_target", "Synonym: test_synonym -> target_schema.test_target")]
        [TestCase("test_schema", "test_synonym", "", "test_target", "Synonym: test_schema.test_synonym -> test_target")]
        [TestCase("test_schema", "test_synonym", "target_schema", "test_target", "Synonym: test_schema.test_synonym -> target_schema.test_target")]
        public static void ToString_WhenInvoked_ReturnsExpectedString(string synonymSchema, string synonymLocalName, string targetSchema, string targetLocalName, string expectedOutput)
        {
            var synonymName = Identifier.CreateQualifiedIdentifier(synonymSchema, synonymLocalName);
            var targetName = Identifier.CreateQualifiedIdentifier(targetSchema, targetLocalName);

            var synonym = new DatabaseSynonym(synonymName, targetName);

            var result = synonym.ToString();

            Assert.That(result, Is.EqualTo(expectedOutput));
        }
    }
}
