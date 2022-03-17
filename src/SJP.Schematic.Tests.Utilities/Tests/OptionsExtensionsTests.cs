using System.Threading.Tasks;
using LanguageExt;
using NUnit.Framework;

namespace SJP.Schematic.Tests.Utilities.Tests;

[TestFixture]
internal static class OptionsExtensionsTests
{
    [Test]
    public static void UnwrapSome_GivenNoneInput_ThrowsArgumentException()
    {
        Assert.That(() => Option<string>.None.UnwrapSome(), Throws.ArgumentException);
    }

    [Test]
    public static void UnwrapSome_GivenSomeInput_ReturnsCorrectSomeValue()
    {
        const string expected = "expected";
        var option = Option<string>.Some(expected);
        var unwrapped = option.UnwrapSome();

        Assert.That(unwrapped, Is.EqualTo(expected));
    }

    [Test]
    public static void UnwrapSomeAsync_GivenNoneInput_ThrowsArgumentException()
    {
        Assert.That(async () => await OptionAsync<string>.None.UnwrapSomeAsync().ConfigureAwait(false), Throws.ArgumentException);
    }

    [Test]
    public static async Task UnwrapSomeAsync_GivenSomeInput_ReturnsCorrectSomeValue()
    {
        const string expected = "expected";
        var option = OptionAsync<string>.Some(expected);
        var unwrapped = await option.UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(unwrapped, Is.EqualTo(expected));
    }
}
