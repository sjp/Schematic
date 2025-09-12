﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
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
using StringHashSet = System.Collections.Generic.HashSet<string>;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore;

/// <summary>
/// Generate data access classes for tables for use with Entity Framework Core.
/// </summary>
/// <seealso cref="DatabaseTableGenerator" />
public class EFCoreTableGenerator : DatabaseTableGenerator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EFCoreTableGenerator"/> class.
    /// </summary>
    /// <param name="fileSystem">A file system.</param>
    /// <param name="nameTranslator">The name translator.</param>
    /// <param name="baseNamespace">The base namespace.</param>
    /// <exception cref="ArgumentNullException"><paramref name="baseNamespace"/> is <see langword="null" />, empty, or whitespace.</exception>
    public EFCoreTableGenerator(IFileSystem fileSystem, INameTranslator nameTranslator, string baseNamespace)
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

    private static string GenerateUniqueName(StringHashSet existingNames, string propertyName)
    {
        var candidateName = propertyName;
        var suffix = 1;
        while (!existingNames.Add(candidateName))
        {
            candidateName = propertyName + "_" + suffix.ToString(CultureInfo.InvariantCulture);
            suffix++;

            if (suffix > 10000)
                throw new InvalidOperationException("Unable to generate a valid candidate column name.");
        }

        return candidateName;
    }

    /// <summary>
    /// Generates source code that enables interoperability with a given database table for Entity Framework Core.
    /// </summary>
    /// <param name="tables">The database tables in the database.</param>
    /// <param name="table">A database table.</param>
    /// <param name="comment">Comment information for the given table.</param>
    /// <returns>A string containing source code to interact with the table.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/> or <paramref name="table"/> is <see langword="null" />.</exception>
    public override string Generate(IReadOnlyCollection<IRelationalDatabaseTable> tables, IRelationalDatabaseTable table, Option<IRelationalDatabaseTableComments> comment)
    {
        ArgumentNullException.ThrowIfNull(tables);
        ArgumentNullException.ThrowIfNull(table);

        var schemaNamespace = NameTranslator.SchemaToNamespace(table.Name);
        var tableNamespace = !schemaNamespace.IsNullOrWhiteSpace()
            ? Namespace + "." + schemaNamespace
            : Namespace;

        var namespaces = table.Columns
            .Select(static c => c.Type.ClrType.Namespace)
            .Where(ns => ns != null && !string.Equals(ns, tableNamespace, StringComparison.Ordinal))
            .Select(static ns => ns!)
            .Union(
            [
                "System.Collections.Generic",
                "System.ComponentModel.DataAnnotations",
                "System.ComponentModel.DataAnnotations.Schema",
            ], StringComparer.Ordinal)
            .Distinct(StringComparer.Ordinal)
            .OrderNamespaces()
            .ToList();

        var usingStatements = namespaces
            .Select(static ns => ParseName(ns))
            .Select(UsingDirective)
            .ToList();
        var namespaceDeclaration = NamespaceDeclaration(ParseName(tableNamespace));
        var classDeclaration = BuildClass(tables, table, comment);

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

    private RecordDeclarationSyntax BuildClass(IEnumerable<IRelationalDatabaseTable> tables, IRelationalDatabaseTable table, Option<IRelationalDatabaseTableComments> comment)
    {
        ArgumentNullException.ThrowIfNull(tables);
        ArgumentNullException.ThrowIfNull(table);

        var className = NameTranslator.TableToClassName(table.Name);
        var columnProperties = table.Columns
            .Select(c => BuildColumn(c, comment, className))
            .ToList();

        var usedNames = new StringHashSet(table.Columns.Select(c => NameTranslator.ColumnToPropertyName(table.Name.LocalName, c.Name.LocalName)), StringComparer.Ordinal);

        var parentKeyProperties = table.ParentKeys.Select(fk =>
        {
            var candidatePropertyName = NameTranslator.TableToClassName(fk.ParentTable);
            var propertyName = GenerateUniqueName(usedNames, candidatePropertyName);

            return BuildParentKey(tables, fk, comment, className, propertyName);
        });
        var childKeyProperties = table.ChildKeys.Select(ck =>
        {
            var candidatePropertyName = NameTranslator.TableToClassName(ck.ChildTable).Pluralize();
            var propertyName = GenerateUniqueName(usedNames, candidatePropertyName);

            return BuildChildKey(tables, ck, className, propertyName);
        });
        var properties = columnProperties.Concat(parentKeyProperties).Concat(childKeyProperties);

        return RecordDeclaration(Token(SyntaxKind.RecordKeyword), className)
            .AddAttributeLists(BuildClassAttributes(table, className).ToArray())
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .WithLeadingTrivia(BuildTableComment(table.Name, comment))
            .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
            .WithMembers(List<MemberDeclarationSyntax>(properties))
            .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));
    }

    private PropertyDeclarationSyntax BuildColumn(IDatabaseColumn column, Option<IRelationalDatabaseTableComments> comment, string className)
    {
        ArgumentNullException.ThrowIfNull(column);
        ArgumentException.ThrowIfNullOrWhiteSpace(className);

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

    private PropertyDeclarationSyntax BuildParentKey(IEnumerable<IRelationalDatabaseTable> tables, IDatabaseRelationalKey relationalKey, Option<IRelationalDatabaseTableComments> comment, string className, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(tables);
        ArgumentNullException.ThrowIfNull(relationalKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(className);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        var parentTable = relationalKey.ParentTable;

        var parentSchemaName = NameTranslator.SchemaToNamespace(parentTable);
        var parentClassName = NameTranslator.TableToClassName(parentTable);
        var qualifiedParentName = !parentSchemaName.IsNullOrWhiteSpace()
            ? parentSchemaName + "." + parentClassName
            : parentClassName;

        var foreignKeyIsNotNull = relationalKey.ChildKey.Columns.All(static c => !c.IsNullable);

        var parentTypeName = foreignKeyIsNotNull
            ? ParseTypeName(qualifiedParentName)
            : NullableType(ParseTypeName(qualifiedParentName));

        var property = PropertyDeclaration(
            parentTypeName,
            Identifier(propertyName)
        );

        var foreignKey = property
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.VirtualKeyword)))
            .WithAccessorList(SyntaxUtilities.PropertyGetSetDeclaration)
            .WithLeadingTrivia(BuildForeignKeyComment(relationalKey, comment));

        if (!foreignKeyIsNotNull)
            return foreignKey;

        return foreignKey
            .WithInitializer(SyntaxUtilities.NotNullDefault)
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
    }

    private PropertyDeclarationSyntax BuildChildKey(IEnumerable<IRelationalDatabaseTable> tables, IDatabaseRelationalKey relationalKey, string className, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(tables);
        ArgumentNullException.ThrowIfNull(relationalKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(className);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        var childTableName = relationalKey.ChildTable;

        var childSchemaName = NameTranslator.SchemaToNamespace(childTableName);
        var childClassName = NameTranslator.TableToClassName(childTableName);
        var qualifiedChildName = !childSchemaName.IsNullOrWhiteSpace()
            ? childSchemaName + "." + childClassName
            : childClassName;

        var childTable = tables.FirstOrDefault(t => t.Name == relationalKey.ChildTable);
        var childKeyIsUnique = childTable != null && IsChildKeyUnique(childTable, relationalKey.ChildKey);

        if (childKeyIsUnique)
        {
            var property = PropertyDeclaration(
                NullableType(ParseTypeName(qualifiedChildName)),
                Identifier(propertyName)
            );

            return property
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.VirtualKeyword)))
                .WithAccessorList(SyntaxUtilities.PropertyGetSetDeclaration)
                .WithLeadingTrivia(BuildChildKeyComment(relationalKey));
        }
        else
        {
            var columnTypeSyntax = GenericName(
                Identifier(nameof(ICollection<object>)),
                TypeArgumentList(
                    SingletonSeparatedList(
                        ParseTypeName(qualifiedChildName))));

            var property = PropertyDeclaration(
                columnTypeSyntax,
                Identifier(propertyName)
            );

            var hashsetInstance = EqualsValueClause(
                ObjectCreationExpression(
                    GenericName(
                        Identifier(nameof(System.Collections.Generic.HashSet<object>)),
                        TypeArgumentList(
                            SingletonSeparatedList(ParseTypeName(qualifiedChildName)))))
                .WithArgumentList(ArgumentList())
            );

            return property
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.VirtualKeyword)))
                .WithAccessorList(SyntaxUtilities.PropertyGetSetDeclaration)
                .WithInitializer(hashsetInstance)
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                .WithLeadingTrivia(BuildChildKeyComment(relationalKey));
        }
    }

    private static SyntaxTriviaList BuildTableComment(Identifier tableName, Option<IRelationalDatabaseTableComments> comment)
    {
        ArgumentNullException.ThrowIfNull(tableName);

        return comment
            .Bind(static c => c.Comment)
            .Match(
                SyntaxUtilities.BuildCommentTrivia,
                () => SyntaxUtilities.BuildCommentTrivia(
                [
                    XmlText("A mapping class to query the "),
                    XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(tableName.LocalName))),
                    XmlText(" table."),
                ])
            );
    }

    private static SyntaxTriviaList BuildColumnComment(Identifier columnName, Option<IRelationalDatabaseTableComments> comment)
    {
        ArgumentNullException.ThrowIfNull(columnName);

        return comment
            .Bind(c => c.ColumnComments.TryGetValue(columnName, out var cc) ? cc : Option<string>.None)
            .Match(
                SyntaxUtilities.BuildCommentTrivia,
                () => SyntaxUtilities.BuildCommentTrivia(
                [
                    XmlText("The "),
                    XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(columnName.LocalName))),
                    XmlText(" column."),
                ])
            );
    }

    private static SyntaxTriviaList BuildForeignKeyComment(IDatabaseRelationalKey relationalKey, Option<IRelationalDatabaseTableComments> comment)
    {
        ArgumentNullException.ThrowIfNull(relationalKey);

        var hasChildKeyName = relationalKey.ChildKey.Name.IsSome;

        return comment
            .Bind(c => relationalKey.ChildKey.Name
                .Match(
                    ckName => c.ForeignKeyComments.TryGetValue(ckName, out var fkc) ? fkc : Option<string>.None,
                    static () => Option<string>.None))
            .Match(
                SyntaxUtilities.BuildCommentTrivia,
                () =>
                {
                    var foreignKeyNameNode = relationalKey.ChildKey.Name.Match(
                        name => XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(name.LocalName))),
                        static () => XmlText(string.Empty) as XmlNodeSyntax
                    );

                    return SyntaxUtilities.BuildCommentTrivia(
                    [
                        XmlText("The" + (hasChildKeyName ? " " : string.Empty)),
                        foreignKeyNameNode,
                        XmlText(" foreign key. Navigates from "),
                        XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(relationalKey.ChildTable.LocalName))),
                        XmlText(" to "),
                        XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(relationalKey.ParentTable.LocalName))),
                        XmlText("."),
                    ]);
                }
            );
    }

    private static SyntaxTriviaList BuildChildKeyComment(IDatabaseRelationalKey relationalKey)
    {
        ArgumentNullException.ThrowIfNull(relationalKey);

        var hasChildKeyName = relationalKey.ChildKey.Name.IsSome;
        var foreignKeyNameNode = relationalKey.ChildKey.Name.Match(
            name => XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(name.LocalName))),
            static () => XmlText(string.Empty) as XmlNodeSyntax
        );

        return SyntaxUtilities.BuildCommentTrivia(
        [
            XmlText("The" + (hasChildKeyName ? " " : string.Empty)),
            foreignKeyNameNode,
            XmlText(" child key. Navigates from "),
            XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(relationalKey.ParentTable.LocalName))),
            XmlText(" to "),
            XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(relationalKey.ChildTable.LocalName))),
            XmlText("."),
        ]);
    }

    private static IEnumerable<AttributeListSyntax> BuildClassAttributes(IRelationalDatabaseTable table, string className)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentException.ThrowIfNullOrWhiteSpace(className);

        var attributes = new List<AttributeListSyntax>();

        if (!string.Equals(className, table.Name.LocalName, StringComparison.Ordinal))
        {
            var attributeArguments = new List<AttributeArgumentSyntax>
            {
                AttributeArgument(
                    LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        Literal(table.Name.LocalName))),
            };

            var schemaName = table.Name.Schema;
            if (!schemaName.IsNullOrWhiteSpace())
            {
                var schemaArgument = AttributeArgument(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(nameof(TableAttribute.Schema)),
                        Token(SyntaxKind.EqualsToken),
                        LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal(schemaName))));
                attributeArguments.Add(schemaArgument);
            }

            var tableAttribute = AttributeList(
                SingletonSeparatedList(
                    Attribute(
                        SyntaxUtilities.AttributeName(nameof(TableAttribute)),
                        AttributeArgumentList(
                            SeparatedList(attributeArguments)))));
            attributes.Add(tableAttribute);
        }

        return attributes;
    }

    private static IEnumerable<AttributeListSyntax> BuildColumnAttributes(IDatabaseColumn column, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(column);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        var attributes = new List<AttributeListSyntax>();
        var clrType = column.Type.ClrType;

        var isConstrainedType = clrType == typeof(string) || clrType == typeof(byte[]);
        if (isConstrainedType && column.Type.MaxLength > 0)
        {
            var maxLengthAttribute = AttributeList(
                SingletonSeparatedList(
                    Attribute(
                        SyntaxUtilities.AttributeName(nameof(MaxLengthAttribute)),
                        AttributeArgumentList(
                            SingletonSeparatedList(
                                AttributeArgument(
                                    LiteralExpression(
                                        SyntaxKind.NumericLiteralExpression,
                                        Literal(column.Type.MaxLength))))))));
            attributes.Add(maxLengthAttribute);
        }

        if (!clrType.IsValueType && !column.IsNullable)
        {
            var requiredAttribute = AttributeList(
                SingletonSeparatedList(
                    Attribute(
                        SyntaxUtilities.AttributeName(nameof(RequiredAttribute)))));
            attributes.Add(requiredAttribute);
        }

        column.AutoIncrement.IfSome(_ =>
        {
            var databaseGeneratedAttribute = AttributeList(
                SingletonSeparatedList(
                    Attribute(
                        SyntaxUtilities.AttributeName(nameof(DatabaseGeneratedAttribute)),
                        AttributeArgumentList(
                            SingletonSeparatedList(
                                AttributeArgument(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName(nameof(DatabaseGeneratedOption)),
                                        IdentifierName(nameof(DatabaseGeneratedOption.Identity)))))))));
            attributes.Add(databaseGeneratedAttribute);
        });

        var columnAttributeArgs = new List<AttributeArgumentSyntax>();
        if (!string.Equals(propertyName, column.Name.LocalName, StringComparison.Ordinal))
        {
            var quotedColumnName = LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(column.Name.LocalName));
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
                        Literal(column.Type.TypeName.LocalName))))
        );

        var columnAttribute = AttributeList(
            SingletonSeparatedList(
                Attribute(
                    SyntaxUtilities.AttributeName(nameof(ColumnAttribute)),
                    AttributeArgumentList(
                        SeparatedList(columnAttributeArgs)))));
        attributes.Add(columnAttribute);

        return attributes;
    }

    private static bool IsChildKeyUnique(IRelationalDatabaseTable table, IDatabaseKey key)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(key);

        var keyColumnNames = key.Columns.Select(static c => c.Name.LocalName).ToList();
        var matchesPkColumns = table.PrimaryKey
            .Match(
                pk =>
                {
                    var pkColumnNames = pk.Columns.Select(static c => c.Name.LocalName).ToList();
                    return keyColumnNames.SequenceEqual(pkColumnNames, StringComparer.Ordinal);
                },
                static () => false
            );
        if (matchesPkColumns)
            return true;

        var matchesUkColumns = table.UniqueKeys.Any(uk =>
        {
            var ukColumnNames = uk.Columns.Select(static c => c.Name.LocalName).ToList();
            return keyColumnNames.SequenceEqual(ukColumnNames, StringComparer.Ordinal);
        });
        if (matchesUkColumns)
            return true;

        var uniqueIndexes = table.Indexes.Where(static i => i.IsUnique).ToList();
        if (uniqueIndexes.Count == 0)
            return false;

        return uniqueIndexes.Exists(i =>
        {
            var indexColumnExpressions = i.Columns
                .Select(ic => ic.DependentColumns.Select(dc => dc.Name.LocalName).FirstOrDefault() ?? ic.Expression)
                .ToList();
            return keyColumnNames.SequenceEqual(indexColumnExpressions, StringComparer.Ordinal);
        });
    }
}