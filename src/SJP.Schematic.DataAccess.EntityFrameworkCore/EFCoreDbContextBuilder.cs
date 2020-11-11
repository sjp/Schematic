using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.DataAccess.CodeGeneration;
using SJP.Schematic.DataAccess.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore
{
    /// <summary>
    /// A builder for generating <see cref="DbContext"/> classes for Entity Framework Core.
    /// </summary>
    public class EFCoreDbContextBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EFCoreDbContextBuilder"/> class.
        /// </summary>
        /// <param name="nameTranslator">A name translator.</param>
        /// <param name="baseNamespace">The base namespace.</param>
        /// <exception cref="ArgumentNullException"><paramref name="nameTranslator"/> is <c>null</c>, or <paramref name="baseNamespace"/> is <c>null</c>, empty or whitespace.</exception>
        public EFCoreDbContextBuilder(INameTranslator nameTranslator, string baseNamespace)
        {
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            NameTranslator = nameTranslator ?? throw new ArgumentNullException(nameof(nameTranslator));
            Namespace = baseNamespace;
        }

        /// <summary>
        /// The name translator when translating database object names to C# object names.
        /// </summary>
        /// <value>A name translator.</value>
        protected INameTranslator NameTranslator { get; }

        /// <summary>
        /// The namespace to use for the <see cref="DbContext"/> class.
        /// </summary>
        /// <value>A string representing a namespace.</value>
        protected string Namespace { get; }

        /// <summary>
        /// Generates source code for a <see cref="DbContext"/>.
        /// </summary>
        /// <param name="tables">A collection of tables in the database.</param>
        /// <param name="views">A collection of views in the database.</param>
        /// <param name="sequences">A collection of sequences in the database.</param>
        /// <returns>A string of source code that represents a <see cref="DbContext"/> definition.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tables"/>, <paramref name="views"/>, or <paramref name="sequences"/> is <c>null</c>.</exception>
        public string Generate(IEnumerable<IRelationalDatabaseTable> tables, IEnumerable<IDatabaseView> views, IEnumerable<IDatabaseSequence> sequences)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));
            if (views == null)
                throw new ArgumentNullException(nameof(views));
            if (sequences == null)
                throw new ArgumentNullException(nameof(sequences));

            var namespaceDeclaration = NamespaceDeclaration(ParseName(Namespace));
            var classDeclaration = BuildDbContext(tables, views, sequences);

            var document = CompilationUnit()
                .WithUsings(List(UsingStatements))
                .WithMembers(
                    SingletonList<MemberDeclarationSyntax>(
                        namespaceDeclaration
                            .WithMembers(
                                SingletonList<MemberDeclarationSyntax>(classDeclaration))));

            using var workspace = new AdhocWorkspace();
            return Formatter.Format(document, workspace).ToFullString();
        }

        private const string SystemNamespace = nameof(System);
        private const string EfCoreNamespace = "Microsoft.EntityFrameworkCore";
        private const string EntityLambdaParameterName = "t";
        private const string ModelBuilderParameterName = "modelBuilder";
        private const string ModelBuilderMethodSummaryComment = "Configure the model that was discovered by convention from the defined entity types.";
        private const string ModelBuilderMethodParamComment = "The builder being used to construct the model for this context.";

        private static readonly IEnumerable<string> Namespaces = new[]
            {
                SystemNamespace,
                EfCoreNamespace
            }
            .OrderNamespaces()
            .ToList();

        private static readonly IEnumerable<UsingDirectiveSyntax> UsingStatements = Namespaces
            .Select(ns => ParseName(ns))
            .Select(UsingDirective)
            .ToList();

        private SyntaxTriviaList OnModelCreateComment { get; } = SyntaxUtilities.BuildCommentTriviaWithParams(
            new[] { XmlText(ModelBuilderMethodSummaryComment) },
            new Dictionary<string, IEnumerable<XmlNodeSyntax>>(StringComparer.Ordinal) { [ModelBuilderParameterName] = new[] { XmlText(ModelBuilderMethodParamComment) } }
        );

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
                        IdentifierName(nameof(DbContext)))));

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
                Identifier(nameof(DbSet<object>)),
                TypeArgumentList(
                    SingletonSeparatedList(
                        ParseTypeName(typeArgument))));

            return PropertyDeclaration(dbSetType, Identifier(propertyName))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
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
                .WithModifiers(TokenList(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.OverrideKeyword)))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(Identifier(ModelBuilderParameterName))
                                .WithType(
                                    IdentifierName(nameof(ModelBuilder))))))
                .WithBody(Block(expressions))
                .WithLeadingTrivia(OnModelCreateComment);
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

            var entity = GetEntityBuilder(qualifiedClassName);
            var propertyName = NameTranslator.ColumnToPropertyName(className, column.Name.LocalName);
            var property = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    entity,
                    IdentifierName(nameof(EntityTypeBuilder.Property))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                SimpleLambdaExpression(
                                    Parameter(
                                        Identifier(EntityLambdaParameterName)),
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName(EntityLambdaParameterName),
                                        IdentifierName(propertyName)))))));

            column.DefaultValue.IfSome(def =>
            {
                property = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        property,
                        IdentifierName(nameof(RelationalPropertyBuilderExtensions.HasDefaultValue))))
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
                            IdentifierName(nameof(RelationalPropertyBuilderExtensions.HasComputedColumnSql))))
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

            var entity = GetEntityBuilder(qualifiedClassName);
            var pkBuilder = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    entity,
                    IdentifierName(nameof(EntityTypeBuilder.HasKey))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                GenerateColumnSet(className, primaryKey.Columns, false)))));

            primaryKey.Name.IfSome(pkName =>
            {
                pkBuilder = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        pkBuilder,
                        IdentifierName(nameof(RelationalKeyBuilderExtensions.HasName))))
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

            var entity = GetEntityBuilder(qualifiedClassName);
            var indexBuilder = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    entity,
                    IdentifierName(nameof(EntityTypeBuilder.HasIndex))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                GenerateColumnSet(className, columns, false)))));

            if (index.IsUnique)
            {
                indexBuilder = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        indexBuilder,
                        IdentifierName(nameof(IndexBuilder.IsUnique))));
            }

            if (index.Name != null)
            {
                indexBuilder = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        indexBuilder,
                        IdentifierName(nameof(RelationalIndexBuilderExtensions.HasDatabaseName))))
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

            var entity = GetEntityBuilder(qualifiedClassName);
            var ukBuilder = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    entity,
                    IdentifierName(nameof(EntityTypeBuilder.HasAlternateKey))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                GenerateColumnSet(className, uniqueKey.Columns, false)))));

            uniqueKey.Name.IfSome(ukName =>
            {
                ukBuilder = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ukBuilder,
                        IdentifierName(nameof(RelationalKeyBuilderExtensions.HasName))))
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

            var entity = GetEntityBuilder(qualifiedClassName);
            var parentKeyBuilder = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    entity,
                    IdentifierName(nameof(EntityTypeBuilder.HasOne))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                SimpleLambdaExpression(
                                    Parameter(
                                        Identifier(EntityLambdaParameterName)),
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName(EntityLambdaParameterName),
                                        IdentifierName(parentPropertyName)))))));

            parentKeyBuilder = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    parentKeyBuilder,
                    IdentifierName(nameof(ReferenceNavigationBuilder.WithMany))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                SimpleLambdaExpression(
                                    Parameter(
                                        Identifier(EntityLambdaParameterName)),
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        PostfixUnaryExpression(
                                            SyntaxKind.SuppressNullableWarningExpression,
                                            IdentifierName(EntityLambdaParameterName)),
                                        IdentifierName(childSetName)))))));

            parentKeyBuilder = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    parentKeyBuilder,
                    IdentifierName(nameof(ReferenceCollectionBuilder.HasForeignKey))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                GenerateColumnSet(className, childKey.Columns, false)))));

            parentKeyBuilder = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    parentKeyBuilder,
                    IdentifierName(nameof(ReferenceCollectionBuilder.HasPrincipalKey))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                GenerateColumnSet(className, relationalKey.ParentKey.Columns, true)))));

            relationalKey.ChildKey.Name.IfSome(childKeyName =>
            {
                parentKeyBuilder = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        parentKeyBuilder,
                        IdentifierName(nameof(RelationalForeignKeyBuilderExtensions.HasConstraintName))))
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

        private SimpleLambdaExpressionSyntax GenerateColumnSet(string className, IEnumerable<IDatabaseColumn> columns, bool suppressNullable)
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
                        Identifier(EntityLambdaParameterName)),
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        suppressNullable
                            ? PostfixUnaryExpression(
                                SyntaxKind.SuppressNullableWarningExpression,
                                IdentifierName(EntityLambdaParameterName))
                            : IdentifierName(EntityLambdaParameterName) as ExpressionSyntax,
                        IdentifierName(propertyName)));
            }

            var columnsSet = columns
                .Select(c =>
                    AnonymousObjectMemberDeclarator(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            suppressNullable
                                ? PostfixUnaryExpression(
                                    SyntaxKind.SuppressNullableWarningExpression,
                                    IdentifierName(EntityLambdaParameterName))
                                : IdentifierName(EntityLambdaParameterName) as ExpressionSyntax,
                            IdentifierName(NameTranslator.ColumnToPropertyName(className, c.Name.LocalName)))))
                .ToList();

            return SimpleLambdaExpression(
                Parameter(
                    Identifier(EntityLambdaParameterName)),
                AnonymousObjectCreationExpression(
                    SeparatedList(columnsSet)));
        }

        private static InvocationExpressionSyntax GetEntityBuilder(string qualifiedClassName)
        {
            if (string.IsNullOrWhiteSpace(qualifiedClassName))
                throw new ArgumentNullException(nameof(qualifiedClassName));

            return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(ModelBuilderParameterName),
                    GenericName(
                        Identifier(nameof(ModelBuilder.Entity)))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList(
                                    ParseTypeName(qualifiedClassName))))));
        }

        private string GetQualifiedClassName(Identifier objectName)
        {
            if (objectName == null)
                throw new ArgumentNullException(nameof(objectName));

            var schemaNamespace = NameTranslator.SchemaToNamespace(objectName);
            var className = NameTranslator.ViewToClassName(objectName);
            return !schemaNamespace.IsNullOrWhiteSpace()
                ? schemaNamespace + "." + className
                : className;
        }

        private InvocationExpressionSyntax BuildViewConfiguration(IDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var qualifiedClassName = GetQualifiedClassName(view.Name);
            var entity = GetEntityBuilder(qualifiedClassName);

            var hasNoKey = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    entity,
                    IdentifierName(nameof(EntityTypeBuilder.HasNoKey))));

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
                    IdentifierName(nameof(RelationalEntityTypeBuilderExtensions.ToView))))
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList(toViewArgs)));
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
                    IdentifierName(ModelBuilderParameterName),
                    GenericName(
                        Identifier(nameof(RelationalModelBuilderExtensions.HasSequence)))
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
                    IdentifierName(nameof(SequenceBuilder.StartsAt))))
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
                    IdentifierName(nameof(SequenceBuilder.IncrementsBy))))
                .WithArgumentList(incrementsByArgs);
        }
    }
}
