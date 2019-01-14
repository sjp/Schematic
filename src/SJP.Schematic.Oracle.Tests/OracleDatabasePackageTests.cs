using System;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using LanguageExt;

namespace SJP.Schematic.Oracle.Tests
{
    [TestFixture]
    internal static class OracleDatabasePackageTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            const string specification = "spec";
            var body = Option<string>.Some("body");

            Assert.Throws<ArgumentNullException>(() => new OracleDatabasePackage(null, specification, body));
        }

        [Test]
        public static void Ctor_GivenNullSpecification_ThrowsArgumentNullException()
        {
            const string packageName = "test_package";
            var body = Option<string>.Some("body");

            Assert.Throws<ArgumentNullException>(() => new OracleDatabasePackage(packageName, null, body));
        }

        [Test]
        public static void Ctor_GivenEmptySpecification_ThrowsArgumentNullException()
        {
            const string packageName = "test_package";
            var body = Option<string>.Some("body");

            Assert.Throws<ArgumentNullException>(() => new OracleDatabasePackage(packageName, string.Empty, body));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceSpecification_ThrowsArgumentNullException()
        {
            const string packageName = "test_package";
            const string specification = "    ";
            var body = Option<string>.Some("body");

            Assert.Throws<ArgumentNullException>(() => new OracleDatabasePackage(packageName, specification, body));
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            const string packageName = "test_package";
            const string specification = "spec";
            var body = Option<string>.Some("body");

            var package = new OracleDatabasePackage(packageName, specification, body);

            Assert.AreEqual(packageName, package.Name.LocalName);
        }

        [Test]
        public static void Specification_PropertyGet_EqualsCtorArg()
        {
            const string packageName = "test_package";
            const string specification = "spec";
            var body = Option<string>.Some("body");

            var package = new OracleDatabasePackage(packageName, specification, body);

            Assert.AreEqual(specification, package.Specification);
        }

        [Test]
        public static void Body_PropertyGetWhenCtorGivenNone_EqualsCtorArg()
        {
            const string packageName = "test_package";
            const string specification = "spec";
            var body = Option<string>.None;

            var package = new OracleDatabasePackage(packageName, specification, body);

            Assert.AreEqual(body, package.Body);
        }

        [Test]
        public static void Body_GivenValueForCtorArgAndPropertyGet_EqualsCtorArg()
        {
            const string packageName = "test_package";
            const string specification = "spec";
            var body = Option<string>.Some("body");

            var package = new OracleDatabasePackage(packageName, specification, body);

            Assert.AreEqual(body.UnwrapSome(), package.Body.UnwrapSome());
        }
    }
}
