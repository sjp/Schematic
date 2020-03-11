using NUnit.Framework;
using LanguageExt;
using SJP.Schematic.Tests.Utilities;

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

            Assert.That(() => new OracleDatabasePackage(null, specification, body), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullSpecification_ThrowsArgumentNullException()
        {
            const string packageName = "test_package";
            var body = Option<string>.Some("body");

            Assert.That(() => new OracleDatabasePackage(packageName, null, body), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptySpecification_ThrowsArgumentNullException()
        {
            const string packageName = "test_package";
            var body = Option<string>.Some("body");

            Assert.That(() => new OracleDatabasePackage(packageName, string.Empty, body), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceSpecification_ThrowsArgumentNullException()
        {
            const string packageName = "test_package";
            const string specification = "    ";
            var body = Option<string>.Some("body");

            Assert.That(() => new OracleDatabasePackage(packageName, specification, body), Throws.ArgumentNullException);
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            const string packageName = "test_package";
            const string specification = "spec";
            var body = Option<string>.Some("body");

            var package = new OracleDatabasePackage(packageName, specification, body);

            Assert.That(package.Name.LocalName, Is.EqualTo(packageName));
        }

        [Test]
        public static void Specification_PropertyGet_EqualsCtorArg()
        {
            const string packageName = "test_package";
            const string specification = "spec";
            var body = Option<string>.Some("body");

            var package = new OracleDatabasePackage(packageName, specification, body);

            Assert.That(package.Specification, Is.EqualTo(specification));
        }

        [Test]
        public static void Body_PropertyGetWhenCtorGivenNone_EqualsCtorArg()
        {
            const string packageName = "test_package";
            const string specification = "spec";
            var body = Option<string>.None;

            var package = new OracleDatabasePackage(packageName, specification, body);

            Assert.That(package.Body, Is.EqualTo(body));
        }

        [Test]
        public static void Body_GivenValueForCtorArgAndPropertyGet_EqualsCtorArg()
        {
            const string packageName = "test_package";
            const string specification = "spec";
            var body = Option<string>.Some("body");

            var package = new OracleDatabasePackage(packageName, specification, body);

            Assert.That(package.Body.UnwrapSome(), Is.EqualTo(body.UnwrapSome()));
        }
    }
}
