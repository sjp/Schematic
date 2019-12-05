using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SJP.Schematic.DataAccess.CodeGeneration
{
    public static class SyntaxUtilities
    {
        public static EqualsValueClauseSyntax NotNullDefault { get; } = EqualsValueClause(
            PostfixUnaryExpression(
                SyntaxKind.SuppressNullableWarningExpression,
                LiteralExpression(
                    SyntaxKind.DefaultLiteralExpression,
                    Token(SyntaxKind.DefaultKeyword))));

        public static AccessorListSyntax PropertyGetSetDeclaration { get; } = AccessorList(
            new SyntaxList<AccessorDeclarationSyntax>(new[]
            {
                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            })
        );

        public static IdentifierNameSyntax AttributeName(string attributeName)
        {
            var trimmedName = !attributeName.EndsWith(AttributeSuffix)
                ? attributeName
                : attributeName.Substring(0, attributeName.Length - AttributeSuffix.Length);

            return IdentifierName(trimmedName);
        }

        private const string AttributeSuffix = "Attribute";

        public static SyntaxTriviaList BuildCommentTrivia(string comment)
        {
            if (comment == null)
                throw new ArgumentNullException(nameof(comment));

            var commentLines = GetLines(comment);
            var commentNodes = commentLines.Count > 1
                ? commentLines.SelectMany(l => new XmlNodeSyntax[] { XmlParaElement(XmlText(l)), XmlText(XmlNewline) }).ToArray()
                : new XmlNodeSyntax[] { XmlText(XmlTextLiteral(comment), XmlNewline) };
            // add a newline after the summary element
            var formattedCommentNodes = new XmlNodeSyntax[] { XmlText(XmlNewline) }.Concat(commentNodes).ToArray();

            return new SyntaxTriviaList(
                Trivia(
                    DocumentationComment(
                        XmlSummaryElement(formattedCommentNodes))),
                ElasticCarriageReturnLineFeed
            );
        }

        public static SyntaxTriviaList BuildCommentTrivia(IEnumerable<XmlNodeSyntax> commentNodes)
        {
            if (commentNodes == null)
                throw new ArgumentNullException(nameof(commentNodes));

            var commentsWithNewlines = new XmlNodeSyntax[] { XmlText(XmlNewline) }
                .Concat(commentNodes)
                .Concat(new XmlNodeSyntax[] { XmlText(XmlNewline) })
                .ToArray();

            return new SyntaxTriviaList(
                Trivia(
                    DocumentationComment(
                        XmlSummaryElement(commentsWithNewlines))),
                ElasticCarriageReturnLineFeed
            );
        }

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

            var lastParamIndex = paramNodes.Count -1;
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

            return new SyntaxTriviaList(
                Trivia(
                    DocumentationComment(combinedSyntaxNodes)),
                ElasticCarriageReturnLineFeed
            );
        }

        public static readonly IReadOnlyDictionary<string, TypeSyntax> TypeSyntaxMap = new Dictionary<string, TypeSyntax>
        {
            ["Boolean"] = PredefinedType(Token(SyntaxKind.BoolKeyword)),
            ["Byte"] = PredefinedType(Token(SyntaxKind.ByteKeyword)),
            ["Byte[]"] = ArrayType(
                PredefinedType(
                    Token(SyntaxKind.ByteKeyword)
                ),
                SingletonList(ArrayRankSpecifier())
            ),
            ["SByte"] = PredefinedType(Token(SyntaxKind.SByteKeyword)),
            ["Char"] = PredefinedType(Token(SyntaxKind.CharKeyword)),
            ["Decimal"] = PredefinedType(Token(SyntaxKind.DecimalKeyword)),
            ["Double"] = PredefinedType(Token(SyntaxKind.DoubleKeyword)),
            ["Single"] = PredefinedType(Token(SyntaxKind.FloatKeyword)),
            ["Int32"] = PredefinedType(Token(SyntaxKind.IntKeyword)),
            ["UInt32"] = PredefinedType(Token(SyntaxKind.UIntKeyword)),
            ["Int64"] = PredefinedType(Token(SyntaxKind.LongKeyword)),
            ["UInt64"] = PredefinedType(Token(SyntaxKind.ULongKeyword)),
            ["Object"] = PredefinedType(Token(SyntaxKind.ObjectKeyword)),
            ["Int16"] = PredefinedType(Token(SyntaxKind.ShortKeyword)),
            ["UInt16"] = PredefinedType(Token(SyntaxKind.UShortKeyword)),
            ["String"] = PredefinedType(Token(SyntaxKind.StringKeyword))
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
