using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests.Extensions;

[TestFixture]
internal static class OptionExtensionsTests
{
    [Test]
    public static void FirstSome_GivenNoneInput_ThrowsArgumentNullException()
    {
        Assert.That(() => ((IEnumerable<Option<string>>)null).FirstSome(), Throws.ArgumentNullException);
    }

    [Test]
    public static void FirstSome_GivenNoSomesInput_ReturnsNone()
    {
        var input = new[]
        {
            Option<string>.None,
            Option<string>.None,
            Option<string>.None,
            Option<string>.None,
        };
        var result = input.FirstSome();

        Assert.That(result, OptionIs.None);
    }

    [Test]
    public static void FirstSome_GivenInputWithSomes_ReturnsFirstSome()
    {
        const string expected = "test_1";
        var input = new[]
        {
            Option<string>.None,
            Option<string>.Some("test_1"),
            Option<string>.Some("test_2"),
            Option<string>.None,
        };
        var result = input.FirstSome();
        var unwrapped = result.UnwrapSome();

        Assert.That(unwrapped, Is.EqualTo(expected));
    }

    [Test]
    public static void FirstSome_GivenNoneAsyncInput_ThrowsArgumentNullException()
    {
        Assert.That(() => ((IEnumerable<OptionAsync<string>>)null).FirstSome(), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task FirstSome_GivenNoSomesAsyncInput_ReturnsNone()
    {
        var input = new[]
        {
            OptionAsync<string>.None,
            OptionAsync<string>.None,
            OptionAsync<string>.None,
            OptionAsync<string>.None,
        };
        var result = input.FirstSome();
        var resultIsNone = await result.IsNone;

        Assert.That(resultIsNone, Is.True);
    }

    [Test]
    public static async Task FirstSome_GivenAsyncInputWithSomes_ReturnsFirstSome()
    {
        const string expected = "test_1";
        var input = new[]
        {
            OptionAsync<string>.None,
            OptionAsync<string>.Some("test_1"),
            OptionAsync<string>.Some("test_2"),
            OptionAsync<string>.None,
        };
        var result = input.FirstSome();
        var unwrapped = await result.UnwrapSomeAsync();

        Assert.That(unwrapped, Is.EqualTo(expected));
    }
}