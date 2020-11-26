using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SJP.Schematic.DataAccess.CodeGeneration
{
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
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
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
                : attributeName.Substring(0, attributeName.Length - AttributeSuffix.Length);

            return IdentifierName(trimmedName);
        }

        private const string AttributeSuffix = "Attribute";

        /// <summary>
        /// Constructs a documentation comment definition for use with Roslyn.
        /// </summary>
        /// <param name="comment">A comment.</param>
        /// <returns>Syntax nodes that represent the comment.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="comment"/> is <c>null</c>.</exception>
        public static SyntaxTriviaList BuildCommentTrivia(string comment)
        {
            if (comment == null)
                throw new ArgumentNullException(nameof(comment));

            var commentLines = GetLines(comment);
            var commentNodes = commentLines.Count > 1
                ? commentLines.SelectMany(static l => new XmlNodeSyntax[] { XmlParaElement(XmlText(l)), XmlText(XmlNewline) }).ToArray()
                : new XmlNodeSyntax[] { XmlText(XmlTextLiteral(comment), XmlNewline) };
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
        /// Constructs a documentation comment definition for use with Roslyn.
        /// </summary>
        /// <param name="commentNodes">Comment nodes.</param>
        /// <returns>Syntax nodes that represent the comment.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="commentNodes"/> is <c>null</c>.</exception>
        public static SyntaxTriviaList BuildCommentTrivia(IEnumerable<XmlNodeSyntax> commentNodes)
        {
            if (commentNodes == null)
                throw new ArgumentNullException(nameof(commentNodes));

            var commentsWithNewlines = new XmlNodeSyntax[] { XmlText(XmlNewline) }
                .Concat(commentNodes)
                .Concat(new XmlNodeSyntax[] { XmlText(XmlNewline) })
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
        /// <exception cref="ArgumentNullException"><paramref name="commentNodes"/> or <paramref name="paramNodes"/> are <c>null</c>.</exception>
        public static SyntaxTriviaList BuildCommentTriviaWithParams(IEnumerable<XmlNodeSyntax> commentNodes, IReadOnlyDictionary<string, IEnumerable<XmlNodeSyntax>> paramNodes)
        {
            if (commentNodes == null)
                throw new ArgumentNullException(nameof(commentNodes));
            if (paramNodes == null)
                throw new ArgumentNullException(nameof(paramNodes));

            var commentsWithNewlines = new XmlNodeSyntax[] { XmlText(XmlNewline) }
                .Concat(commentNodes)
                .Concat(new XmlNodeSyntax[] { XmlText(XmlNewline) })
                .ToArray();

            var summarySyntaxNode = XmlSummaryElement(commentsWithNewlines);

            var lastParamIndex = paramNodes.Count - 1;
            var paramSyntaxNodes = paramNodes
                .SelectMany((kv, i) =>
                {
                    var nodes = new List<XmlNodeSyntax>
                    {
                        XmlText(XmlNewline),
                        XmlParamElement(kv.Key, kv.Value.ToArray())
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
        public static readonly IReadOnlyDictionary<string, TypeSyntax> TypeSyntaxMap = new Dictionary<string, TypeSyntax>(StringComparer.Ordinal)
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
            [nameof(String)] = PredefinedType(Token(SyntaxKind.StringKeyword))
        };

        private static IReadOnlyCollection<string> GetLines(string comment)
        {
            if (comment == null)
                throw new ArgumentNullException(nameof(comment));

            return comment.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static readonly SyntaxToken XmlNewline = XmlTextNewLine(Environment.NewLine);
    }
}
