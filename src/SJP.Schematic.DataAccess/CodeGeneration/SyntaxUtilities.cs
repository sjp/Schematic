using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SJP.Schematic.Core.Utilities;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SJP.Schematic.DataAccess.CodeGeneration;

/// <summary>
/// Utility methods for generating code with Roslyn.
/// </summary>
public static class SyntaxUtilities
{
    /// <summary>
    /// Returns an assignment expression that generates <c>= default!</c>.
    /// </summary>
    /// <value>A not null default assignment expression.</value>
    public static EqualsValueClauseSyntax NotNullDefault { get; } = EqualsValueClause(
        PostfixUnaryExpression(
            SyntaxKind.SuppressNullableWarningExpression,
            LiteralExpression(
                SyntaxKind.DefaultLiteralExpression,
                Token(SyntaxKind.DefaultKeyword))));

    /// <summary>
    /// Returns an expression that generates <c>{ get; set; }</c>.
    /// </summary>
    /// <value>An auto property expression.</value>
    public static AccessorListSyntax PropertyGetSetDeclaration { get; } = AccessorList(
        List(new[]
        {
            AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
            AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
        })
    );

    /// <summary>
    /// Creates qualified attribute name for use with Roslyn.
    /// </summary>
    /// <param name="attributeName">Name of the attribute.</param>
    /// <returns>An attribute name definition.</returns>
    public static IdentifierNameSyntax AttributeName(string attributeName)
    {
        var trimmedName = !attributeName.EndsWith(AttributeSuffix, StringComparison.Ordinal)
            ? attributeName
            : attributeName[..^AttributeSuffix.Length];

        return IdentifierName(trimmedName);
    }

    private const string AttributeSuffix = "Attribute";

    /// <summary>
    /// Constructs a documentation comment definition for use with Roslyn.
    /// </summary>
    /// <param name="comment">A comment.</param>
    /// <returns>Syntax nodes that represent the comment.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="comment"/> is <see langword="null" />.</exception>
    public static SyntaxTriviaList BuildCommentTrivia(string comment)
    {
        ArgumentNullException.ThrowIfNull(comment);

        var commentLines = GetLines(comment);
        XmlNodeSyntax[] commentNodes = commentLines.Count switch
        {
            0 => [XmlText(XmlTextLiteral(string.Empty), XmlNewline)],
            1 => [XmlText(XmlTextLiteral(SanitizeXmlText(commentLines[0])), XmlNewline)],
            _ => commentLines.SelectMany(static l => new XmlNodeSyntax[] { XmlParaElement(BuildXmlText(l)), XmlText(XmlNewline) }).ToArray()
        };
        // add a newline after the summary element
        var formattedCommentNodes = new XmlNodeSyntax[] { XmlText(XmlNewline) }.Concat(commentNodes).ToArray();

        return TriviaList(
            Trivia(
                DocumentationComment(
                    XmlSummaryElement(formattedCommentNodes))),
            ElasticCarriageReturnLineFeed
        );
    }

