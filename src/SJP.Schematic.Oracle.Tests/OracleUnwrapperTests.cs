using System.IO;
using NUnit.Framework;

namespace SJP.Schematic.Oracle.Tests;

[TestFixture]
internal static class OracleUnwrapperTests
{
    [Test]
    public static void TryUnwrap_GivenNullInput_ReturnsFalse()
    {
        var result = OracleUnwrapper.TryUnwrap(null, out _);

        Assert.That(result, Is.False);
    }

    [Test]
    public static void TryUnwrap_GivenNullInput_ReturnsExpectedValue()
    {
        _ = OracleUnwrapper.TryUnwrap(null, out var unwrapped);

        Assert.That(unwrapped, Is.Null);
    }

    [Test]
    public static void TryUnwrap_GivenValidInput_ReturnsTrue()
    {
        var result = OracleUnwrapper.TryUnwrap(WrappedExample, out _);

        Assert.That(result, Is.True);
    }

    [Test]
    public static void TryUnwrap_GivenValidInput_ReturnsExpectedValue()
    {
        _ = OracleUnwrapper.TryUnwrap(WrappedExample, out var unwrapped);

        Assert.That(unwrapped, Is.EqualTo(ExpectedUnwrappedExample).IgnoreLineEndingFormat);
    }

    [Test]
    public static void TryUnwrap_GivenInvalidInput_ReturnsFalse()
    {
        var result = OracleUnwrapper.TryUnwrap(MissingMagicPrefixExample, out _);

        Assert.That(result, Is.False);
    }

    [Test]
    public static void TryUnwrap_GivenInvalidInput_ReturnsExpectedValue()
    {
        _ = OracleUnwrapper.TryUnwrap(MissingMagicPrefixExample, out var unwrapped);

        Assert.That(unwrapped, Is.Null);
    }

    [Test]
    public static void Unwrap_GivenNullInput_ReturnsNull()
    {
        var result = OracleUnwrapper.Unwrap(null);

        Assert.That(result, Is.Null);
    }

    [Test]
    public static void Unwrap_GivenValidInput_ReturnsUnwrappedInput()
    {
        var result = OracleUnwrapper.Unwrap(WrappedExample);

        Assert.That(result, Is.EqualTo(ExpectedUnwrappedExample).IgnoreLineEndingFormat);
    }

    [Test]
    public static void Unwrap_GivenInvalidInput_ReturnsInputUnchanged()
    {
        var result = OracleUnwrapper.Unwrap(MissingMagicPrefixExample);

        Assert.That(result, Is.EqualTo(MissingMagicPrefixExample));
    }

