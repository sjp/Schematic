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
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.DataAccess.CodeGeneration;
using SJP.Schematic.DataAccess.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore
{
    public class EFCoreDbContextBuilder
    {
        public EFCoreDbContextBuilder(INameTranslator nameTranslator, string baseNamespace)
        {
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            NameTranslator = nameTranslator ?? throw new ArgumentNullException(nameof(nameTranslator));
            Namespace = baseNamespace;
        }

        protected INameTranslator NameTranslator { get; }

        protected string Namespace { get; }

        public string Generate(IEnumerable<IRelationalDatabaseTable> tables, IEnumerable<IDatabaseView> views, IEnumerable<IDatabaseSequence> sequences)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));
            if (views == null)
                throw new ArgumentNullException(nameof(views));
            if (sequences == null)
                throw new ArgumentNullException(nameof(sequences));

            var namespaces = new[]
                {
                    "System",
                    "Microsoft.EntityFrameworkCore"
                }
                .OrderNamespaces()
                .ToList();

            var usingStatements = namespaces
                .Select(ns => ParseName(ns))
                .Select(UsingDirective)
                .ToList();
            var namespaceDeclaration = NamespaceDeclaration(ParseName(Namespace));
            var classDeclaration = BuildDbContext(tables, views, sequences);

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

        private const string ModelBuilderMethodSummaryComment = "Configure the model that was discovered by convention from the defined entity types.";
        private const string ModelBuilderMethodParamComment = "The builder being used to construct the model for this context.";

        private static SyntaxTriviaList BuildOnModelCreateComment()
        {
            var summary = new[] { XmlText(ModelBuilderMethodSummaryComment) };
            var param = new Dictionary<string, IEnumerable<XmlNodeSyntax>>
            {
                ["modelBuilder"] = new[] { XmlText(ModelBuilderMethodParamComment) }
            };

            return SyntaxUtilities.BuildCommentTriviaWithParams(summary, param);
        }

        private ClassDeclarationSyntax BuildDbContext(IEnumerable<IRelationalDatabaseTable> tables, IEnumerable<IDatabaseView> views, IEnumerable<IDatabaseSequence> sequences)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));
            if (views == null)
                throw new ArgumentNullException(nameof(views));
            if (sequences == null)
                throw new ArgumentNullException(nameof(sequences));

            const string className = "AppContext";
            var baseClass = BaseList(
                SingletonSeparatedList<BaseTypeSyntax>(
                    SimpleBaseType(
                        IdentifierName(nameof(Microsoft.EntityFrameworkCore.DbContext)))));

            var tableDbSets = tables.Select(BuildTableDbSet).ToList();
            var viewDbSets = views.Select(BuildViewDbSet).ToList();
            var modelBuilderMethod = BuildOnModelCreatingMethod(tables, views, sequences);
            var members = tableDbSets
                .Concat(viewDbSets)
                .Concat(new MemberDeclarationSyntax[] { modelBuilderMethod })
                .ToList();

            return ClassDeclaration(className)
                .WithBaseList(baseClass)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithMembers(List(members));
        }

        private PropertyDeclarationSyntax BuildTableDbSet(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var schemaNamespace = NameTranslator.SchemaToNamespace(table.Name);
            var className = NameTranslator.TableToClassName(table.Name);
            var qualifiedClassName = !schemaNamespace.IsNullOrWhiteSpace()
                ? schemaNamespace + "." + className
                : className;
            var setName = className.Pluralize();
            var qualifiedTableName = !table.Name.Schema.IsNullOrWhiteSpace()
                ? table.Name.Schema + "." + table.Name.LocalName
                : table.Name.LocalName;

            return BuildDbSetProperty(qualifiedClassName, setName, qualifiedTableName, "table");
        }

        private PropertyDeclarationSyntax BuildViewDbSet(IDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var schemaNamespace = NameTranslator.SchemaToNamespace(view.Name);
            var className = NameTranslator.ViewToClassName(view.Name);
            var qualifiedClassName = !schemaNamespace.IsNullOrWhiteSpace()
                ? schemaNamespace + "." + className
                : className;
            var setName = className.Pluralize();
            var qualifiedViewName = !view.Name.Schema.IsNullOrWhiteSpace()
                ? view.Name.Schema + "." + view.Name.LocalName
                : view.Name.LocalName;

            return BuildDbSetProperty(qualifiedClassName, setName, qualifiedViewName, "view");
        }

        private static PropertyDeclarationSyntax BuildDbSetProperty(string typeArgument, string propertyName, string qualifiedTargetName, string objectType)
        {
            if (typeArgument.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(typeArgument));
            if (propertyName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(propertyName));
            if (qualifiedTargetName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(qualifiedTargetName));
            if (objectType.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(objectType));

            var dbSetType = GenericName(
                Identifier(nameof(Microsoft.EntityFrameworkCore.DbSet<object>)),
                TypeArgumentList(
                    SingletonSeparatedList(
                        ParseTypeName(typeArgument))));

            return PropertyDeclaration(dbSetType, Identifier(propertyName))
                .WithModifiers(new SyntaxTokenList(Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(SyntaxUtilities.PropertyGetSetDeclaration)
                .WithLeadingTrivia(BuildDbSetComment(qualifiedTargetName, objectType))
                .WithInitializer(SyntaxUtilities.NotNullDefault)
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        private static SyntaxTriviaList BuildDbSetComment(string targetName, string objectType)
        {
            if (targetName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(targetName));
            if (objectType.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(objectType));

            return SyntaxUtilities.BuildCommentTrivia(new XmlNodeSyntax[]
            {
                XmlText("Accesses the "),
                XmlElement("c", SingletonList<XmlNodeSyntax>(XmlText(targetName))),
                XmlText(" " + objectType + ".")
            });
        }

        private MethodDeclarationSyntax BuildOnModelCreatingMethod(IEnumerable<IRelationalDatabaseTable> tables, IEnumerable<IDatabaseView> views, IEnumerable<IDatabaseSequence> sequences)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));
            if (views == null)
                throw new ArgumentNullException(nameof(views));
            if (sequences == null)
                throw new ArgumentNullException(nameof(sequences));

            var tableConfigs = tables.SelectMany(BuildTableConfiguration);
            var viewConfigs = views.Select(BuildViewConfiguration);
            var sequenceConfigs = sequences.Select(BuildSequenceConfiguration);
            var expressions = tableConfigs
                .Concat(viewConfigs)
                .Concat(sequenceConfigs)
                .Select(invExpr => ExpressionStatement(invExpr))
                .ToArray();

            return MethodDeclaration(
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                Identifier("OnModelCreating"))
                .WithModifiers(TokenList(new[] { Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.OverrideKeyword) }))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(Identifier("modelBuilder"))
                                .WithType(
                                    IdentifierName(nameof(Microsoft.EntityFrameworkCore.ModelBuilder))))))
                .WithBody(Block(expressions))
                .WithLeadingTrivia(BuildOnModelCreateComment());
        }

        private IEnumerable<InvocationExpressionSyntax> BuildTableConfiguration(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var columnExprs = table.Columns
                .Where(c => c.IsComputed || c.DefaultValue.IsSome)
                .Select(c => BuildTableColumnPropertyForBuilder(table, c));
            var primaryKeyExpr = table.PrimaryKey
                .Match(
                    pk => new[] { BuildTablePrimaryKeyForBuilder(table, pk) },
                    Array.Empty<InvocationExpressionSyntax>
                );
            var uniqueKeyExprs = table.UniqueKeys.Select(uk => BuildTableUniqueKeyForBuilder(table, uk));
            var indexExprs = table.Indexes.Select(i => BuildTableIndexForBuilder(table, i));
            var foreignKeyExprs = table.ParentKeys.Select(fk => BuildTableChildKeyForBuilder(table, fk));

            return columnExprs
                .Concat(primaryKeyExpr)
                .Concat(uniqueKeyExprs)
                .Concat(indexExprs)
                .Concat(foreignKeyExprs)
                .ToList();
        }

        private InvocationExpressionSyntax BuildViewConfiguration(IDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var schemaNamespace = NameTranslator.SchemaToNamespace(view.Name);
            var className = NameTranslator.ViewToClassName(view.Name);
            var qualifiedClassName = !schemaNamespace.IsNullOrWhiteSpace()
                ? schemaNamespace + "." + className
                : className;

            var entity = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("modelBuilder"),
                    GenericName(
                        Identifier(nameof(Microsoft.EntityFrameworkCore.ModelBuilder.Entity)))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList(
                                    ParseTypeName(qualifiedClassName))))));

            var hasNoKey = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    entity,
                    IdentifierName(nameof(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder.HasNoKey))));

            var toViewArgs = new List<ArgumentSyntax>
            {
                Argument(
                    LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        Literal(view.Name.LocalName)))
            };
            if (!view.Name.Schema.IsNullOrWhiteSpace())
            {
                var schemaArg = Argument(
                    LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        Literal(view.Name.Schema)));
                toViewArgs.Add(schemaArg);
            }

            return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    hasNoKey,
                    IdentifierName(nameof(Microsoft.EntityFrameworkCore.RelationalEntityTypeBuilderExtensions.ToView))))
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList(toViewArgs)));
        }

        private InvocationExpressionSyntax BuildTableColumnPropertyForBuilder(IRelationalDatabaseTable table, IDatabaseColumn column)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var schemaNamespace = NameTranslator.SchemaToNamespace(table.Name);
            var className = NameTranslator.TableToClassName(table.Name);
            var qualifiedClassName = !schemaNamespace.IsNullOrWhiteSpace()
                ? schemaNamespace + "." + className
                : className;

            var entity = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("modelBuilder"),
                    GenericName(
                        Identifier(nameof(Microsoft.EntityFrameworkCore.ModelBuilder.Entity)))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList(
                                    ParseTypeName(qualifiedClassName))))));

            var propertyName = NameTranslator.ColumnToPropertyName(className, column.Name.LocalName);
            var property = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    entity,
                    IdentifierName(nameof(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder.Property))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                SimpleLambdaExpression(
                                    Parameter(
                                        Identifier("t")),
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("t"),
                                        IdentifierName(propertyName)))))));

            column.DefaultValue.IfSome(def =>
            {
                property = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        property,
                        IdentifierName(nameof(Microsoft.EntityFrameworkCore.RelationalPropertyBuilderExtensions.HasDefaultValue))))
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal(def))))));
            });

            if (column.IsComputed && column is IDatabaseComputedColumn computedColumn)
            {
                computedColumn.Definition.IfSome(def =>
                {
                    property = InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            property,
                            IdentifierName(nameof(Microsoft.EntityFrameworkCore.RelationalPropertyBuilderExtensions.HasComputedColumnSql))))
                        .WithArgumentList(
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(
                                        LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            Literal(def))))));
                });
            }

            return property;
        }

        private InvocationExpressionSyntax BuildTablePrimaryKeyForBuilder(IRelationalDatabaseTable table, IDatabaseKey primaryKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (primaryKey == null)
                throw new ArgumentNullException(nameof(primaryKey));

            var schemaNamespace = NameTranslator.SchemaToNamespace(table.Name);
            var className = NameTranslator.TableToClassName(table.Name);
            var qualifiedClassName = !schemaNamespace.IsNullOrWhiteSpace()
                ? schemaNamespace + "." + className
                : className;

            var entity = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("modelBuilder"),
                    GenericName(
                        Identifier(nameof(Microsoft.EntityFrameworkCore.ModelBuilder.Entity)))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList(
                                    ParseTypeName(qualifiedClassName))))));

            var pkBuilder = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    entity,
                    IdentifierName(nameof(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder.HasKey))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                GenerateColumnSet(className, primaryKey.Columns)))));

            primaryKey.Name.IfSome(pkName =>
            {
                pkBuilder = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        pkBuilder,
                        IdentifierName(nameof(Microsoft.EntityFrameworkCore.RelationalKeyBuilderExtensions.HasName))))
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal(pkName.LocalName))))));
            });

            return pkBuilder;
        }

        private SimpleLambdaExpressionSyntax GenerateColumnSet(string className, IEnumerable<IDatabaseColumn> columns)
        {
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            var columnsList = columns.ToList();
            if (columnsList.Count == 1)
            {
                var column = columnsList[0];
                var propertyName = NameTranslator.ColumnToPropertyName(className, column.Name.LocalName);

                return SimpleLambdaExpression(
                    Parameter(
                        Identifier("t")),
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("t"),
                            IdentifierName(propertyName)));
            }

            var columnsSet = columns.Select(c =>
            {
                var propertyName = NameTranslator.ColumnToPropertyName(className, c.Name.LocalName);
                return AnonymousObjectMemberDeclarator(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("t"),
                            IdentifierName(propertyName)));
            }).ToList();

            return SimpleLambdaExpression(
                Parameter(
                    Identifier("t")),
                    AnonymousObjectCreationExpression(
                        SeparatedList(columnsSet)));
        }

        private InvocationExpressionSyntax BuildTableIndexForBuilder(IRelationalDatabaseTable table, IDatabaseIndex index)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (index == null)
                throw new ArgumentNullException(nameof(index));

            var columns = index.Columns.SelectMany(c => c.DependentColumns).ToList();
            var schemaNamespace = NameTranslator.SchemaToNamespace(table.Name);
            var className = NameTranslator.TableToClassName(table.Name);
            var qualifiedClassName = !schemaNamespace.IsNullOrWhiteSpace()
                ? schemaNamespace + "." + className
                : className;

            var entity = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("modelBuilder"),
                    GenericName(
                        Identifier(nameof(Microsoft.EntityFrameworkCore.ModelBuilder.Entity)))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList(
                                    ParseTypeName(qualifiedClassName))))));

            var indexBuilder = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    entity,
                    IdentifierName(nameof(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder.HasIndex))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                GenerateColumnSet(className, columns)))));

            if (index.IsUnique)
            {
                indexBuilder = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        indexBuilder,
                        IdentifierName(nameof(Microsoft.EntityFrameworkCore.Metadata.Builders.IndexBuilder.IsUnique))));
            }

            if (index.Name != null)
            {
                indexBuilder = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        indexBuilder,
                        IdentifierName(nameof(Microsoft.EntityFrameworkCore.RelationalIndexBuilderExtensions.HasName))))
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal(index.Name.LocalName))))));
            }

            return indexBuilder;
        }

        private InvocationExpressionSyntax BuildTableUniqueKeyForBuilder(IRelationalDatabaseTable table, IDatabaseKey uniqueKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (uniqueKey == null)
                throw new ArgumentNullException(nameof(uniqueKey));

            var schemaNamespace = NameTranslator.SchemaToNamespace(table.Name);
            var className = NameTranslator.TableToClassName(table.Name);
            var qualifiedClassName = !schemaNamespace.IsNullOrWhiteSpace()
                ? schemaNamespace + "." + className
                : className;

            var entity = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("modelBuilder"),
                    GenericName(
                        Identifier(nameof(Microsoft.EntityFrameworkCore.ModelBuilder.Entity)))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList(
                                    ParseTypeName(qualifiedClassName))))));

            var ukBuilder = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    entity,
                    IdentifierName(nameof(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder.HasAlternateKey))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                GenerateColumnSet(className, uniqueKey.Columns)))));

            uniqueKey.Name.IfSome(ukName =>
            {
                ukBuilder = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ukBuilder,
                        IdentifierName(nameof(Microsoft.EntityFrameworkCore.RelationalKeyBuilderExtensions.HasName))))
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal(ukName.LocalName))))));
            });

            return ukBuilder;
        }

        private InvocationExpressionSyntax BuildTableChildKeyForBuilder(IRelationalDatabaseTable table, IDatabaseRelationalKey relationalKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (relationalKey == null)
                throw new ArgumentNullException(nameof(relationalKey));

            var schemaNamespace = NameTranslator.SchemaToNamespace(table.Name);
            var className = NameTranslator.TableToClassName(table.Name);
            var qualifiedClassName = !schemaNamespace.IsNullOrWhiteSpace()
                ? schemaNamespace + "." + className
                : className;
            var childSetName = className.Pluralize();

            var childKey = relationalKey.ChildKey;
            var parentPropertyName = NameTranslator.TableToClassName(relationalKey.ParentTable);

            var entity = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("modelBuilder"),
                    GenericName(
                        Identifier(nameof(Microsoft.EntityFrameworkCore.ModelBuilder.Entity)))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList(
                                    ParseTypeName(qualifiedClassName))))));

            var parentKeyBuilder = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    entity,
                    IdentifierName(nameof(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder.HasOne))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                SimpleLambdaExpression(
                                    Parameter(
                                        Identifier("t")),
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("t"),
                                        IdentifierName(parentPropertyName)))))));

            parentKeyBuilder = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    parentKeyBuilder,
                    IdentifierName(nameof(Microsoft.EntityFrameworkCore.Metadata.Builders.ReferenceNavigationBuilder.WithMany))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                SimpleLambdaExpression(
                                    Parameter(
                                        Identifier("t")),
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("t"),
                                        IdentifierName(childSetName)))))));

            parentKeyBuilder = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    parentKeyBuilder,
                    IdentifierName(nameof(Microsoft.EntityFrameworkCore.Metadata.Builders.ReferenceCollectionBuilder.HasForeignKey))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                GenerateColumnSet(className, childKey.Columns)))));

            parentKeyBuilder = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    parentKeyBuilder,
                    IdentifierName(nameof(Microsoft.EntityFrameworkCore.Metadata.Builders.ReferenceCollectionBuilder.HasPrincipalKey))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                GenerateColumnSet(className, relationalKey.ParentKey.Columns)))));

            relationalKey.ChildKey.Name.IfSome(childKeyName =>
            {
                parentKeyBuilder = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        parentKeyBuilder,
                        IdentifierName(nameof(Microsoft.EntityFrameworkCore.RelationalForeignKeyBuilderExtensions.HasConstraintName))))
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal(childKeyName.LocalName))))));
            });

            return parentKeyBuilder;
        }

        private static InvocationExpressionSyntax BuildSequenceConfiguration(IDatabaseSequence sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            var hasSequenceArgs = new List<ArgumentSyntax>
            {
                Argument(
                    LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        Literal(sequence.Name.LocalName)))
            };
            if (!sequence.Name.Schema.IsNullOrWhiteSpace())
            {
                var schemaArg = Argument(
                    LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        Literal(sequence.Name.Schema)))
                    .WithNameColon(
                        NameColon(
                            IdentifierName("schema")));
                hasSequenceArgs.Add(schemaArg);
            }

            var hasSequence = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("modelBuilder"),
                    GenericName(
                        Identifier(nameof(Microsoft.EntityFrameworkCore.RelationalModelBuilderExtensions.HasSequence)))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    PredefinedType(Token(SyntaxKind.DecimalKeyword)))))))
                .WithArgumentList(
                    ArgumentList(SeparatedList(hasSequenceArgs)));

            var startsAtArgs = ArgumentList(
                SingletonSeparatedList(
                    Argument(
                        LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            Literal(sequence.Start)))));

            var startsAt = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    hasSequence,
                    IdentifierName(nameof(Microsoft.EntityFrameworkCore.Metadata.Builders.SequenceBuilder.StartsAt))))
                .WithArgumentList(startsAtArgs);

            var incrementsByArgs = ArgumentList(
                SingletonSeparatedList(
                    Argument(
                        LiteralExpression(
                            SyntaxKind.NumericLiteralExpression,
                            Literal(sequence.Increment)))));

            return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    startsAt,
                    IdentifierName(nameof(Microsoft.EntityFrameworkCore.Metadata.Builders.SequenceBuilder.IncrementsBy))))
                .WithArgumentList(incrementsByArgs);
        }
    }
}
