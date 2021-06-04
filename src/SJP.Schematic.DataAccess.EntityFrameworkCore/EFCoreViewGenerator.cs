using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO.Abstractions;
using System.Linq;
using LanguageExt;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.DataAccess.CodeGeneration;
using SJP.Schematic.DataAccess.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore
{
    /// <summary>
    /// Generate data access classes for views for use with Entity Framework Core.
    /// </summary>
    /// <seealso cref="DatabaseTableGenerator" />
    public class EFCoreViewGenerator : DatabaseViewGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EFCoreViewGenerator"/> class.
        /// </summary>
        /// <param name="fileSystem">A file system.</param>
        /// <param name="nameTranslator">The name translator.</param>
        /// <param name="baseNamespace">The base namespace.</param>
        /// <exception cref="ArgumentNullException"><paramref name="baseNamespace"/> is <c>null</c>, empty, or whitespace.</exception>
        public EFCoreViewGenerator(IFileSystem fileSystem, INameTranslator nameTranslator, string baseNamespace)
            : base(fileSystem, nameTranslator)
        {
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            Namespace = baseNamespace;
        }

        /// <summary>
        /// The namespace to use for the generated classes.
        /// </summary>
        /// <value>A string representing a namespace.</value>
        protected string Namespace { get; }

        /// <summary>
        /// Generates source code that enables interoperability with a given database view for Entity Framework Core.
        /// </summary>
        /// <param name="view">A database view.</param>
        /// <param name="comment">Comment information for the given view.</param>
        /// <returns>A string containing source code to interact with the view.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="view"/> is <c>null</c>.</exception>
        public override string Generate(IDatabaseView view, Option<IDatabaseViewComments> comment)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var schemaNamespace = NameTranslator.SchemaToNamespace(view.Name);
            var viewNamespace = !schemaNamespace.IsNullOrWhiteSpace()
                ? Namespace + "." + schemaNamespace
                : Namespace;

            var namespaces = new[]
                {
                    "System.ComponentModel.DataAnnotations",
                    "System.ComponentModel.DataAnnotations.Schema"
                }
                .Union(
                    view.Columns
                        .Select(static c => c.Type.ClrType.Namespace)
                        .Where(ns => ns != null && !string.Equals(ns, viewNamespace, StringComparison.Ordinal))
                        .Select(static ns => ns!),
                    StringComparer.Ordinal
                )
                .Distinct(StringComparer.Ordinal)
                .OrderNamespaces()
                .ToList();

            var usingStatements = namespaces
                .Select(static ns => ParseName(ns))
                .Select(UsingDirective)
                .ToList();
            var namespaceDeclaration = NamespaceDeclaration(ParseName(viewNamespace));
            var classDeclaration = BuildClass(view, comment);

            var document = CompilationUnit()
                .WithUsings(List(usingStatements))
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        namespaceDeclaration
                            .WithMembers(
                                SingletonList<MemberDeclarationSyntax>(classDeclaration))));

            using var workspace = new AdhocWorkspace();
            return Formatter.Format(document, workspace).ToFullString();
        }

        private RecordDeclarationSyntax BuildClass(IDatabaseView view, Option<IDatabaseViewComments> comment)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var className = NameTranslator.ViewToClassName(view.Name);
            var properties = view.Columns
                .Select(vc => BuildColumn(vc, comment, className))
                .ToList();

            return RecordDeclaration(Token(SyntaxKind.RecordKeyword), className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithLeadingTrivia(BuildViewComment(view.Name, comment))
                .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
                .WithMembers(List<MemberDeclarationSyntax>(properties))
                .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));
        }

        private PropertyDeclarationSyntax BuildColumn(IDatabaseColumn column, Option<IDatabaseViewComments> comment, string className)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (className.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(className));

            var clrType = column.Type.ClrType;
            var propertyName = NameTranslator.ColumnToPropertyName(className, column.Name.LocalName);

            var columnTypeSyntax = column.IsNullable
                ? NullableType(ParseTypeName(clrType.FullName!))
                : ParseTypeName(clrType.FullName!);
            if (string.Equals(clrType.Namespace, nameof(System), StringComparison.Ordinal) && SyntaxUtilities.TypeSyntaxMap.ContainsKey(clrType.Name))
            {
                columnTypeSyntax = column.IsNullable
                    ? NullableType(SyntaxUtilities.TypeSyntaxMap[clrType.Name])
                    : SyntaxUtilities.TypeSyntaxMap[clrType.Name];
            }

            var baseProperty = PropertyDeclaration(
                columnTypeSyntax,
                Identifier(propertyName)
            );

            var columnSyntax = baseProperty
                .AddAttributeLists(BuildColumnAttributes(column, propertyName).ToArray())
                .WithModifiers(SyntaxTokenList.Create(Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(SyntaxUtilities.PropertyGetSetDeclaration)
                .WithLeadingTrivia(BuildColumnComment(column.Name, comment));

            var isNotNullRefType = !column.IsNullable && !column.Type.ClrType.IsValueType;
            if (!isNotNullRefType)
                return columnSyntax;

            return columnSyntax
                .WithInitializer(SyntaxUtilities.NotNullDefault)
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        private static SyntaxTriviaList BuildViewComment(Identifier viewName, Option<IDatabaseViewComments> comment)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return comment
                .Bind(static c => c.Comment)
                .Match(
                    SyntaxUtilities.BuildCommentTrivia,
                    () => SyntaxUtilities.BuildCommentTrivia(new XmlNodeSyntax[]
                    {
                        XmlText("A mapping class to query the "),
                        XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(viewName.LocalName))),
                        XmlText(" view.")
                    })
                );
        }

        private static SyntaxTriviaList BuildColumnComment(Identifier columnName, Option<IDatabaseViewComments> comment)
        {
            if (columnName == null)
                throw new ArgumentNullException(nameof(columnName));

            return comment
                .Bind(c => c.ColumnComments.TryGetValue(columnName, out var cc) ? cc : Option<string>.None)
                .Match(
                    SyntaxUtilities.BuildCommentTrivia,
                    () => SyntaxUtilities.BuildCommentTrivia(new XmlNodeSyntax[]
                    {
                        XmlText("The "),
                        XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(columnName.LocalName))),
                        XmlText(" column.")
                    })
                );
        }

        private static IEnumerable<AttributeListSyntax> BuildColumnAttributes(IDatabaseColumn column, string propertyName)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (propertyName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(propertyName));

            var columnAttributeArgs = new List<AttributeArgumentSyntax>();
            if (!string.Equals(propertyName, column.Name.LocalName, StringComparison.Ordinal))
            {
                var quotedColumnName = LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    Literal(column.Name.LocalName));
                columnAttributeArgs.Add(AttributeArgument(quotedColumnName));
            }
            columnAttributeArgs.Add(
                AttributeArgument(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(nameof(ColumnAttribute.TypeName)),
                        Token(SyntaxKind.EqualsToken),
                        LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal(column.Type.TypeName.LocalName)))));

            return new[]
            {
                AttributeList(
                    SingletonSeparatedList(
                        Attribute(
                            SyntaxUtilities.AttributeName(nameof(ColumnAttribute)),
                            AttributeArgumentList(
                                SeparatedList(columnAttributeArgs)))))
            };
        }
    }
}
