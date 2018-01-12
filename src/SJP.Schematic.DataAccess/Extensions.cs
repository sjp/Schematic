using System;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace SJP.Schematic.DataAccess.Extensions
{
    public static class NumericExtensions
    {
        public static string ToNumericLiteral(this int input)
        {
            var literal = SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                SyntaxFactory.Literal(input)
            );

            return literal.ToFullString();
        }

        public static string ToNumericLiteral(this uint input)
        {
            var literal = SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                SyntaxFactory.Literal(input)
            );

            return literal.ToFullString();
        }

        public static string ToNumericLiteral(this long input)
        {
            var literal = SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                SyntaxFactory.Literal(input)
            );

            return literal.ToFullString();
        }

        public static string ToNumericLiteral(this ulong input)
        {
            var literal = SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                SyntaxFactory.Literal(input)
            );

            return literal.ToFullString();
        }

        public static string ToNumericLiteral(this float input)
        {
            var literal = SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                SyntaxFactory.Literal(input)
            );

            return literal.ToFullString();
        }

        public static string ToNumericLiteral(this double input)
        {
            var literal = SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                SyntaxFactory.Literal(input)
            );

            return literal.ToFullString();
        }

        public static string ToNumericLiteral(this decimal input)
        {
            var literal = SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression,
                SyntaxFactory.Literal(input)
            );

            return literal.ToFullString();
        }
    }

    public static class StringExtensions
    {
        public static string ToStringLiteral(this string input)
        {
            if (input == null)
                return input;

            var literal = SyntaxFactory.LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                SyntaxFactory.Literal(input)
            );

            return literal.ToFullString();
        }
    }

    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendComment(this StringBuilder builder, string indent, string comment)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (indent == null)
                throw new ArgumentNullException(nameof(indent));
            if (comment == null)
                throw new ArgumentNullException(nameof(comment));

            return builder.Append(indent)
                .AppendLine("/// <summary>")
                .Append(indent)
                .Append("/// ")
                .AppendLine(comment)
                .Append(indent)
                .AppendLine("/// </summary>");
        }
    }
}
