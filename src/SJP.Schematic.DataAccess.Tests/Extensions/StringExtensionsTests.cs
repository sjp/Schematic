using NUnit.Framework;
using SJP.Schematic.DataAccess.Extensions;

namespace SJP.Schematic.DataAccess.Tests.Extensions;

[TestFixture]
internal static class StringExtensionsTests
{
    [Test]
    public static void Pascalize_GivenNullInput_ThrowsArgumentNullException()
    {
        Assert.That(() => StringExtensions.Pascalize(null), Throws.ArgumentNullException);
    }

    [TestCase("customer", "Customer")]
    [TestCase("CUSTOMER", "CUSTOMER")]
    [TestCase("CUStomer", "CUStomer")]
    [TestCase("customer_name", "CustomerName")]
    [TestCase("customer_first_name", "CustomerFirstName")]
    [TestCase("customer_first_name goes here", "CustomerFirstNameGoesHere")]
    [TestCase("customer name", "CustomerName")]
    [TestCase("customer   name", "CustomerName")]
    public static void Pascalize(string input, string expectedOutput)
    {
        Assert.That(input.Pascalize(), Is.EqualTo(expectedOutput));
    }

    [Test]
    public static void Camelize_GivenNullInput_ThrowsArgumentNullException()
    {
        Assert.That(() => StringExtensions.Camelize(null), Throws.ArgumentNullException);
    }

    [TestCase("customer", "customer")]
    [TestCase("CUSTOMER", "cUSTOMER")]
    [TestCase("CUStomer", "cUStomer")]
    [TestCase("customer_name", "customerName")]
    [TestCase("customer_first_name", "customerFirstName")]
    [TestCase("customer_first_name goes here", "customerFirstNameGoesHere")]
    [TestCase("customer name", "customerName")]
    [TestCase("customer   name", "customerName")]
    [TestCase("", "")]
    public static void Camelize(string input, string expectedOutput)
    {
        Assert.That(input.Camelize(), Is.EqualTo(expectedOutput));
    }

    [Test]
    public static void Underscore_GivenNullInput_ThrowsArgumentNullException()
    {
        Assert.That(() => StringExtensions.Underscore(null), Throws.ArgumentNullException);
    }

    [TestCase("SomeTitle", "some_title")]
    [TestCase("someTitle", "some_title")]
    [TestCase("some title", "some_title")]
    [TestCase("some title that will be underscored", "some_title_that_will_be_underscored")]
    [TestCase("SomeTitleThatWillBeUnderscored", "some_title_that_will_be_underscored")]
    [TestCase("SomeForeignWordsLikeÄgyptenÑu", "some_foreign_words_like_ägypten_ñu")]
    [TestCase("Some wordsTo be Underscored", "some_words_to_be_underscored")]
    public static void Underscore(string input, string expectedOuput)
    {
        Assert.That(input.Underscore(), Is.EqualTo(expectedOuput));
    }

    [Test]
    public static void Pluralize_GivenNullInput_ThrowsArgumentNullException()
    {
        Assert.That(() => StringExtensions.Pluralize(null), Throws.ArgumentNullException);
    }

    [TestCase("test", "tests")]
    [TestCase("Name", "Names")]
    [TestCase("database", "databases")]
    public static void Pluralize(string input, string expectedOuput)
    {
        Assert.That(input.Pluralize(), Is.EqualTo(expectedOuput));
    }
}