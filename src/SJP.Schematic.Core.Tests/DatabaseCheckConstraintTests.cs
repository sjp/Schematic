using System;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class DatabaseCheckConstraintTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DatabaseCheckConstraint(null, "test_check", true));
        }

        [Test]
        public static void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DatabaseCheckConstraint("test_check", null, true));
        }

        [Test]
        public static void Ctor_GivenEmptyDefinition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DatabaseCheckConstraint("test_check", string.Empty, true));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceDefinition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DatabaseCheckConstraint("test_check", "      ", true));
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier checkName = "test_check";
            var check = new DatabaseCheckConstraint(checkName, "test_check", true);

            Assert.AreEqual(checkName, check.Name);
        }

        [Test]
        public static void Definition_PropertyGet_EqualsCtorArg()
        {
            const string checkDefinition = "test_check_definition";
            var check = new DatabaseCheckConstraint("test_check", checkDefinition, true);

            Assert.AreEqual(checkDefinition, check.Definition);
        }

        [Test]
        public static void IsEnabled_WhenTrueProvidedInCtor_ReturnsTrue()
        {
            var check = new DatabaseCheckConstraint("test_check", "test_check_definition", true);

            Assert.IsTrue(check.IsEnabled);
        }

        [Test]
        public static void IsEnabled_WhenFalseProvidedInCtor_ReturnsFalse()
        {
            var check = new DatabaseCheckConstraint("test_check", "test_check_definition", false);

            Assert.IsFalse(check.IsEnabled);
        }
    }
}
