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
}
