using System;
using NUnit.Framework;
using SJP.Schematic.Lint.Naming;

namespace SJP.Schematic.Lint.Tests.Naming;

internal static class NameTokenizerTests
{
    [Test]
    public static void Tokenize_GivenNullName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => NameTokenizer.Tokenize(null!),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("name")
        );
    }

    [TestCase("shipping_adress", "shipping,adress")]
    [TestCase("shipping-adress", "shipping,adress")]
    [TestCase("shipping adress", "shipping,adress")]
    [TestCase("dbo.orders", "dbo,orders")]
    [TestCase("ReceivedDte", "received,dte")]
    [TestCase("receivedDte", "received,dte")]
    [TestCase("XMLHttpRequest", "xml,http,request")]
    [TestCase("ABCDefGHIjk", "abc,def,gh,ijk")]
    [TestCase("aBCd", "a,b,cd")]
    [TestCase("ABC1def", "abc,1,def")]
    [TestCase("A1B", "a,1,b")]
    [TestCase("  order   line  ", "order,line")]
    [TestCase("__shipping__address__", "shipping,address")]
    [TestCase("address1", "address,1")]
    [TestCase("v2Address", "v,2,address")]
    [TestCase("ADDRESS", "address")]
    [TestCase("___", "")]
    [TestCase("", "")]
    public static void Tokenize_GivenName_SplitsItIntoExpectedWords(string name, string expected)
    {
        var expectedWords = expected.Length == 0 ? [] : expected.Split(',');

        Assert.That(NameTokenizer.Tokenize(name), Is.EqualTo(expectedWords));
    }
}
