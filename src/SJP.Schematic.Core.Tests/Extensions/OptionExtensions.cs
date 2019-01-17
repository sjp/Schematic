using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions
{
    [TestFixture]
    internal static class OptionExtensionsTests
    {
        [Test]
        public static void UnwrapSome_GivenNoneInput_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Option<string>.None.UnwrapSome());
        }

        [Test]
        public static void UnwrapSome_GivenSomeInput_ReturnsCorrectSomeValue()
        {
            const string expected = "expected";
            var option = Option<string>.Some(expected);
            var unwrapped = option.UnwrapSome();

            Assert.AreEqual(expected, unwrapped);
        }

        [Test]
        public static void UnwrapSomeAsync_GivenNoneInput_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(() => OptionAsync<string>.None.UnwrapSomeAsync());
        }

        [Test]
        public static async Task UnwrapSomeAsync_GivenSomeInput_ReturnsCorrectSomeValue()
        {
            const string expected = "expected";
            var option = OptionAsync<string>.Some(expected);
            var unwrapped = await option.UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expected, unwrapped);
        }

        [Test]
        public static void FirstSome_GivenNoneInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ((IEnumerable<Option<string>>)null).FirstSome());
        }

        [Test]
        public static void FirstSome_GivenNoSomesInput_ReturnsNone()
        {
            var input = new[]
            {
                Option<string>.None,
                Option<string>.None,
                Option<string>.None,
                Option<string>.None
            };
            var result = input.FirstSome();

            Assert.IsTrue(result.IsNone);
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
                Option<string>.None
            };
            var result = input.FirstSome();
            var unwrapped = result.UnwrapSome();

            Assert.AreEqual(expected, unwrapped);
        }

        [Test]
        public static void FirstSome_GivenNoneAsyncInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ((IEnumerable<OptionAsync<string>>)null).FirstSome());
        }

        [Test]
        public static async Task FirstSome_GivenNoSomesAsyncInput_ReturnsNone()
        {
            var input = new[]
            {
                OptionAsync<string>.None,
                OptionAsync<string>.None,
                OptionAsync<string>.None,
                OptionAsync<string>.None
            };
            var result = input.FirstSome();
            var resultIsNone = await result.IsNone.ConfigureAwait(false);

            Assert.IsTrue(resultIsNone);
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
                OptionAsync<string>.None
            };
            var result = input.FirstSome();
            var unwrapped = await result.UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expected, unwrapped);
        }
    }
}
