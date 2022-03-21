using NUnit.Framework;
using SJP.Schematic.Sqlite.Pragma;

namespace SJP.Schematic.Sqlite.Tests.Pragma;

[TestFixture]
internal static class EncodingExtensionsTests
{
    [Test]
    public static void AsTextEncoding_GivenInvalidEncoding_ThrowsArgumentException()
    {
        const Encoding encoding = (Encoding)999;
        Assert.That(() => encoding.AsTextEncoding(), Throws.ArgumentException);
    }

    [TestCase(Encoding.Utf8, ExpectedResult = "Unicode (UTF-8)")]
    [TestCase(Encoding.Utf16, ExpectedResult = "Unicode")]
    [TestCase(Encoding.Utf16le, ExpectedResult = "Unicode")]
    [TestCase(Encoding.Utf16be, ExpectedResult = "Unicode (Big-Endian)")]
    public static string AsTextEncoding_GivenValidEncoding_ReturnsExpectedSysEncoding(Encoding encoding)
        => encoding.AsTextEncoding().EncodingName;
}