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
    public class PocoViewGenerator : DatabaseViewGenerator
    {
        public PocoViewGenerator(INameTranslator nameTranslator, string baseNamespace)
            : base(nameTranslator)
        {
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            Namespace = baseNamespace;
        }

        protected string Namespace { get; }

        public override string Generate(IDatabaseView view, Option<IDatabaseViewComments> comment)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var schemaNamespace = NameTranslator.SchemaToNamespace(view.Name);
            var viewNamespace = !schemaNamespace.IsNullOrWhiteSpace()
                ? Namespace + "." + schemaNamespace
                : Namespace;

            var namespaces = view.Columns
                .Select(c => c.Type.ClrType.Namespace)
                .Where(ns => ns != viewNamespace)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            var usingStatements = namespaces
                .Select(ns => ParseName(ns))
                .Select(UsingDirective)
                .ToList();
            var namespaceDeclaration = NamespaceDeclaration(ParseName(viewNamespace));
            var classDeclaration = BuildClass(view, comment);

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

        private ClassDeclarationSyntax BuildClass(IDatabaseView view, Option<IDatabaseViewComments> comment)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var className = NameTranslator.ViewToClassName(view.Name);
            var properties = view.Columns
                .Select(vc => BuildColumn(vc, comment, className))
                .ToList();

            return ClassDeclaration(className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithLeadingTrivia(BuildViewComment(view.Name, comment))
                .WithMembers(new SyntaxList<MemberDeclarationSyntax>(properties));
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
                ? NullableType(ParseTypeName(clrType.FullName))
                : ParseTypeName(clrType.FullName);
            if (clrType.Namespace == "System" && TypeSyntaxMap.ContainsKey(clrType.Name))
                columnTypeSyntax = column.IsNullable
                    ? NullableType(TypeSyntaxMap[clrType.Name])
                    : TypeSyntaxMap[clrType.Name];

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

        private static SyntaxTriviaList BuildViewComment(Identifier viewName, Option<IDatabaseViewComments> comment)
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            return comment
                .Bind(c => c.Comment)
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

        private static readonly IReadOnlyDictionary<string, TypeSyntax> TypeSyntaxMap = new Dictionary<string, TypeSyntax>
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
    }
}
