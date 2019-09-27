using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SJP.Schematic.DataAccess.Extensions
{
    public static class StringExtensions
    {
        public static string? ToStringLiteral([NotNullIfNotNull("input")] this string? input)
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
}