    /// <summary>
    /// Constructs an XML text node for use within a documentation comment.
    /// </summary>
    /// <param name="text">Text to embed within a documentation comment, e.g. a database object name.</param>
    /// <returns>An XML text node whose contents are safe to embed within a documentation comment.</returns>
    /// <remarks>Any text sourced from a database should be passed through this method, as it removes characters that would otherwise generate malformed documentation comments.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null" />.</exception>
    public static XmlTextSyntax BuildXmlText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return XmlText(SanitizeXmlText(text));
    }

    /// <summary>
    /// Constructs a documentation comment definition for use with Roslyn.
    /// </summary>
    /// <param name="commentNodes">Comment nodes.</param>
    /// <returns>Syntax nodes that represent the comment.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="commentNodes"/> is <see langword="null" />.</exception>
    public static SyntaxTriviaList BuildCommentTrivia(IEnumerable<XmlNodeSyntax> commentNodes)
    {
        ArgumentNullException.ThrowIfNull(commentNodes);

        var commentsWithNewlines = new XmlNodeSyntax[] { XmlText(XmlNewline) }
            .Concat(commentNodes)
            .Concat([XmlText(XmlNewline)])
            .ToArray();

        return TriviaList(
            Trivia(
                DocumentationComment(
                    XmlSummaryElement(commentsWithNewlines))),
            ElasticCarriageReturnLineFeed
        );
    }

    /// <summary>
    /// Constructs a documentation comment definition for use with Roslyn.
    /// </summary>
    /// <param name="commentNodes">Comment nodes representing method documentation.</param>
    /// <param name="paramNodes">Nodes presenting parameter documentation.</param>
    /// <returns>Syntax nodes that represent the comment.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="commentNodes"/> or <paramref name="paramNodes"/> are <see langword="null" />.</exception>
    public static SyntaxTriviaList BuildCommentTriviaWithParams(IEnumerable<XmlNodeSyntax> commentNodes, IReadOnlyDictionary<string, IEnumerable<XmlNodeSyntax>> paramNodes)
    {
        ArgumentNullException.ThrowIfNull(commentNodes);
        ArgumentNullException.ThrowIfNull(paramNodes);

        var commentsWithNewlines = new XmlNodeSyntax[] { XmlText(XmlNewline) }
            .Concat(commentNodes)
            .Concat([XmlText(XmlNewline)])
            .ToArray();

        var summarySyntaxNode = XmlSummaryElement(commentsWithNewlines);

        var lastParamIndex = paramNodes.Count - 1;
        var paramSyntaxNodes = paramNodes
            .SelectMany((kv, i) =>
            {
                var nodes = new List<XmlNodeSyntax>
                {
                    XmlText(XmlNewline),
                    XmlParamElement(kv.Key, kv.Value.ToArray()),
                };
                if (i != lastParamIndex)
                    nodes.Add(XmlText(XmlNewline));

                return nodes;
            })
            .ToList();
        var combinedSyntaxNodes = new[] { summarySyntaxNode }.Concat(paramSyntaxNodes).ToArray();

        return TriviaList(
            Trivia(
                DocumentationComment(combinedSyntaxNodes)),
            ElasticCarriageReturnLineFeed
        );
    }

    /// <summary>
    /// A type syntax lookup that translates from built-in C# types to Roslyn type definitions.
    /// </summary>
    public static readonly FrozenDictionary<string, TypeSyntax> TypeSyntaxMap = new Dictionary<string, TypeSyntax>(StringComparer.Ordinal)
    {
        [nameof(Boolean)] = PredefinedType(Token(SyntaxKind.BoolKeyword)),
        [nameof(Byte)] = PredefinedType(Token(SyntaxKind.ByteKeyword)),
        ["Byte[]"] = ArrayType(
            PredefinedType(Token(SyntaxKind.ByteKeyword)),
            SingletonList(ArrayRankSpecifier())),
        [nameof(SByte)] = PredefinedType(Token(SyntaxKind.SByteKeyword)),
        [nameof(Char)] = PredefinedType(Token(SyntaxKind.CharKeyword)),
        [nameof(Decimal)] = PredefinedType(Token(SyntaxKind.DecimalKeyword)),
        [nameof(Double)] = PredefinedType(Token(SyntaxKind.DoubleKeyword)),
        [nameof(Single)] = PredefinedType(Token(SyntaxKind.FloatKeyword)),
        [nameof(Int32)] = PredefinedType(Token(SyntaxKind.IntKeyword)),
        [nameof(UInt32)] = PredefinedType(Token(SyntaxKind.UIntKeyword)),
        [nameof(Int64)] = PredefinedType(Token(SyntaxKind.LongKeyword)),
        [nameof(UInt64)] = PredefinedType(Token(SyntaxKind.ULongKeyword)),
        [nameof(Object)] = PredefinedType(Token(SyntaxKind.ObjectKeyword)),
        [nameof(Int16)] = PredefinedType(Token(SyntaxKind.ShortKeyword)),
        [nameof(UInt16)] = PredefinedType(Token(SyntaxKind.UShortKeyword)),
        [nameof(String)] = PredefinedType(Token(SyntaxKind.StringKeyword)),
    }.ToFrozenDictionary(StringComparer.Ordinal);

    /// <summary>
    /// Constructs the type definition used to declare a value of a given CLR type, preferring the built-in C# type alias where one exists.
    /// </summary>
    /// <param name="clrType">A CLR type.</param>
    /// <param name="isNullable">Whether the value being declared is nullable.</param>
    /// <returns>A type definition for use with Roslyn.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="clrType"/> is <see langword="null" />.</exception>
    public static TypeSyntax BuildTypeSyntax(Type clrType, bool isNullable)
    {
        ArgumentNullException.ThrowIfNull(clrType);

        var typeSyntax = string.Equals(clrType.Namespace, nameof(System), StringComparison.Ordinal)
            && TypeSyntaxMap.TryGetValue(clrType.Name, out var predefinedType)
            ? predefinedType
            : ParseTypeName(clrType.FullName!);

        return isNullable
            ? NullableType(typeSyntax)
            : typeSyntax;
    }

    private static IReadOnlyList<string> GetLines(string comment)
    {
        ArgumentNullException.ThrowIfNull(comment);

        var lines = new List<string>();
        foreach (var line in comment.AsSpan().EnumerateLines())
        {
            if (!line.IsEmpty)
                lines.Add(line.ToString());
        }

        return lines;
    }

    /// <summary>
    /// Removes characters that cannot be represented within a documentation comment.
    /// </summary>
    /// <param name="text">Text to sanitize.</param>
    /// <returns>The given text, with line breaks collapsed to spaces and characters that are invalid in XML removed.</returns>
    /// <remarks>
    /// A line break would place the remainder of the text outside of the leading <c>///</c>, causing it to be parsed as code,
    /// while a character that is invalid in XML causes Roslyn to reject the text entirely.
    /// </remarks>
    private static string SanitizeXmlText(string text)
    {
        if (!RequiresSanitization(text))
            return text;

        var builder = StringBuilderCache.Acquire(text.Length);

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c is '\r' or '\n')
            {
                // treat CRLF as one line break rather than two
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                    i++;
                builder.Append(' ');
            }
            else if (i + 1 < text.Length && char.IsSurrogatePair(c, text[i + 1]))
            {
                builder.Append(c).Append(text[i + 1]);
                i++;
            }
            else if (XmlConvert.IsXmlChar(c))
            {
                builder.Append(c);
            }
        }

        return builder.GetStringAndRelease();
    }

    private static bool RequiresSanitization(string text)
    {
        foreach (var c in text)
        {
            // surrogates are only valid when paired, which is determined while sanitizing
            if (c is '\r' or '\n' || char.IsSurrogate(c) || !XmlConvert.IsXmlChar(c))
                return true;
        }

        return false;
    }

    private static readonly SyntaxToken XmlNewline = XmlTextNewLine(Environment.NewLine);
}