using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SJP.Schematic.DataAccess.Extensions
{
    public static class SyntaxUtilities
    {
        public static EqualsValueClauseSyntax NotNullDefault { get; } = SyntaxFactory.EqualsValueClause(
            SyntaxFactory.PostfixUnaryExpression(
                SyntaxKind.SuppressNullableWarningExpression,
                SyntaxFactory.LiteralExpression(
                    SyntaxKind.DefaultLiteralExpression,
                    SyntaxFactory.Token(SyntaxKind.DefaultKeyword))));

        public static AccessorListSyntax PropertyGetSetDeclaration { get; } = SyntaxFactory.AccessorList(
            new SyntaxList<AccessorDeclarationSyntax>(new[]
            {
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            })
        );

        public static IdentifierNameSyntax AttributeName(string attributeName)
        {
            var trimmedName = !attributeName.EndsWith(AttributeSuffix)
                ? attributeName
                : attributeName.Substring(0, attributeName.Length - AttributeSuffix.Length);

            return SyntaxFactory.IdentifierName(trimmedName);
        }

        private const string AttributeSuffix = "Attribute";

        public static SyntaxTriviaList BuildCommentTrivia(string comment)
        {
            if (comment == null)
                throw new ArgumentNullException(nameof(comment));

            var commentLines = GetLines(comment);
            var commentNodes = commentLines.Count > 1
                ? commentLines.SelectMany(l => new XmlNodeSyntax[] { SyntaxFactory.XmlParaElement(SyntaxFactory.XmlText(l)), SyntaxFactory.XmlText(XmlNewline) }).ToArray()
                : new XmlNodeSyntax[] { SyntaxFactory.XmlText(SyntaxFactory.XmlTextLiteral(comment), XmlNewline) };
            // add a newline after the summary element
            var formattedCommentNodes = new XmlNodeSyntax[] { SyntaxFactory.XmlText(XmlNewline) }.Concat(commentNodes).ToArray();

            return new SyntaxTriviaList(
                SyntaxFactory.Trivia(
                    SyntaxFactory.DocumentationComment(
                        SyntaxFactory.XmlSummaryElement(formattedCommentNodes))),
                SyntaxFactory.ElasticCarriageReturnLineFeed
            );
        }

        public static SyntaxTriviaList BuildCommentTrivia(IEnumerable<XmlNodeSyntax> commentNodes)
        {
            if (commentNodes == null)
                throw new ArgumentNullException(nameof(commentNodes));

            var commentsWithNewlines = new XmlNodeSyntax[] { SyntaxFactory.XmlText(XmlNewline) }
                .Concat(commentNodes)
                .Concat(new XmlNodeSyntax[] { SyntaxFactory.XmlText(XmlNewline) })
                .ToArray();

            return new SyntaxTriviaList(
                SyntaxFactory.Trivia(
                    SyntaxFactory.DocumentationComment(
                        SyntaxFactory.XmlSummaryElement(commentsWithNewlines))),
                SyntaxFactory.ElasticCarriageReturnLineFeed
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

        private static readonly SyntaxToken XmlNewline = SyntaxFactory.XmlTextNewLine(Environment.NewLine);
    }
}
