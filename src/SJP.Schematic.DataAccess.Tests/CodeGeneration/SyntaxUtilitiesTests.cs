using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using NUnit.Framework;
using SJP.Schematic.DataAccess.CodeGeneration;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SJP.Schematic.DataAccess.Tests.CodeGeneration;

[TestFixture]
internal static class SyntaxUtilitiesTests
{
    [Test]
    public static void BuildCommentTrivia_GivenNullComment_ThrowsArgumentNullException()
    {
        Assert.That(() => SyntaxUtilities.BuildCommentTrivia((string)null), Throws.ArgumentNullException);
    }

    [Test]
    public static void BuildXmlText_GivenNullText_ThrowsArgumentNullException()
    {
        Assert.That(() => SyntaxUtilities.BuildXmlText(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void BuildCommentTrivia_GivenCommentWithMarkupCharacters_EscapesMarkup()
    {
        var source = BuildSourceWithComment("Amount in EUR & cents, must be < 100 > 0");

        Assert.Multiple(() =>
        {
            Assert.That(source, Does.Contain("Amount in EUR &amp; cents, must be &lt; 100 &gt; 0"));
            Assert.That(GetSyntaxErrors(source), Is.Empty);
        });
    }

    [Test]
    public static void BuildCommentTrivia_GivenCommentContainingClosingSummaryElement_EscapesMarkup()
    {
        var source = BuildSourceWithComment("</summary> public sealed class Injected { } <summary>");

        Assert.Multiple(() =>
        {
            Assert.That(source, Does.Contain("/// &lt;/summary&gt; public sealed class Injected { } &lt;summary&gt;"));
            Assert.That(GetSyntaxErrors(source), Is.Empty);
        });
    }

    [Test]
    public static void BuildCommentTrivia_GivenSingleLineCommentSurroundedByNewlines_DoesNotEscapeCommentTrivia()
    {
        var source = BuildSourceWithComment("\r\ncomment text\n");

        Assert.Multiple(() =>
        {
            Assert.That(source, Does.Contain("/// comment text"));
            Assert.That(GetSyntaxErrors(source), Is.Empty);
        });
    }

    [Test]
    public static void BuildCommentTrivia_GivenSingleLineCommentWithTrailingCodeOnNewline_DoesNotEscapeCommentTrivia()
    {
        var source = BuildSourceWithComment("\npublic sealed class Injected { }");

        Assert.Multiple(() =>
        {
            Assert.That(source, Does.Contain("/// public sealed class Injected { }"));
            Assert.That(GetSyntaxErrors(source), Is.Empty);
        });
    }

    [Test]
    public static void BuildCommentTrivia_GivenMultiLineComment_BuildsParagraphPerLine()
    {
        var source = BuildSourceWithComment("first & line\r\nsecond <line>");

        Assert.Multiple(() =>
        {
            Assert.That(source, Does.Contain("<para>first &amp; line</para>"));
            Assert.That(source, Does.Contain("<para>second &lt;line&gt;</para>"));
            Assert.That(GetSyntaxErrors(source), Is.Empty);
        });
    }

    [Test]
    public static void BuildCommentTrivia_GivenCommentWithInvalidXmlCharacter_RemovesInvalidCharacter()
    {
        var source = BuildSourceWithComment("before\u0001after");

        Assert.Multiple(() =>
        {
            Assert.That(source, Does.Contain("/// beforeafter"));
            Assert.That(GetSyntaxErrors(source), Is.Empty);
        });
    }

    [Test]
    public static void BuildCommentTrivia_GivenEmptyComment_BuildsEmptySummary()
    {
        var source = BuildSourceWithComment(string.Empty);

        Assert.That(GetSyntaxErrors(source), Is.Empty);
    }

    [Test]
    public static void BuildXmlText_GivenTextWithMarkupCharacters_EscapesMarkup()
    {
        var result = SyntaxUtilities.BuildXmlText("a & b < c > d").ToFullString();

        Assert.That(result, Is.EqualTo("a &amp; b &lt; c &gt; d"));
    }

    [Test]
    public static void BuildXmlText_GivenTextWithNewlines_ReplacesNewlinesWithSpaces()
    {
        var result = SyntaxUtilities.BuildXmlText("first\r\nsecond\nthird\rfourth").ToFullString();

        Assert.That(result, Is.EqualTo("first second third fourth"));
    }

    [Test]
    public static void BuildXmlText_GivenTextWithInvalidXmlCharacter_RemovesInvalidCharacter()
    {
        var result = SyntaxUtilities.BuildXmlText("before\u0001after").ToFullString();

        Assert.That(result, Is.EqualTo("beforeafter"));
    }

    [Test]
    public static void BuildXmlText_GivenTextWithSupplementaryCharacter_PreservesSupplementaryCharacter()
    {
        const string input = "a \U0001F600 b\nc";

        var result = SyntaxUtilities.BuildXmlText(input).ToFullString();

        Assert.That(result, Is.EqualTo("a \U0001F600 b c"));
    }

    [Test]
    public static void BuildXmlText_GivenTextWithoutInvalidCharacters_PreservesText()
    {
        const string input = "a plain name";

        var result = SyntaxUtilities.BuildXmlText(input).ToFullString();

        Assert.That(result, Is.EqualTo(input));
    }

    [Test]
    public static void BuildCommentTrivia_GivenNameNodeWithNewline_DoesNotEscapeCommentTrivia()
    {
        var trivia = SyntaxUtilities.BuildCommentTrivia(
        [
            XmlText("A mapping class to query the "),
            XmlElement("c", SingletonList<XmlNodeSyntax>(SyntaxUtilities.BuildXmlText("evil\npublic sealed class Injected { } //"))),
            XmlText(" table."),
        ]);
        var source = BuildSource(trivia);

        Assert.Multiple(() =>
        {
            Assert.That(source, Does.Contain("<c>evil public sealed class Injected { } //</c>"));
            Assert.That(GetSyntaxErrors(source), Is.Empty);
        });
    }

    private static string BuildSourceWithComment(string comment) => BuildSource(SyntaxUtilities.BuildCommentTrivia(comment));

    private static string BuildSource(SyntaxTriviaList trivia)
    {
        var classDeclaration = ClassDeclaration("TestClass")
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
            .WithLeadingTrivia(trivia);

        var document = CompilationUnit()
            .WithMembers(
                SingletonList<MemberDeclarationSyntax>(
                    NamespaceDeclaration(ParseName("TestNamespace"))
                        .WithMembers(SingletonList<MemberDeclarationSyntax>(classDeclaration))));

        using var workspace = new AdhocWorkspace();
        return Formatter.Format(document, workspace).ToFullString();
    }

    private static Diagnostic[] GetSyntaxErrors(string source)
    {
        var parseOptions = new CSharpParseOptions(documentationMode: DocumentationMode.Diagnose);

        return CSharpSyntaxTree.ParseText(source, parseOptions)
            .GetDiagnostics()
            .Where(static d => d.Severity >= DiagnosticSeverity.Warning)
            .ToArray();
    }
}
