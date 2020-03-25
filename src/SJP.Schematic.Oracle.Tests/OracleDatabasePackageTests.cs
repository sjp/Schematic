using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
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

        [TestCase("", "test_package", "Package: test_package")]
        [TestCase("test_schema", "test_package", "Package: test_schema.test_package")]
        public static void ToString_WhenInvoked_ReturnsExpectedString(string schema, string localName, string expectedOutput)
        {
            var packageName = Identifier.CreateQualifiedIdentifier(schema, localName);
            const string specification = "spec";
            var body = Option<string>.Some("body");

            var package = new OracleDatabasePackage(packageName, specification, body);

            var result = package.ToString();

            Assert.That(result, Is.EqualTo(expectedOutput));
        }
    }
}
