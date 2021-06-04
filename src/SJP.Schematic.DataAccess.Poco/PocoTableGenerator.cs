using System;
using System.Collections.Generic;
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

namespace SJP.Schematic.DataAccess.Poco
{
    /// <summary>
    /// Generate data access classes for tables for use with POCO data access projects.
    /// </summary>
    /// <seealso cref="DatabaseTableGenerator" />
    public class PocoTableGenerator : DatabaseTableGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PocoTableGenerator"/> class.
        /// </summary>
        /// <param name="fileSystem">A file system.</param>
        /// <param name="nameTranslator">The name translator.</param>
        /// <param name="baseNamespace">The base namespace.</param>
        /// <exception cref="ArgumentNullException"><paramref name="baseNamespace"/> is <c>null</c>, empty, or whitespace.</exception>
        public PocoTableGenerator(IFileSystem fileSystem, INameTranslator nameTranslator, string baseNamespace)
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
        /// Generates source code that enables interoperability with a given database table for POCO projects.
        /// </summary>
        /// <param name="tables">The database tables in the database.</param>
        /// <param name="table">A database table.</param>
        /// <param name="comment">Comment information for the given table.</param>
        /// <returns>A string containing source code to interact with the table.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tables"/> or <paramref name="table"/> is <c>null</c>.</exception>
        public override string Generate(IReadOnlyCollection<IRelationalDatabaseTable> tables, IRelationalDatabaseTable table, Option<IRelationalDatabaseTableComments> comment)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var schemaNamespace = NameTranslator.SchemaToNamespace(table.Name);
            var tableNamespace = !schemaNamespace.IsNullOrWhiteSpace()
                ? Namespace + "." + schemaNamespace
                : Namespace;

            var namespaces = table.Columns
                .Select(static c => c.Type.ClrType.Namespace)
                .Where(ns => ns != null && !string.Equals(ns, tableNamespace, StringComparison.Ordinal))
                .Select(static ns => ns!)
                .Distinct(StringComparer.Ordinal)
                .OrderNamespaces()
                .ToList();

            var usingStatements = namespaces
                .Select(static ns => ParseName(ns))
                .Select(UsingDirective)
                .ToList();
            var namespaceDeclaration = NamespaceDeclaration(ParseName(tableNamespace));
            var classDeclaration = BuildClass(table, comment);

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

        private RecordDeclarationSyntax BuildClass(IRelationalDatabaseTable table, Option<IRelationalDatabaseTableComments> comment)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var className = NameTranslator.TableToClassName(table.Name);
            var properties = table.Columns
                .Select(c => BuildColumn(c, comment, className))
                .ToList();

            return RecordDeclaration(Token(SyntaxKind.RecordKeyword), className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.SealedKeyword))
                .WithLeadingTrivia(BuildTableComment(table.Name, comment))
                .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
                .WithMembers(List<MemberDeclarationSyntax>(properties))
                .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));
        }

        private PropertyDeclarationSyntax BuildColumn(IDatabaseColumn column, Option<IRelationalDatabaseTableComments> comment, string className)
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
            if (string.Equals(clrType.Namespace, "System", StringComparison.Ordinal) && SyntaxUtilities.TypeSyntaxMap.ContainsKey(clrType.Name))
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

        private static SyntaxTriviaList BuildTableComment(Identifier tableName, Option<IRelationalDatabaseTableComments> comment)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            return comment
                .Bind(static c => c.Comment)
                .Match(
                    SyntaxUtilities.BuildCommentTrivia,
                    () => SyntaxUtilities.BuildCommentTrivia(new XmlNodeSyntax[]
                    {
                        XmlText("A mapping class to query the "),
                        XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(tableName.LocalName))),
                        XmlText(" table.")
                    })
                );
        }

        private static SyntaxTriviaList BuildColumnComment(Identifier columnName, Option<IRelationalDatabaseTableComments> comment)
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
    }
}