    [Test]
    public static void UnwrapUnsafe_GivenNullInput_ThrowsArgumentNullException()
    {
        Assert.That(() => OracleUnwrapper.UnwrapUnsafe(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void UnwrapUnsafe_GivenEncryptedInput_ReturnsExpectedResult()
    {
        var result = OracleUnwrapper.UnwrapUnsafe(WrappedExample);

        Assert.That(result, Is.EqualTo(ExpectedUnwrappedExample).IgnoreLineEndingFormat);
    }

    [Test]
    public static void UnwrapUnsafe_GivenInvalidInput_ThrowsInvalidDataException()
    {
        Assert.That(() => OracleUnwrapper.UnwrapUnsafe(MissingMagicPrefixExample), Throws.TypeOf<InvalidDataException>());
    }

    [Test]
    public static void UnwrapUnsafe_GivenInputWithIncorrectHash_ThrowsInvalidDataException()
    {
        Assert.That(() => OracleUnwrapper.UnwrapUnsafe(InvalidHashExample), Throws.TypeOf<InvalidDataException>());
    }

    [Test]
    public static void IsWrappedDefinition_GivenNullInput_ThrowsArgumentNullException()
    {
        Assert.That(() => OracleUnwrapper.IsWrappedDefinition(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void IsWrappedDefinition_GivenEncryptedInput_ReturnsTrue()
    {
        var isWrapped = OracleUnwrapper.IsWrappedDefinition(WrappedExample);

        Assert.That(isWrapped, Is.True);
    }

    [Test]
    public static void IsWrappedDefinition_GivenInputMissingWrappedKeyword_ReturnsFalse()
    {
        var isWrapped = OracleUnwrapper.IsWrappedDefinition(MissingWrappedKeywordExample);

        Assert.That(isWrapped, Is.False);
    }

    [Test]
    public static void IsWrappedDefinition_GivenInputEndingBeforeMagicPrefix_ReturnsFalse()
    {
        var isWrapped = OracleUnwrapper.IsWrappedDefinition(EndsBeforeMagicPrefixExample);

        Assert.That(isWrapped, Is.False);
    }

    [Test]
    public static void IsWrappedDefinition_GivenInputMissingMagicPrefix_ReturnsFalse()
    {
        var isWrapped = OracleUnwrapper.IsWrappedDefinition(MissingMagicPrefixExample);

        Assert.That(isWrapped, Is.False);
    }

    [Test]
    public static void IsWrappedDefinition_GivenInputEndingBeforeHexPrefix_ReturnsFalse()
    {
        var isWrapped = OracleUnwrapper.IsWrappedDefinition(EndsBeforeHexPrefixExample);

        Assert.That(isWrapped, Is.False);
    }

    [Test]
    public static void IsWrappedDefinition_GivenInputWithNonHexPrefix_ReturnsFalse()
    {
        var isWrapped = OracleUnwrapper.IsWrappedDefinition(NonHexPrefixExample);

        Assert.That(isWrapped, Is.False);
    }

    [Test]
    public static void IsWrappedDefinition_GivenInputMissingHexPrefix_ReturnsFalse()
    {
        var isWrapped = OracleUnwrapper.IsWrappedDefinition(MissingHexPrefixExample);

        Assert.That(isWrapped, Is.False);
    }

    [Test]
    public static void IsWrappedDefinition_GivenInputMissingAbcd_ReturnsFalse()
    {
        var isWrapped = OracleUnwrapper.IsWrappedDefinition(MissingAbcdExample);

        Assert.That(isWrapped, Is.False);
    }

    [Test]
    public static void IsWrappedDefinition_GivenInputEndsBeforeHexSuffix_ReturnsFalse()
    {
        var isWrapped = OracleUnwrapper.IsWrappedDefinition(EndsBeforeHexSuffixExample);

        Assert.That(isWrapped, Is.False);
    }

    [Test]
    public static void IsWrappedDefinition_GivenInputMissingHexSuffix_ReturnsFalse()
    {
        var isWrapped = OracleUnwrapper.IsWrappedDefinition(MissingHexSuffixExample);

        Assert.That(isWrapped, Is.False);
    }

    [Test]
    public static void IsWrappedDefinition_GivenInputEndsBeforeLengthHexNumber_ReturnsFalse()
    {
        var isWrapped = OracleUnwrapper.IsWrappedDefinition(EndsBeforeLengthHexNumberExample);

        Assert.That(isWrapped, Is.False);
    }

    [Test]
    public static void IsWrappedDefinition_GivenInputMissingOneHexNumber_ReturnsFalse()
    {
        var isWrapped = OracleUnwrapper.IsWrappedDefinition(MissingOneHexNumberExample);

        Assert.That(isWrapped, Is.False);
    }

    [Test]
    public static void IsWrappedDefinition_GivenInputMissingLengthHex_ReturnsFalse()
    {
        var isWrapped = OracleUnwrapper.IsWrappedDefinition(MissingLengthHexExample);

        Assert.That(isWrapped, Is.False);
    }

    [Test]
    public static void IsWrappedDefinition_GivenInputWithInvalidLengthHex_ReturnsFalse()
    {
        var isWrapped = OracleUnwrapper.IsWrappedDefinition(LengthHexNumberInvalidExample);

        Assert.That(isWrapped, Is.False);
    }

    [Test]
    public static void IsWrappedDefinition_GivenInputMissingBase64_ReturnsFalse()
    {
        var isWrapped = OracleUnwrapper.IsWrappedDefinition(MissingBase64Example);

        Assert.That(isWrapped, Is.False);
    }

    [Test]
    public static void IsWrappedDefinition_GivenInputWithInvalidBase64_ReturnsFalse()
    {
        var isWrapped = OracleUnwrapper.IsWrappedDefinition(InvalidBase64Example);

        Assert.That(isWrapped, Is.False);
    }

    private const string WrappedExample = @"PROCEDURE wrap_it wrapped
a000000
b2
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
7
e0 f7
uveH+zmPD1snPwtBZmL3T1hkxRcwgy7w154VfC9GAME+MzyOFMq0DNYH29gBazqoJJQ0Xemb
pdRvlWrC2UeKeKiS2uzT80HMAvKIMOYhjXZT4CrU98zgprrwl4jKnFKvFljUAnGx8GHexDSU
XRa3oykCJIUWEovu72mqAm0vttgZB9E/9E6y2HhxKdu1k8arcrHegHYAvF1pwn1e6sCFJg04
QGsN1g1JLYIklPGBDEEZInWt0w==
";

    private const string ExpectedUnwrappedExample = @"PROCEDURE WRAP_IT (SEED_IN NUMBER)
IS
  V_RAND INTEGER;
BEGIN
  DBMS_RANDOM.INITIALIZE (SEED_IN);
  FOR I IN 1..5 LOOP
   V_RAND := MOD(ABS(DBMS_RANDOM.RANDOM),45);
   DBMS_OUTPUT.PUT_LINE(I||': '||V_RAND);
  END LOOP;
END;";

    // missing wrapped keyword
    private const string InvalidHashExample = @"PROCEDURE wrap_it wrapped
a000000
b2
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
7
e0 f7
uveH+zmPD2snPwtBZmL3T1hkxRcwgy7w154VfC9GAME+MzyOFMq0DNYH29gBazqoJJQ0Xemb
pdRvlWrC2UeKeKiS2uzT80HMAvKIMOYhjXZT4CrU98zgprrwl4jKnFKvFljUAnGx8GHexDSU
XRa3oykCJIUWEovu72mqAm0vttgZB9E/9E6y2HhxKdu1k8arcrHegHYAvF1pwn1e6sCFJg04
QGsN1g1JLYIklPGBDEEZInWt0w==
";

    // missing wrapped keyword
    private const string MissingWrappedKeywordExample = @"PROCEDURE wrap_it
a000000
b2
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
7
e0 f7
uveH+zmPD1snPwtBZmL3T1hkxRcwgy7w154VfC9GAME+MzyOFMq0DNYH29gBazqoJJQ0Xemb
pdRvlWrC2UeKeKiS2uzT80HMAvKIMOYhjXZT4CrU98zgprrwl4jKnFKvFljUAnGx8GHexDSU
XRa3oykCJIUWEovu72mqAm0vttgZB9E/9E6y2HhxKdu1k8arcrHegHYAvF1pwn1e6sCFJg04
QGsN1g1JLYIklPGBDEEZInWt0w==
";

    private const string EndsBeforeMagicPrefixExample = "PROCEDURE wrap_it wrapped";

    // contains missing a000000 line
    private const string MissingMagicPrefixExample = @"PROCEDURE wrap_it wrapped
b2
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
7
e0 f7
uveH+zmPD1snPwtBZmL3T1hkxRcwgy7w154VfC9GAME+MzyOFMq0DNYH29gBazqoJJQ0Xemb
pdRvlWrC2UeKeKiS2uzT80HMAvKIMOYhjXZT4CrU98zgprrwl4jKnFKvFljUAnGx8GHexDSU
XRa3oykCJIUWEovu72mqAm0vttgZB9E/9E6y2HhxKdu1k8arcrHegHYAvF1pwn1e6sCFJg04
QGsN1g1JLYIklPGBDEEZInWt0w==
";

    // missing hex number below magic prefix
    private const string MissingHexPrefixExample = @"PROCEDURE wrap_it wrapped
a000000
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
7
e0 f7
uveH+zmPD1snPwtBZmL3T1hkxRcwgy7w154VfC9GAME+MzyOFMq0DNYH29gBazqoJJQ0Xemb
pdRvlWrC2UeKeKiS2uzT80HMAvKIMOYhjXZT4CrU98zgprrwl4jKnFKvFljUAnGx8GHexDSU
XRa3oykCJIUWEovu72mqAm0vttgZB9E/9E6y2HhxKdu1k8arcrHegHYAvF1pwn1e6sCFJg04
QGsN1g1JLYIklPGBDEEZInWt0w==
";

    private const string EndsBeforeHexPrefixExample = @"PROCEDURE wrap_it wrapped
a000000";

    // non hex number below magic prefix
    private const string NonHexPrefixExample = @"PROCEDURE wrap_it wrapped
a000000
hello
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
7
e0 f7
uveH+zmPD1snPwtBZmL3T1hkxRcwgy7w154VfC9GAME+MzyOFMq0DNYH29gBazqoJJQ0Xemb
pdRvlWrC2UeKeKiS2uzT80HMAvKIMOYhjXZT4CrU98zgprrwl4jKnFKvFljUAnGx8GHexDSU
XRa3oykCJIUWEovu72mqAm0vttgZB9E/9E6y2HhxKdu1k8arcrHegHYAvF1pwn1e6sCFJg04
QGsN1g1JLYIklPGBDEEZInWt0w==
";

    // contains missing abcd lines
    private const string MissingAbcdExample = @"PROCEDURE wrap_it wrapped
a000000
b2
abcd
abcd
abcd
abcd
7
e0 f7
uveH+zmPD1snPwtBZmL3T1hkxRcwgy7w154VfC9GAME+MzyOFMq0DNYH29gBazqoJJQ0Xemb
pdRvlWrC2UeKeKiS2uzT80HMAvKIMOYhjXZT4CrU98zgprrwl4jKnFKvFljUAnGx8GHexDSU
XRa3oykCJIUWEovu72mqAm0vttgZB9E/9E6y2HhxKdu1k8arcrHegHYAvF1pwn1e6sCFJg04
QGsN1g1JLYIklPGBDEEZInWt0w==
";

    private const string EndsBeforeHexSuffixExample = @"PROCEDURE wrap_it wrapped
a000000
b2
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd";

    // missing hex number below abcd lines
    private const string MissingHexSuffixExample = @"PROCEDURE wrap_it wrapped
a000000
b2
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
e0 f7
uveH+zmPD1snPwtBZmL3T1hkxRcwgy7w154VfC9GAME+MzyOFMq0DNYH29gBazqoJJQ0Xemb
pdRvlWrC2UeKeKiS2uzT80HMAvKIMOYhjXZT4CrU98zgprrwl4jKnFKvFljUAnGx8GHexDSU
XRa3oykCJIUWEovu72mqAm0vttgZB9E/9E6y2HhxKdu1k8arcrHegHYAvF1pwn1e6sCFJg04
QGsN1g1JLYIklPGBDEEZInWt0w==
";

    private const string EndsBeforeLengthHexNumberExample = @"PROCEDURE wrap_it wrapped
a000000
b2
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
7";

    // missing only one hex number before base64
    private const string MissingOneHexNumberExample = @"PROCEDURE wrap_it wrapped
a000000
b2
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
7
f7
uveH+zmPD1snPwtBZmL3T1hkxRcwgy7w154VfC9GAME+MzyOFMq0DNYH29gBazqoJJQ0Xemb
pdRvlWrC2UeKeKiS2uzT80HMAvKIMOYhjXZT4CrU98zgprrwl4jKnFKvFljUAnGx8GHexDSU
XRa3oykCJIUWEovu72mqAm0vttgZB9E/9E6y2HhxKdu1k8arcrHegHYAvF1pwn1e6sCFJg04
QGsN1g1JLYIklPGBDEEZInWt0w==
";

    private const string LengthHexNumberInvalidExample = @"PROCEDURE wrap_it wrapped
a000000
b2
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
7
f7 hello
uveH+zmPD1snPwtBZmL3T1hkxRcwgy7w154VfC9GAME+MzyOFMq0DNYH29gBazqoJJQ0Xemb
pdRvlWrC2UeKeKiS2uzT80HMAvKIMOYhjXZT4CrU98zgprrwl4jKnFKvFljUAnGx8GHexDSU
XRa3oykCJIUWEovu72mqAm0vttgZB9E/9E6y2HhxKdu1k8arcrHegHYAvF1pwn1e6sCFJg04
QGsN1g1JLYIklPGBDEEZInWt0w==
";

    // missing hex numbers before base64
    private const string MissingLengthHexExample = @"PROCEDURE wrap_it wrapped
a000000
b2
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
7
uveH+zmPD1snPwtBZmL3T1hkxRcwgy7w154VfC9GAME+MzyOFMq0DNYH29gBazqoJJQ0Xemb
pdRvlWrC2UeKeKiS2uzT80HMAvKIMOYhjXZT4CrU98zgprrwl4jKnFKvFljUAnGx8GHexDSU
XRa3oykCJIUWEovu72mqAm0vttgZB9E/9E6y2HhxKdu1k8arcrHegHYAvF1pwn1e6sCFJg04
QGsN1g1JLYIklPGBDEEZInWt0w==
";

    // missing hex numbers before base64
    private const string MissingBase64Example = @"PROCEDURE wrap_it wrapped
a000000
b2
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
7
e0 f7";

    // missing hex numbers before base64
    private const string InvalidBase64Example = @"PROCEDURE wrap_it wrapped
a000000
b2
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
abcd
7
e0 f7
uveH+zmPD1snPwtBZmL3ThkxRcwgy7w154VfC9GAME+MzyOFMq0DNYH29gBazqoJJQ0Xemb
pdRvlWrC2UeKeKiS2uzT80HMAvKIMOYhjXZT4CrU98zgprrwl4jKnFKvFljUAnGx8GHexDSU
XRa3oykCJIUWEovu72mqAm0vttgZB9E/9E6y2HhxKdu1k8arcrHegHYAvF1pwn1e6sCFJg04
QGsN1g1JLYIklPGBDEEZInWt0w==
";
}