using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

        public static IEnumerable<string> OrderNamespaces(this IEnumerable<string> namespaces)
        {
            if (namespaces == null)
                throw new ArgumentNullException(nameof(namespaces));

            var system = new List<string>();
            var nonSystem = new List<string>();

            foreach (var ns in namespaces)
            {
                var group = ns == "System" || ns.StartsWith("System.")
                    ? system
                    : nonSystem;
                group.Add(ns);
            }

            return system.OrderBy(n => n)
                .Concat(nonSystem.OrderBy(n => n))
                .ToList();
        }
    }
}
