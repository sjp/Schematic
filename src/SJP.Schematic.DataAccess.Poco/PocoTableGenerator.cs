using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.DataAccess.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SJP.Schematic.DataAccess.Poco
{
    public class PocoTableGenerator : DatabaseTableGenerator
    {
        public PocoTableGenerator(INameTranslator nameTranslator, string baseNamespace)
            : base(nameTranslator)
        {
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            Namespace = baseNamespace;
        }

        protected string Namespace { get; }

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
                .Select(c => c.Type.ClrType.Namespace)
                .Where(ns => ns != tableNamespace)
                .Distinct()
                .OrderNamespaces()
                .ToList();

            var usingStatements = namespaces
                .Select(ns => ParseName(ns))
                .Select(UsingDirective)
                .ToList();
            var namespaceDeclaration = NamespaceDeclaration(ParseName(tableNamespace));
            var classDeclaration = BuildClass(table, comment);

            var document = CompilationUnit()
                .WithUsings(new SyntaxList<UsingDirectiveSyntax>(usingStatements))
                .WithMembers(
                    new SyntaxList<MemberDeclarationSyntax>(
                        namespaceDeclaration.WithMembers(
                            new SyntaxList<MemberDeclarationSyntax>(classDeclaration)
                        )
                    )
                );

            using var workspace = new AdhocWorkspace();
            return Formatter.Format(document, workspace).ToFullString();
        }

        private ClassDeclarationSyntax BuildClass(IRelationalDatabaseTable table, Option<IRelationalDatabaseTableComments> comment)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var className = NameTranslator.TableToClassName(table.Name);
            var properties = table.Columns
                .Select(c => BuildColumn(c, comment, className))
                .ToList();

            return ClassDeclaration(className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithLeadingTrivia(BuildTableComment(table.Name, comment))
                .WithMembers(new SyntaxList<MemberDeclarationSyntax>(properties));
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
                ? NullableType(ParseTypeName(clrType.FullName))
                : ParseTypeName(clrType.FullName);
            if (clrType.Namespace == "System" && SyntaxUtilities.TypeSyntaxMap.ContainsKey(clrType.Name))
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
                .Bind(c => c.Comment)
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
