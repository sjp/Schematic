using System.Globalization;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions;

[TestFixture]
internal static class CharExtensionsTests
{
    [Test]
    public static void GetUnicodeCategory_GivenSpaceChar_ReturnsSpaceSeparatorCategory()
    {
        Assert.That(' '.GetUnicodeCategory(), Is.EqualTo(UnicodeCategory.SpaceSeparator));
    }

    [Test]
    public static void GetUnicodeCategory_GivenLowercaseChar_ReturnsLowercaseCategory()
    {
        Assert.That('a'.GetUnicodeCategory(), Is.EqualTo(UnicodeCategory.LowercaseLetter));
    }

    [Test]
    public static void GetUnicodeCategory_GivenUppercaseChar_ReturnsUppercaseCategory()
    {
        Assert.That('A'.GetUnicodeCategory(), Is.EqualTo(UnicodeCategory.UppercaseLetter));
    }

    [Test]
    public static void IsDigit_GivenDigit_ReturnsTrue()
    {
        Assert.That('1'.IsDigit(), Is.True);
    }

    [Test]
    public static void IsDigit_GivenNonDigit_ReturnsFalse()
    {
        Assert.That('a'.IsDigit(), Is.False);
    }

    [Test]
    public static void IsLetter_GivenLetter_ReturnsTrue()
    {
        Assert.That('a'.IsLetter(), Is.True);
    }

    [Test]
    public static void IsLetter_GivenNonLetter_ReturnsFalse()
    {
        Assert.That('1'.IsLetter(), Is.False);
    }

    [Test]
    public static void IsLetterOrDigit_GivenLetter_ReturnsTrue()
    {
        Assert.That('a'.IsLetterOrDigit(), Is.True);
    }

    [Test]
    public static void IsLetterOrDigit_GivenDigit_ReturnsTrue()
    {
        Assert.That('1'.IsLetterOrDigit(), Is.True);
    }

    [Test]
    public static void IsLetterOrDigit_GivenNonDigitOrLetter_ReturnsFalse()
    {
        Assert.That('_'.IsLetterOrDigit(), Is.False);
    }

    [Test]
    public static void IsPunctuation_GivenPunctuation_ReturnsTrue()
    {
        Assert.That(','.IsPunctuation(), Is.True);
    }

    [Test]
    public static void IsLetter_GivenNonPunctuation_ReturnsFalse()
    {
        Assert.That('1'.IsPunctuation(), Is.False);
    }

    [Test]
    public static void IsWhiteSpace_GivenWhiteSpace_ReturnsTrue()
    {
        Assert.That(' '.IsWhiteSpace(), Is.True);
    }

    [Test]
    public static void IsWhiteSpace_GivenNonWhiteSpace_ReturnsFalse()
    {
        Assert.That('1'.IsWhiteSpace(), Is.False);
    }

    [Test]
    public static void ToLowerInvariant_GivenLowercaseChar_ReturnsLowercaseChar()
    {
        Assert.That('a'.ToLowerInvariant(), Is.EqualTo('a'));
    }

    [Test]
    public static void ToLowerInvariant_GivenUppercaseChar_ReturnsLowercaseChar()
    {
        Assert.That('A'.ToLowerInvariant(), Is.EqualTo('a'));
    }

    [Test]
    public static void ToUpperInvariant_GivenLowercaseChar_ReturnsUppercaseChar()
    {
        Assert.That('a'.ToUpperInvariant(), Is.EqualTo('A'));
    }

    [Test]
    public static void ToUpperInvariant_GivenUppercaseChar_ReturnsUppercaseChar()
    {
        Assert.That('A'.ToUpperInvariant(), Is.EqualTo('A'));
    }
}