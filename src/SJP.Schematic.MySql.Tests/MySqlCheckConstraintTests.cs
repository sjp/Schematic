using System;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Tests
{
    [TestFixture]
    internal static class MySqlCheckConstraintTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MySqlCheckConstraint(null, "test", true));
        }

        [Test]
        public static void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
        {
            Identifier checkName = "test_check";

            Assert.Throws<ArgumentNullException>(() => new MySqlCheckConstraint(checkName, null, true));
        }

        [Test]
        public static void Ctor_GivenEmptyDefinition_ThrowsArgumentNullException()
        {
            Identifier checkName = "test_check";

            Assert.Throws<ArgumentNullException>(() => new MySqlCheckConstraint(checkName, string.Empty, true));
        }

        [Test]
        public static void Ctor_GivenWhitespaceDefinition_ThrowsArgumentNullException()
        {
            Identifier checkName = "test_check";

            Assert.Throws<ArgumentNullException>(() => new MySqlCheckConstraint(checkName, "    ", true));
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier checkName = "test_check";
            var check = new MySqlCheckConstraint(checkName, "test_definition", true);

            Assert.AreEqual(checkName, check.Name.UnwrapSome());
        }

        [Test]
        public static void Definition_PropertyGet_EqualsCtorArg()
        {
            Identifier checkName = "test_check";
            const string definition = "test_definition";
            var check = new MySqlCheckConstraint(checkName, definition, true);

            Assert.AreEqual(definition, check.Definition);
        }

        [Test]
        public static void IsEnabled_PropertyGetGivenTrueCtorArg_EqualsCtorArg()
        {
            Identifier checkName = "test_check";
            const string definition = "test_definition";
            var check = new MySqlCheckConstraint(checkName, definition, true);

            Assert.IsTrue(check.IsEnabled);
        }

        [Test]
        public static void IsEnabled_PropertyGetGivenFalseCtorArg_EqualsCtorArg()
        {
            Identifier checkName = "test_check";
            const string definition = "test_definition";
            var check = new MySqlCheckConstraint(checkName, definition, false);

            Assert.IsFalse(check.IsEnabled);
        }
    }
}
