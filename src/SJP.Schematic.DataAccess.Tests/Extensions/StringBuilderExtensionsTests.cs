using System;
using System.Text;
using NUnit.Framework;
using SJP.Schematic.DataAccess.Extensions;

namespace SJP.Schematic.DataAccess.Tests
{
    [TestFixture]
    internal static class StringBuilderExtensionsTests
    {
        [Test]
        public static void AppendComment_GivenNullBuilder_ThrowsArgumentNullException()
        {
            const string indent = " ";
            const string comment = " ";

            Assert.Throws<ArgumentNullException>(() => StringBuilderExtensions.AppendComment(null, indent, comment));
        }

        [Test]
        public static void AppendComment_GivenNullIndent_ThrowsArgumentNullException()
        {
            var builder = new StringBuilder();
            const string comment = " ";

            Assert.Throws<ArgumentNullException>(() => builder.AppendComment(null, comment));
        }

        [Test]
        public static void AppendComment_GivenNullComment_ThrowsArgumentNullException()
        {
            var builder = new StringBuilder();
            const string indent = " ";

            Assert.Throws<ArgumentNullException>(() => builder.AppendComment(indent, null));
        }

        [Test]
        public static void AppendComment_GivenSingleLineComment_ReturnsExpectedText()
        {
            var builder = new StringBuilder();
            var indent = string.Empty;
            const string comment = "test";

            const string expected = @"/// <summary>
/// test
/// </summary>
";

            var result = builder.AppendComment(indent, comment).ToString();

            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void AppendComment_GivenSingleLineComment_RetainsIndentation()
        {
            var builder = new StringBuilder();
            var indent = string.Empty;
            const string comment = "    test";

            const string expected = @"/// <summary>
///     test
/// </summary>
";

            var result = builder.AppendComment(indent, comment).ToString();

            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void AppendComment_GivenMultiLineComment_ReturnsExpectedText()
        {
            var builder = new StringBuilder();
            var indent = string.Empty;
            const string comment = @"line 1
line 2
line 3";

            const string expected = @"/// <summary>
/// <para>line 1</para>
/// <para>line 2</para>
/// <para>line 3</para>
/// </summary>
";

            var result = builder.AppendComment(indent, comment).ToString();

            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void AppendComment_GivenMultiLineComment_SkipsMultipleLineBreaks()
        {
            var builder = new StringBuilder();
            var indent = string.Empty;
            const string comment = @"line 1

line 2



line 3";

            const string expected = @"/// <summary>
/// <para>line 1</para>
/// <para>line 2</para>
/// <para>line 3</para>
/// </summary>
";

            var result = builder.AppendComment(indent, comment).ToString();

            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void AppendComment_GivenMultiLineComment_SkipsMultipleLineBreaksRetainingIndentation()
        {
            var builder = new StringBuilder();
            var indent = string.Empty;
            const string comment = @"line 1

    line 2



line 3";

            const string expected = @"/// <summary>
/// <para>line 1</para>
/// <para>    line 2</para>
/// <para>line 3</para>
/// </summary>
";

            var result = builder.AppendComment(indent, comment).ToString();

            Assert.AreEqual(expected, result);
        }
    }
}
