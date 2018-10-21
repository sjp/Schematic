using System;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal static class SqlServerCheckConstraintTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlServerCheckConstraint(null, "test_check", true));
        }

        [Test]
        public static void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlServerCheckConstraint("test_check", null, true));
        }

        [Test]
        public static void Ctor_GivenEmptyDefinition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlServerCheckConstraint("test_check", string.Empty, true));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceDefinition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlServerCheckConstraint("test_check", "      ", true));
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier checkName = "test_check";
            var check = new SqlServerCheckConstraint(checkName, "test_check", true);

            Assert.AreEqual(checkName, check.Name);
        }

        [Test]
        public static void Definition_PropertyGet_EqualsCtorArg()
        {
            const string checkDefinition = "test_check_definition";
            var check = new SqlServerCheckConstraint("test_check", checkDefinition, true);

            Assert.AreEqual(checkDefinition, check.Definition);
        }

        [Test]
        public static void IsEnabled_PropertyGetWhenCtorGivenTrue_EqualsCtorArg()
        {
            var check = new SqlServerCheckConstraint("test_check", "test_check_definition", true);

            Assert.IsTrue(check.IsEnabled);
        }

        [Test]
        public static void IsEnabled_PropertyGetWhenCtorGivenFalse_EqualsCtorArg()
        {
            var check = new SqlServerCheckConstraint("test_check", "test_check_definition", false);

            Assert.IsFalse(check.IsEnabled);
        }
    }
}
