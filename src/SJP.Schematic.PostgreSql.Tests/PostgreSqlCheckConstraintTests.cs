using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests
{
    [TestFixture]
    internal static class PostgreSqlCheckConstraintTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            Assert.That(() => new PostgreSqlCheckConstraint(null, "test_check"), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
        {
            Assert.That(() => new PostgreSqlCheckConstraint("test_check", null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyDefinition_ThrowsArgumentNullException()
        {
            Assert.That(() => new PostgreSqlCheckConstraint("test_check", string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceDefinition_ThrowsArgumentNullException()
        {
            Assert.That(() => new PostgreSqlCheckConstraint("test_check", "      "), Throws.ArgumentNullException);
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier checkName = "test_check";
            var check = new PostgreSqlCheckConstraint(checkName, "test_check");

            Assert.That(check.Name.UnwrapSome(), Is.EqualTo(checkName));
        }

        [Test]
        public static void Definition_PropertyGet_EqualsCtorArg()
        {
            const string checkDefinition = "test_check_definition";
            var check = new PostgreSqlCheckConstraint("test_check", checkDefinition);

            Assert.That(check.Definition, Is.EqualTo(checkDefinition));
        }

        [Test]
        public static void IsEnabled_PropertyGet_ReturnsTrue()
        {
            var check = new PostgreSqlCheckConstraint("test_check", "test_check_definition");

            Assert.That(check.IsEnabled, Is.True);
        }
    }
}
