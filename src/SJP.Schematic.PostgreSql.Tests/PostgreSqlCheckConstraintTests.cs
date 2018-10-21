using System;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests
{
    [TestFixture]
    internal static class PostgreSqlCheckConstraintTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlCheckConstraint(null, "test_check"));
        }

        [Test]
        public static void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlCheckConstraint("test_check", null));
        }

        [Test]
        public static void Ctor_GivenEmptyDefinition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlCheckConstraint("test_check", string.Empty));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceDefinition_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PostgreSqlCheckConstraint("test_check", "      "));
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier checkName = "test_check";
            var check = new PostgreSqlCheckConstraint(checkName, "test_check");

            Assert.AreEqual(checkName, check.Name);
        }

        [Test]
        public static void Definition_PropertyGet_EqualsCtorArg()
        {
            const string checkDefinition = "test_check_definition";
            var check = new PostgreSqlCheckConstraint("test_check", checkDefinition);

            Assert.AreEqual(checkDefinition, check.Definition);
        }

        [Test]
        public static void IsEnabled_PropertyGet_ReturnsTrue()
        {
            var check = new PostgreSqlCheckConstraint("test_check", "test_check_definition");

            Assert.IsTrue(check.IsEnabled);
        }
    }
}
