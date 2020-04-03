using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Sqlite.Tests
{
    [TestFixture]
    internal static class SqliteCheckConstraintTests
    {
        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void Ctor_GivenNullOrWhiteSpaceDefinition_ThrowsArgumentNullException(string definition)
        {
            Assert.That(() => new SqliteCheckConstraint(Option<Identifier>.Some("test_check"), definition), Throws.ArgumentNullException);
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier checkName = "test_check";
            var check = new SqliteCheckConstraint(checkName, "test_check_definition");

            Assert.That(check.Name.UnwrapSome(), Is.EqualTo(checkName));
        }

        [Test]
        public static void Definition_PropertyGet_EqualsCtorArg()
        {
            const string checkDefinition = "test_check_definition";
            var check = new SqliteCheckConstraint(Option<Identifier>.Some("test_check"), checkDefinition);

            Assert.That(check.Definition, Is.EqualTo(checkDefinition));
        }

        [Test]
        public static void IsEnabled_PropertyGet_ReturnsTrue()
        {
            var check = new SqliteCheckConstraint(Option<Identifier>.Some("test_check"), "test_check_definition");

            Assert.That(check.IsEnabled, Is.True);
        }
    }
}
