using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
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

namespace SJP.Schematic.DataAccess.EntityFrameworkCore
{
    public class EFCoreTableGenerator : DatabaseTableGenerator
    {
        public EFCoreTableGenerator(INameTranslator nameTranslator, string baseNamespace, string indent = "    ")
            : base(nameTranslator, indent)
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
                .Union(new[]
                {
                    "System.Collections.Generic",
                    "System.ComponentModel.DataAnnotations",
                    "System.ComponentModel.DataAnnotations.Schema"
                })
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            var usingStatements = namespaces
                .Select(ns => ParseName(ns))
                .Select(UsingDirective)
                .ToList();
            var namespaceDeclaration = NamespaceDeclaration(ParseName(tableNamespace));
            var classDeclaration = BuildClass(tables, table, comment);

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

        private ClassDeclarationSyntax BuildClass(IEnumerable<IRelationalDatabaseTable> tables, IRelationalDatabaseTable table, Option<IRelationalDatabaseTableComments> comment)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var className = NameTranslator.TableToClassName(table.Name);
            var columnProperties = table.Columns
                .Select(vc => BuildColumn(vc, comment, className))
                .ToList();
            var parentKeyProperties = table.ParentKeys.Select(fk => BuildParentKey(tables, fk, comment, className));
            var childKeyProperties = table.ChildKeys.Select(ck => BuildChildKey(tables, ck, className));
            var properties = columnProperties.Concat(parentKeyProperties).Concat(childKeyProperties);

            return ClassDeclaration(className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAttributeLists(BuildClassAttributes(table, className).ToArray())
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
            if (clrType.Namespace == nameof(System) && SyntaxUtilities.TypeSyntaxMap.ContainsKey(clrType.Name))
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

        private PropertyDeclarationSyntax BuildParentKey(IEnumerable<IRelationalDatabaseTable> tables, IDatabaseRelationalKey relationalKey, Option<IRelationalDatabaseTableComments> comment, string className)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));
            if (relationalKey == null)
                throw new ArgumentNullException(nameof(relationalKey));
            if (className.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(className));

            var parentTable = relationalKey.ParentTable;

            var parentSchemaName = parentTable.Schema;
            var parentClassName = NameTranslator.TableToClassName(parentTable);
            var qualifiedParentName = !parentSchemaName.IsNullOrWhiteSpace()
                ? parentSchemaName + "." + parentClassName
                : parentClassName;

            var foreignKeyIsNotNull = relationalKey.ChildKey.Columns.All(c => !c.IsNullable);

            var parentTypeName = foreignKeyIsNotNull
                ? ParseTypeName(qualifiedParentName)
                : NullableType(ParseTypeName(qualifiedParentName));

            var property = PropertyDeclaration(
                parentTypeName,
                Identifier(parentClassName)
            );

