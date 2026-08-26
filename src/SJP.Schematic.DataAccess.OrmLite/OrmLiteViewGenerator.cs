using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using LanguageExt;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using ServiceStack.DataAnnotations;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.DataAccess.CodeGeneration;
using SJP.Schematic.DataAccess.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SJP.Schematic.DataAccess.OrmLite;

/// <summary>
/// Generate data access classes for views for use with OrmLite.
/// </summary>
/// <seealso cref="DatabaseViewGenerator" />
public class OrmLiteViewGenerator : DatabaseViewGenerator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrmLiteViewGenerator"/> class.
    /// </summary>
    /// <param name="fileSystem">A file system.</param>
    /// <param name="nameTranslator">The name translator.</param>
    /// <param name="baseNamespace">The base namespace.</param>
    /// <exception cref="ArgumentNullException"><paramref name="fileSystem"/>, <paramref name="nameTranslator"/>, or <paramref name="baseNamespace"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="baseNamespace"/> is empty or whitespace.</exception>
    public OrmLiteViewGenerator(IFileSystem fileSystem, INameTranslator nameTranslator, string baseNamespace)
        : base(fileSystem, nameTranslator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseNamespace);

        Namespace = baseNamespace;
    }

    /// <summary>
    /// The namespace to use for the generated classes.
    /// </summary>
    /// <value>A string representing a namespace.</value>
    protected string Namespace { get; }

    /// <summary>
    /// Generates source code that enables interoperability with a given database view for OrmLite.
    /// </summary>
    /// <param name="view">A database view.</param>
    /// <param name="comment">Comment information for the given view.</param>
    /// <returns>A string containing source code to interact with the view.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="view"/> is <see langword="null" />.</exception>
    public override string Generate(IDatabaseView view, Option<IDatabaseViewComments> comment)
    {
        ArgumentNullException.ThrowIfNull(view);

        var schemaNamespace = NameTranslator.SchemaToNamespace(view.Name);
        var viewNamespace = !schemaNamespace.IsNullOrWhiteSpace()
            ? Namespace + "." + schemaNamespace
            : Namespace;

        var namespaces = new[] { "ServiceStack.DataAnnotations" }
            .Union(
                view.Columns
                    .Select(static c => c.Type.ClrType.Namespace)
                    .Where(ns => ns != null && !string.Equals(ns, viewNamespace, StringComparison.Ordinal))
                    .Select(static ns => ns!)
                , StringComparer.Ordinal)
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
        ArgumentNullException.ThrowIfNull(view);

        var className = NameTranslator.ViewToClassName(view.Name);
        var properties = view.Columns
            .Select(vc => BuildColumn(vc, comment, className))
            .ToList();

        return RecordDeclaration(Token(SyntaxKind.RecordKeyword), className)
            .AddAttributeLists(BuildClassAttributes(view, className).ToArray())
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.SealedKeyword))
            .WithLeadingTrivia(BuildViewComment(view.Name, comment))
            .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
            .WithMembers(List<MemberDeclarationSyntax>(properties))
            .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));
    }

    private static IEnumerable<AttributeListSyntax> BuildClassAttributes(IDatabaseView view, string className)
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentException.ThrowIfNullOrWhiteSpace(className);

        var attributes = new List<AttributeListSyntax>();

        var schemaName = view.Name.Schema;
        if (!schemaName.IsNullOrWhiteSpace())
        {
            var schemaAttribute = AttributeList(
                SingletonSeparatedList(
                    Attribute(
                        SyntaxUtilities.AttributeName(nameof(SchemaAttribute)),
                        AttributeArgumentList(
                            SingletonSeparatedList(
                                AttributeArgument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal(schemaName))))))));
            attributes.Add(schemaAttribute);
        }

        if (!string.Equals(className, view.Name.LocalName, StringComparison.Ordinal))
        {
            var aliasAttribute = AttributeList(
                SingletonSeparatedList(
                    Attribute(
                        SyntaxUtilities.AttributeName(nameof(AliasAttribute)),
                        AttributeArgumentList(
                            SingletonSeparatedList(
                                AttributeArgument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal(view.Name.LocalName))))))));
            attributes.Add(aliasAttribute);
        }

        return attributes;
    }

    private PropertyDeclarationSyntax BuildColumn(IDatabaseColumn column, Option<IDatabaseViewComments> comment, string className)
    {
        ArgumentNullException.ThrowIfNull(column);
        ArgumentException.ThrowIfNullOrWhiteSpace(className);

        var propertyName = NameTranslator.ColumnToPropertyName(className, column.Name.LocalName);
        var columnTypeSyntax = SyntaxUtilities.BuildTypeSyntax(column.Type.ClrType, column.IsNullable);

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

    private static IEnumerable<AttributeListSyntax> BuildColumnAttributes(IDatabaseColumn column, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(column);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        if (string.Equals(propertyName, column.Name.LocalName, StringComparison.Ordinal))
            return [];

        return
        [
            AttributeList(
                SingletonSeparatedList(
                    Attribute(
                        SyntaxUtilities.AttributeName(nameof(AliasAttribute)),
                        AttributeArgumentList(
                            SingletonSeparatedList(
                                AttributeArgument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal(column.Name.LocalName)))))))
            ),
        ];
    }
}