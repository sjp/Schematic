using NUnit.Framework;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests.Pragma
{
    [TestFixture]
    internal static class EncodingExtensionsTests
    {
        [Test]
        public static void AsTextEncoding_GivenInvalidEncoding_ThrowsArgumentException()
        {
            const Encoding encoding = (Encoding)999;
            Assert.That(() => encoding.AsTextEncoding(), Throws.ArgumentException);
        }

        [TestCase(Encoding.Utf8, "Unicode (UTF-8)")]
        [TestCase(Encoding.Utf16, "Unicode")]
        [TestCase(Encoding.Utf16le, "Unicode")]
        [TestCase(Encoding.Utf16be, "Unicode (Big-Endian)")]
        public static void AsTextEncoding_GivenValidEncoding_ReturnsExpectedSysEncoding(Encoding encoding, string expectedEncodingName)
        {
            var sysEncoding = encoding.AsTextEncoding();

            Assert.That(sysEncoding.EncodingName, Is.EqualTo(expectedEncodingName));
        }
    }
}