            var foreignKey = property
                .WithModifiers(new SyntaxTokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.VirtualKeyword)))
                .WithAccessorList(SyntaxUtilities.PropertyGetSetDeclaration)
                .WithLeadingTrivia(BuildForeignKeyComment(relationalKey, comment));

            if (!foreignKeyIsNotNull)
                return foreignKey;

            return foreignKey
                .WithInitializer(SyntaxUtilities.NotNullDefault)
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }


        private PropertyDeclarationSyntax BuildChildKey(IEnumerable<IRelationalDatabaseTable> tables, IDatabaseRelationalKey relationalKey, string className)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));
            if (relationalKey == null)
                throw new ArgumentNullException(nameof(relationalKey));
            if (className.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(className));

            var childTableName = relationalKey.ChildTable;

            var childSchemaName = childTableName.Schema;
            var childClassName = NameTranslator.TableToClassName(childTableName);
            var qualifiedChildName = !childSchemaName.IsNullOrWhiteSpace()
                ? childSchemaName + "." + childClassName
                : childClassName;

            var propertyName = childClassName.Pluralize();

            var childTable = tables.FirstOrDefault(t => t.Name == relationalKey.ChildTable);
            var childKeyIsUnique = childTable != null && IsChildKeyUnique(childTable, relationalKey.ChildKey);

            if (childKeyIsUnique)
            {
                var property = PropertyDeclaration(
                    NullableType(ParseTypeName(qualifiedChildName)),
                    Identifier(propertyName)
                );

                return property
                    .WithModifiers(new SyntaxTokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.VirtualKeyword)))
                    .WithAccessorList(SyntaxUtilities.PropertyGetSetDeclaration)
                    .WithLeadingTrivia(BuildChildKeyComment(relationalKey));
            }
            else
            {
                var columnTypeSyntax = GenericName(
                    Identifier(nameof(ICollection<object>)),
                    TypeArgumentList(
                        SingletonSeparatedList(
                            ParseTypeName(qualifiedChildName)))
                );

                var property = PropertyDeclaration(
                    columnTypeSyntax,
                    Identifier(propertyName)
                );

                var hashsetInstance = EqualsValueClause(
                    ObjectCreationExpression(
                        GenericName(
                            Identifier(nameof(System.Collections.Generic.HashSet<object>)),
                            TypeArgumentList(
                                SingletonSeparatedList(ParseTypeName(qualifiedChildName)))
                            ))
                    .WithArgumentList(ArgumentList())
                );

                return property
                    .WithModifiers(new SyntaxTokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.VirtualKeyword)))
                    .WithAccessorList(SyntaxUtilities.PropertyGetSetDeclaration)
                    .WithInitializer(hashsetInstance)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                    .WithLeadingTrivia(BuildChildKeyComment(relationalKey));
            }
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

        private static SyntaxTriviaList BuildForeignKeyComment(IDatabaseRelationalKey relationalKey, Option<IRelationalDatabaseTableComments> comment)
        {
            if (relationalKey == null)
                throw new ArgumentNullException(nameof(relationalKey));

            var hasChildKeyName = relationalKey.ChildKey.Name.IsSome;

            return comment
                .Bind(c => relationalKey.ChildKey.Name
                    .Match(
                        ckName => c.ForeignKeyComments.TryGetValue(ckName, out var fkc) ? fkc : Option<string>.None,
                        () => Option<string>.None))
                .Match(
                    SyntaxUtilities.BuildCommentTrivia,
                    () =>
                    {
                        var foreignKeyNameNode = relationalKey.ChildKey.Name.Match(
                            name => XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(name.LocalName))),
                            () => XmlText(string.Empty) as XmlNodeSyntax
                        );

                        return SyntaxUtilities.BuildCommentTrivia(new XmlNodeSyntax[]
                        {
                            XmlText("The" + (hasChildKeyName ? " " : string.Empty)),
                            foreignKeyNameNode,
                            XmlText(" foreign key. Navigates from "),
                            XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(relationalKey.ChildTable.LocalName))),
                            XmlText(" to "),
                            XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(relationalKey.ParentTable.LocalName))),
                            XmlText(".")
                        });
                    }
                );
        }

        private static SyntaxTriviaList BuildChildKeyComment(IDatabaseRelationalKey relationalKey)
        {
            if (relationalKey == null)
                throw new ArgumentNullException(nameof(relationalKey));

            var hasChildKeyName = relationalKey.ChildKey.Name.IsSome;
            var foreignKeyNameNode = relationalKey.ChildKey.Name.Match(
                name => XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(name.LocalName))),
                () => XmlText(string.Empty) as XmlNodeSyntax
            );

            return SyntaxUtilities.BuildCommentTrivia(new XmlNodeSyntax[]
            {
                XmlText("The" + (hasChildKeyName ? " " : string.Empty)),
                foreignKeyNameNode,
                XmlText(" child key. Navigates from "),
                XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(relationalKey.ParentTable.LocalName))),
                XmlText(" to "),
                XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(relationalKey.ChildTable.LocalName))),
                XmlText(".")
            });
        }

        private static IEnumerable<AttributeListSyntax> BuildClassAttributes(IRelationalDatabaseTable table, string className)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (className.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(className));

            var attributes = new List<AttributeListSyntax>();

            if (className != table.Name.LocalName)
            {
                var attributeArguments = new List<AttributeArgumentSyntax>
                {
                    AttributeArgument(
                        LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(table.Name.LocalName)))
                };

                var schemaName = table.Name.Schema;
                if (!schemaName.IsNullOrWhiteSpace())
                {
                    var schemaArgument = AttributeArgument(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName(nameof(System.ComponentModel.DataAnnotations.Schema.TableAttribute.Schema)),
                            Token(SyntaxKind.EqualsToken),
                            LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(schemaName))
                        )
                    );
                    attributeArguments.Add(schemaArgument);
                }

                var tableAttribute = AttributeList(
                    SeparatedList(new[]
                    {
                        Attribute(
                            SyntaxUtilities.AttributeName(nameof(System.ComponentModel.DataAnnotations.Schema.TableAttribute)),
                            AttributeArgumentList(SeparatedList(attributeArguments))
                        )
                    })
                );
                attributes.Add(tableAttribute);
            }

            return attributes;
        }


        private static IEnumerable<AttributeListSyntax> BuildColumnAttributes(IDatabaseColumn column, string propertyName)
            {
                if (column == null)
                    throw new ArgumentNullException(nameof(column));
                if (propertyName.IsNullOrWhiteSpace())
                    throw new ArgumentNullException(nameof(propertyName));

            var attributes = new List<AttributeListSyntax>();
            var clrType = column.Type.ClrType;

            var isConstrainedType = clrType == typeof(string) || clrType == typeof(byte[]);
            if (isConstrainedType && column.Type.MaxLength > 0)
            {
                var maxLengthAttribute = AttributeList(
                    SeparatedList(new[]
                    {
                        Attribute(
                            SyntaxUtilities.AttributeName(nameof(System.ComponentModel.DataAnnotations.MaxLengthAttribute)),
                            AttributeArgumentList(
                                SeparatedList(new[]
                                {
                                    AttributeArgument(
                                        LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(column.Type.MaxLength)))
                                }))
                        )
                    })
                );
                attributes.Add(maxLengthAttribute);
            }

            if (!clrType.IsValueType && !column.IsNullable)
            {
                var requiredAttribute = AttributeList(
                    SeparatedList(new[]
                    {
                        Attribute(
                            SyntaxUtilities.AttributeName(nameof(System.ComponentModel.DataAnnotations.RequiredAttribute))
                        )
                    })
                );
                attributes.Add(requiredAttribute);
            }

            column.AutoIncrement.IfSome(_ =>
            {
                var databaseGeneratedAttribute = AttributeList(
                    SeparatedList(new[]
                    {
                        Attribute(
                            SyntaxUtilities.AttributeName(nameof(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute)),
                            AttributeArgumentList(
                                SeparatedList(new[]
                                {
                                    AttributeArgument(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName(nameof(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption)),
                                            IdentifierName(nameof(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity))))
                                })
                            )
                        )
                    })
                );
                attributes.Add(databaseGeneratedAttribute);
            });

            var columnAttributeArgs = new List<AttributeArgumentSyntax>();
            if (propertyName != column.Name.LocalName)
            {
                var quotedColumnName = LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(column.Name.LocalName));
                columnAttributeArgs.Add(AttributeArgument(quotedColumnName));
            }
            columnAttributeArgs.Add(AttributeArgument(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(nameof(System.ComponentModel.DataAnnotations.Schema.ColumnAttribute.TypeName)),
                    Token(SyntaxKind.EqualsToken),
                    LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(column.Type.TypeName.LocalName))
                )
            ));

            var columnAttribute = AttributeList(
                SeparatedList(new[]
                {
                    Attribute(
                        SyntaxUtilities.AttributeName(nameof(System.ComponentModel.DataAnnotations.Schema.ColumnAttribute)),
                        AttributeArgumentList(SeparatedList(columnAttributeArgs))
                    )
                })
            );
            attributes.Add(columnAttribute);

            return attributes;
        }

        private static bool IsChildKeyUnique(IRelationalDatabaseTable table, IDatabaseKey key)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var keyColumnNames = key.Columns.Select(c => c.Name.LocalName).ToList();
            var matchesPkColumns = table.PrimaryKey
                .Match(
                    pk =>
                    {
                        var pkColumnNames = pk.Columns.Select(c => c.Name.LocalName).ToList();
                        return keyColumnNames.SequenceEqual(pkColumnNames);
                    },
                    () => false
                );
            if (matchesPkColumns)
                return true;

            var matchesUkColumns = table.UniqueKeys.Any(uk =>
            {
                var ukColumnNames = uk.Columns.Select(c => c.Name.LocalName).ToList();
                return keyColumnNames.SequenceEqual(ukColumnNames);
            });
            if (matchesUkColumns)
                return true;

            var uniqueIndexes = table.Indexes.Where(i => i.IsUnique).ToList();
            if (uniqueIndexes.Count == 0)
                return false;

            return uniqueIndexes.Any(i =>
            {
                var indexColumnExpressions = i.Columns
                    .Select(ic => ic.DependentColumns.Select(dc => dc.Name.LocalName).FirstOrDefault() ?? ic.Expression)
                    .ToList();
                return keyColumnNames.SequenceEqual(indexColumnExpressions);
            });
        }
    }
}
