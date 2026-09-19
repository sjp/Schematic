using System;
using System.Threading.Tasks;
using LanguageExt;
using NUnit.Framework;

namespace SJP.Schematic.Tests.Utilities.Tests;

internal static class OptionsExtensionsTests
{
    [Test]
    public static void UnwrapSome_GivenNoneInput_ThrowsArgumentException()
    {
        Assert.That(() => Option<string>.None.UnwrapSome(), Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("input"));
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
        Assert.That(() => OptionAsync<string>.None.UnwrapSomeAsync(), Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("input"));
    }

    [Test]
    public static async Task UnwrapSomeAsync_GivenSomeInput_ReturnsCorrectSomeValue()
    {
        const string expected = "expected";
        var option = OptionAsync<string>.Some(expected);
        var unwrapped = await option.UnwrapSomeAsync();

        Assert.That(unwrapped, Is.EqualTo(expected));
    }
}