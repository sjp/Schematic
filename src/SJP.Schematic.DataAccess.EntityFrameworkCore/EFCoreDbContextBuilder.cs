using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using LanguageExt;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.DataAccess.CodeGeneration;
using SJP.Schematic.DataAccess.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using StringHashSet = System.Collections.Generic.HashSet<string>;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore;

/// <summary>
/// A builder for generating <see cref="DbContext"/> classes for Entity Framework Core.
/// </summary>
public class EFCoreDbContextBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EFCoreDbContextBuilder"/> class, using <see cref="DefaultContextClassName"/> for the generated class name.
    /// </summary>
    /// <param name="nameTranslator">A name translator.</param>
    /// <param name="baseNamespace">The base namespace.</param>
    /// <exception cref="ArgumentNullException"><paramref name="nameTranslator"/> or <paramref name="baseNamespace"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="baseNamespace"/> is empty or whitespace.</exception>
    public EFCoreDbContextBuilder(INameTranslator nameTranslator, string baseNamespace)
        : this(nameTranslator, baseNamespace, DefaultContextClassName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EFCoreDbContextBuilder"/> class.
    /// </summary>
    /// <param name="nameTranslator">A name translator.</param>
    /// <param name="baseNamespace">The base namespace.</param>
    /// <param name="contextClassName">The name to use for the generated <see cref="DbContext"/> class.</param>
    /// <exception cref="ArgumentNullException"><paramref name="nameTranslator"/>, <paramref name="baseNamespace"/> or <paramref name="contextClassName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="baseNamespace"/> or <paramref name="contextClassName"/> is empty or whitespace.</exception>
    public EFCoreDbContextBuilder(INameTranslator nameTranslator, string baseNamespace, string contextClassName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseNamespace);
        ArgumentException.ThrowIfNullOrWhiteSpace(contextClassName);

        NameTranslator = nameTranslator ?? throw new ArgumentNullException(nameof(nameTranslator));
        Namespace = baseNamespace;
        ContextClassName = contextClassName;
    }

    /// <summary>
    /// The name used for the generated <see cref="DbContext"/> class when no other name is provided.
    /// </summary>
    /// <remarks>Deliberately not <c>AppContext</c>, which would collide with <see cref="AppContext"/> in the generated code.</remarks>
    public const string DefaultContextClassName = "DatabaseContext";

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
    /// The name of the generated <see cref="DbContext"/> class.
    /// </summary>
    /// <value>A string representing a class name.</value>
    public string ContextClassName { get; }

    /// <summary>
    /// Generates source code for a <see cref="DbContext"/>.
    /// </summary>
    /// <param name="tables">A collection of tables in the database.</param>
    /// <param name="views">A collection of views in the database.</param>
    /// <param name="sequences">A collection of sequences in the database.</param>
    /// <returns>A string of source code that represents a <see cref="DbContext"/> definition.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tables"/>, <paramref name="views"/>, or <paramref name="sequences"/> is <see langword="null" />.</exception>
    public string Generate(IEnumerable<IRelationalDatabaseTable> tables, IEnumerable<IDatabaseView> views, IEnumerable<IDatabaseSequence> sequences)
    {
        ArgumentNullException.ThrowIfNull(tables);
        ArgumentNullException.ThrowIfNull(views);
        ArgumentNullException.ThrowIfNull(sequences);

        var namespaceDeclaration = NamespaceDeclaration(ParseName(Namespace));
        var classDeclaration = BuildDbContext(tables, views, sequences);

        var document = CompilationUnit()
            .WithUsings(List(UsingStatements))
            .WithMembers(
                SingletonList<MemberDeclarationSyntax>(
                    namespaceDeclaration
                        .WithMembers(
                            SingletonList<MemberDeclarationSyntax>(classDeclaration))));

        return SyntaxUtilities.FormatSyntaxTree(document);
    }

    private const string SystemNamespace = nameof(System);
    private const string EfCoreNamespace = "Microsoft.EntityFrameworkCore";
    private const string EntityLambdaParameterName = "t";
    private const string ModelBuilderParameterName = "modelBuilder";
    private const string ModelBuilderMethodSummaryComment = "Configure the model that was discovered by convention from the defined entity types.";
    private const string ModelBuilderMethodParamComment = "The builder being used to construct the model for this context.";
    private const string OptionsParameterName = "options";
    private const string OptionsParameterComment = "The options to be used by this context.";

    private static readonly ImmutableArray<string> Namespaces =
    [
        .. new[] { SystemNamespace, EfCoreNamespace }.OrderNamespaces(),
    ];

    private static readonly ImmutableArray<UsingDirectiveSyntax> UsingStatements =
    [
        .. Namespaces.Select(static ns => UsingDirective(ParseName(ns))),
    ];

    private SyntaxTriviaList OnModelCreateComment { get; } = SyntaxUtilities.BuildCommentTriviaWithParams(
        [XmlText(ModelBuilderMethodSummaryComment)],
        new Dictionary<string, IEnumerable<XmlNodeSyntax>>(StringComparer.Ordinal) { [ModelBuilderParameterName] = [XmlText(ModelBuilderMethodParamComment)] }
    );

    private ClassDeclarationSyntax BuildDbContext(IEnumerable<IRelationalDatabaseTable> tables, IEnumerable<IDatabaseView> views, IEnumerable<IDatabaseSequence> sequences)
    {
        ArgumentNullException.ThrowIfNull(tables);
        ArgumentNullException.ThrowIfNull(views);
        ArgumentNullException.ThrowIfNull(sequences);

        var baseClass = BaseList(
            SingletonSeparatedList<BaseTypeSyntax>(
                SimpleBaseType(
                    IdentifierName(nameof(DbContext)))));

        var tablesList = tables.ToList();

        // DbSet properties are not namespace qualified, so objects sharing a local name in
        // different schemas would otherwise declare the same property twice on the context.
        var setNames = new StringHashSet(StringComparer.Ordinal);
        var tableDbSets = tablesList.Select(t => BuildTableDbSet(t, setNames)).ToList();
        var viewDbSets = views.Select(v => BuildViewDbSet(v, setNames)).ToList();
        var modelBuilderMethod = BuildOnModelCreatingMethod(tablesList, views, sequences);
        var members = BuildConstructors()
            .Concat(tableDbSets)
            .Concat(viewDbSets)
            .Concat(new MemberDeclarationSyntax[] { modelBuilderMethod })
            .ToList();

        return ClassDeclaration(ContextClassName)
            .WithBaseList(baseClass)
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .WithMembers(List(members));
    }

    private IEnumerable<MemberDeclarationSyntax> BuildConstructors()
    {
        var defaultCtor = ConstructorDeclaration(Identifier(ContextClassName))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
            .WithBody(Block())
            .WithLeadingTrivia(SyntaxUtilities.BuildCommentTrivia(BuildConstructorSummary()));

        var optionsType = GenericName(
            Identifier(nameof(DbContextOptions<DbContext>)),
            TypeArgumentList(
                SingletonSeparatedList<TypeSyntax>(
                    IdentifierName(ContextClassName))));

        var optionsCtor = ConstructorDeclaration(Identifier(ContextClassName))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
            .WithParameterList(
                ParameterList(
                    SingletonSeparatedList(
                        Parameter(Identifier(OptionsParameterName))
                            .WithType(optionsType))))
            .WithInitializer(
                ConstructorInitializer(
                    SyntaxKind.BaseConstructorInitializer,
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                IdentifierName(OptionsParameterName))))))
            .WithBody(Block())
            .WithLeadingTrivia(
                SyntaxUtilities.BuildCommentTriviaWithParams(
                    BuildConstructorSummary(),
                    new Dictionary<string, IEnumerable<XmlNodeSyntax>>(StringComparer.Ordinal) { [OptionsParameterName] = [XmlText(OptionsParameterComment)] }));

        return [defaultCtor, optionsCtor];
    }

    private XmlNodeSyntax[] BuildConstructorSummary() =>
    [
        XmlText("Initializes a new instance of the "),
        XmlElement("c", SingletonList<XmlNodeSyntax>(SyntaxUtilities.BuildXmlText(ContextClassName))),
        XmlText(" class."),
    ];

    private PropertyDeclarationSyntax BuildTableDbSet(IRelationalDatabaseTable table, StringHashSet setNames)
    {
        ArgumentNullException.ThrowIfNull(table);

        var schemaNamespace = NameTranslator.SchemaToNamespace(table.Name);
        var className = NameTranslator.TableToClassName(table.Name);
        var qualifiedClassName = !schemaNamespace.IsNullOrWhiteSpace()
            ? schemaNamespace + "." + className
            : className;
        var setName = UniqueNameGenerator.GenerateUniqueName(setNames, className.Pluralize());
        var qualifiedTableName = !table.Name.Schema.IsNullOrWhiteSpace()
            ? table.Name.Schema + "." + table.Name.LocalName
            : table.Name.LocalName;

        return BuildDbSetProperty(qualifiedClassName, setName, qualifiedTableName, "table");
    }

    private PropertyDeclarationSyntax BuildViewDbSet(IDatabaseView view, StringHashSet setNames)
    {
        ArgumentNullException.ThrowIfNull(view);

        var schemaNamespace = NameTranslator.SchemaToNamespace(view.Name);
        var className = NameTranslator.ViewToClassName(view.Name);
        var qualifiedClassName = !schemaNamespace.IsNullOrWhiteSpace()
            ? schemaNamespace + "." + className
            : className;
        var setName = UniqueNameGenerator.GenerateUniqueName(setNames, className.Pluralize());
        var qualifiedViewName = !view.Name.Schema.IsNullOrWhiteSpace()
            ? view.Name.Schema + "." + view.Name.LocalName
            : view.Name.LocalName;

        return BuildDbSetProperty(qualifiedClassName, setName, qualifiedViewName, "view");
    }

    private static PropertyDeclarationSyntax BuildDbSetProperty(string typeArgument, string propertyName, string qualifiedTargetName, string objectType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeArgument);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        ArgumentException.ThrowIfNullOrWhiteSpace(qualifiedTargetName);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectType);

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
        ArgumentException.ThrowIfNullOrWhiteSpace(targetName);
        ArgumentException.ThrowIfNullOrWhiteSpace(objectType);

        return SyntaxUtilities.BuildCommentTrivia(
        [
            XmlText("Accesses the "),
            XmlElement("c", SingletonList<XmlNodeSyntax>(SyntaxUtilities.BuildXmlText(targetName))),
            XmlText(" " + objectType + "."),
        ]);
    }

    private MethodDeclarationSyntax BuildOnModelCreatingMethod(IEnumerable<IRelationalDatabaseTable> tables, IEnumerable<IDatabaseView> views, IEnumerable<IDatabaseSequence> sequences)
    {
        ArgumentNullException.ThrowIfNull(tables);
        ArgumentNullException.ThrowIfNull(views);
        ArgumentNullException.ThrowIfNull(sequences);

        var navigationResolver = new EFCoreNavigationResolver(NameTranslator, tables);
        var tableConfigs = tables.SelectMany(t => BuildTableConfiguration(t, navigationResolver));
        var viewConfigs = views.Select(BuildViewConfiguration);
        var sequenceConfigs = sequences.Select(BuildSequenceConfiguration);
        var expressions = tableConfigs
            .Concat(viewConfigs)
            .Concat(sequenceConfigs)
            .Select(static invExpr => ExpressionStatement(invExpr))
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

    private IEnumerable<InvocationExpressionSyntax> BuildTableConfiguration(IRelationalDatabaseTable table, EFCoreNavigationResolver navigationResolver)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(navigationResolver);

        var columnExprs = table.GetMappedColumns()
            .Where(static c => c.IsComputed || c.Default.IsSome || c.AutoIncrement.IsSome)
            .Select(c => BuildTableColumnPropertyForBuilder(table, c));
        var primaryKeyExpr = table.PrimaryKey
            .Match(
                pk => [BuildTablePrimaryKeyForBuilder(table, pk)],
                Array.Empty<InvocationExpressionSyntax>
            );
        var uniqueKeyExprs = table.UniqueKeys.Select(uk => BuildTableUniqueKeyForBuilder(table, uk));
        var indexExprs = table.Indexes.Select(i => BuildTableIndexForBuilder(table, i));
        var foreignKeyExprs = table.ParentKeys
            .Select((fk, i) => BuildTableChildKeyForBuilder(table, fk, navigationResolver.ResolveRelationship(table, i)));

        return columnExprs
            .Concat(primaryKeyExpr)
            .Concat(uniqueKeyExprs)
            .Concat(indexExprs)
            .Concat(foreignKeyExprs)
            .ToList();
    }

    // A default classified as a literal is written as a value rather than as SQL, so that the
    // generated context configures the same constant EF Core would insert. Returns null when the
    // constant cannot be recovered from the text, leaving the caller to pass the expression through.
    private static ExpressionSyntax? TryGetLiteralExpression(string definition)
    {
        var value = definition.Trim();

        // SQL Server reports a default wrapped in parentheses, and a scalar in two pairs of them
        while (value.Length > 1 && value[0] == '(' && value[^1] == ')')
            value = value[1..^1].Trim();

        if (value.Length == 0)
            return null;

        if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
            return LiteralExpression(SyntaxKind.TrueLiteralExpression);
        if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
            return LiteralExpression(SyntaxKind.FalseLiteralExpression);

        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integralValue))
            return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(integralValue));
        if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var decimalValue))
            return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(decimalValue));

        // a national character literal (e.g. N'test') carries the same text
        var quoted = value.Length > 2 && (value[0] == 'N' || value[0] == 'n') && value[1] == '\''
            ? value[1..]
            : value;
        if (quoted.Length >= 2 && quoted[0] == '\'' && quoted[^1] == '\'')
        {
            return LiteralExpression(
                SyntaxKind.StringLiteralExpression,
                Literal(quoted[1..^1].Replace("''", "'", StringComparison.Ordinal)));
        }

        // MySQL reports the value of a literal rather than the SQL that produced it, so what is left
        // is the text of the value itself
        return LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value));
    }

    private InvocationExpressionSyntax BuildTableColumnPropertyForBuilder(IRelationalDatabaseTable table, IDatabaseColumn column)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(column);

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

        column.Default.IfSome(def =>
        {
            // a default of NULL is the absence of a value rather than a value, and EF Core has
            // nothing to configure for it
            if (def.Kind == DefaultValueKind.Null)
                return;

            var literal = def.Kind == DefaultValueKind.Literal
                ? TryGetLiteralExpression(def.Definition)
                : null;

            // A sequence default could be expressed with UseSequence, but that also asks EF Core to
            // create the sequence, which the generated context does not describe, so the expression
            // is passed through as SQL like any other.
            property = literal != null
                ? InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        property,
                        IdentifierName(nameof(RelationalPropertyBuilderExtensions.HasDefaultValue))))
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(literal))))
                : InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        property,
                        IdentifierName(nameof(RelationalPropertyBuilderExtensions.HasDefaultValueSql))))
                    .WithArgumentList(
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal(def.Definition))))));
        });

        column.AutoIncrement.IfSome(autoIncrement =>
        {
            property = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    property,
                    IdentifierName(nameof(PropertyBuilder.ValueGeneratedOnAdd))));

            // As with the index options above, this is a provider-specific extension method
            // (NpgsqlPropertyBuilderExtensions) named rather than referenced. Only PostgreSQL and
            // Oracle report an always-generated identity, so a context generated from any other
            // database never calls it.
            if (autoIncrement.Generation == IdentityGeneration.Always)
            {
                property = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        property,
                        IdentifierName("UseIdentityAlwaysColumn")));
            }
        });

        if (column.IsComputed)
        {
            column.ComputedDefinition.IfSome(def =>
            {
                var arguments = new List<ArgumentSyntax>
                {
                    Argument(
                        LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal(def))),
                };

                // The 'stored' argument is nullable, so it is only supplied when the source database
                // says how the computed values are kept; EF Core otherwise falls back to its
                // provider default.
                if (column.ComputedStorage != ComputedColumnStorage.Unknown)
                {
                    arguments.Add(
                        Argument(
                            LiteralExpression(
                                column.ComputedStorage == ComputedColumnStorage.Stored
                                    ? SyntaxKind.TrueLiteralExpression
                                    : SyntaxKind.FalseLiteralExpression))
                        .WithNameColon(NameColon(IdentifierName("stored"))));
                }

                property = InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        property,
                        IdentifierName(nameof(RelationalPropertyBuilderExtensions.HasComputedColumnSql))))
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList(arguments)));
            });
        }

        return property;
    }

    private InvocationExpressionSyntax BuildTablePrimaryKeyForBuilder(IRelationalDatabaseTable table, IDatabaseKey primaryKey)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(primaryKey);

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
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(index);

        var columns = index.Columns.SelectMany(static c => c.DependentColumns).ToList();
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

        index.FilterDefinition.IfSome(filterDefinition =>
        {
            indexBuilder = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    indexBuilder,
                    IdentifierName(nameof(RelationalIndexBuilderExtensions.HasFilter))))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    Literal(filterDefinition))))));
        });

        // The remaining calls are provider-specific extension methods (SqlServerIndexBuilderExtensions,
        // NpgsqlIndexBuilderExtensions). They are named rather than referenced, because this project
        // only depends on EF Core's relational package. Only the dialect that owns each concept can
        // produce the value that triggers it, so a generated context never calls into a provider that
        // the source database did not come from.
        if (index.IndexType == IndexType.Clustered)
        {
            indexBuilder = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    indexBuilder,
                    IdentifierName("IsClustered")));
        }

        index.FillFactor.IfSome(fillFactor =>
        {
            indexBuilder = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    indexBuilder,
                    IdentifierName("HasFillFactor")))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    Literal(fillFactor))))));
        });

        var accessMethod = GetIndexAccessMethod(index.IndexType);
        if (accessMethod != null)
        {
            indexBuilder = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    indexBuilder,
                    IdentifierName("HasMethod")))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    Literal(accessMethod))))));
        }

        return indexBuilder;
    }

    // The PostgreSQL access method name for an index structure that is not the provider's default,
    // or null when the structure does not name one.
    private static string? GetIndexAccessMethod(IndexType indexType)
    {
        return indexType switch
        {
            IndexType.Gin => "gin",
            IndexType.Gist => "gist",
            IndexType.Brin => "brin",
            _ => null,
        };
    }

    private InvocationExpressionSyntax BuildTableUniqueKeyForBuilder(IRelationalDatabaseTable table, IDatabaseKey uniqueKey)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(uniqueKey);

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

    private InvocationExpressionSyntax BuildTableChildKeyForBuilder(IRelationalDatabaseTable table, IDatabaseRelationalKey relationalKey, RelationshipNavigations navigations)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(relationalKey);
        ArgumentNullException.ThrowIfNull(navigations);

        var schemaNamespace = NameTranslator.SchemaToNamespace(table.Name);
        var className = NameTranslator.TableToClassName(table.Name);
        var qualifiedClassName = !schemaNamespace.IsNullOrWhiteSpace()
            ? schemaNamespace + "." + className
            : className;

        var parentSchemaNamespace = NameTranslator.SchemaToNamespace(relationalKey.ParentTable);
        var parentClassName = NameTranslator.TableToClassName(relationalKey.ParentTable);
        var qualifiedParentClassName = !parentSchemaNamespace.IsNullOrWhiteSpace()
            ? parentSchemaNamespace + "." + parentClassName
            : parentClassName;

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
                                    IdentifierName(navigations.DependentPropertyName)))))));

        // a child key constrained to be unique is generated as a single reference on the parent, not a collection
        var inverseMethodName = navigations.IsOneToOne
            ? nameof(ReferenceNavigationBuilder.WithOne)
            : nameof(ReferenceNavigationBuilder.WithMany);

        parentKeyBuilder = InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                parentKeyBuilder,
                IdentifierName(inverseMethodName)))
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
                                    IdentifierName(navigations.PrincipalPropertyName)))))));

        // one-to-one relationships are symmetric, so the dependent and principal entities must be named explicitly
        parentKeyBuilder = InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                parentKeyBuilder,
                BuildKeyMethodName(nameof(ReferenceCollectionBuilder.HasForeignKey), navigations.IsOneToOne ? qualifiedClassName : null)))
            .WithArgumentList(
                ArgumentList(
                    SingletonSeparatedList(
                        Argument(
                            GenerateColumnSet(className, relationalKey.ChildKey.Columns, false)))));

        parentKeyBuilder = InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                parentKeyBuilder,
                BuildKeyMethodName(nameof(ReferenceCollectionBuilder.HasPrincipalKey), navigations.IsOneToOne ? qualifiedParentClassName : null)))
            .WithArgumentList(
                ArgumentList(
                    SingletonSeparatedList(
                        Argument(
                            GenerateColumnSet(parentClassName, relationalKey.ParentKey.Columns, true)))));

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

    private static SimpleNameSyntax BuildKeyMethodName(string methodName, string? qualifiedEntityName) =>
        qualifiedEntityName == null
            ? IdentifierName(methodName)
            : GenericName(
                Identifier(methodName),
                TypeArgumentList(
                    SingletonSeparatedList(
                        ParseTypeName(qualifiedEntityName))));

    private SimpleLambdaExpressionSyntax GenerateColumnSet(string className, IEnumerable<IDatabaseColumn> columns, bool suppressNullable)
    {
        ArgumentNullException.ThrowIfNull(columns);

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
                        : IdentifierName(EntityLambdaParameterName),
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
                            : IdentifierName(EntityLambdaParameterName),
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
        ArgumentException.ThrowIfNullOrWhiteSpace(qualifiedClassName);

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
        ArgumentNullException.ThrowIfNull(objectName);

        var schemaNamespace = NameTranslator.SchemaToNamespace(objectName);
        var className = NameTranslator.ViewToClassName(objectName);
        return !schemaNamespace.IsNullOrWhiteSpace()
            ? schemaNamespace + "." + className
            : className;
    }

    private InvocationExpressionSyntax BuildViewConfiguration(IDatabaseView view)
    {
        ArgumentNullException.ThrowIfNull(view);

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
                    Literal(view.Name.LocalName))),
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
        ArgumentNullException.ThrowIfNull(sequence);

        var hasSequenceArgs = new List<ArgumentSyntax>
        {
            Argument(
                LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    Literal(sequence.Name.LocalName))),
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
                            SingletonSeparatedList(
                                GetSequenceValueType(sequence.Type))))))
            .WithArgumentList(
                ArgumentList(SeparatedList(hasSequenceArgs)));

        var startsAtArgs = ArgumentList(
            SingletonSeparatedList(
                Argument(
                    LiteralExpression(
                        SyntaxKind.NumericLiteralExpression,
                        Literal(ToBoundaryValue(sequence.Start))))));

        var result = InvocationExpression(
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
                        Literal(ToIncrementsByValue(sequence.Increment))))));

        result = InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                result,
                IdentifierName(nameof(SequenceBuilder.IncrementsBy))))
            .WithArgumentList(incrementsByArgs);

        var builder = result;
        sequence.MinValue.IfSome(min => builder = BuildSequenceBoundary(builder, nameof(SequenceBuilder.HasMin), min));
        sequence.MaxValue.IfSome(max => builder = BuildSequenceBoundary(builder, nameof(SequenceBuilder.HasMax), max));
        result = builder;

        if (sequence.Cycle)
        {
            result = InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    result,
                    IdentifierName(nameof(SequenceBuilder.IsCyclic))));
        }

        return result;
    }

    private static InvocationExpressionSyntax BuildSequenceBoundary(InvocationExpressionSyntax target, string methodName, decimal boundary)
    {
        return InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                target,
                IdentifierName(methodName)))
            .WithArgumentList(
                ArgumentList(
                    SingletonSeparatedList(
                        Argument(
                            LiteralExpression(
                                SyntaxKind.NumericLiteralExpression,
                                Literal(ToBoundaryValue(boundary)))))));
    }

    /// <summary>
    /// Names the type argument for <c>HasSequence&lt;T&gt;()</c>, given the type the database
    /// generates values in. EF Core supports only the integral types and <see cref="decimal"/>,
    /// so a sequence of any other type is configured as the widest integral type it supports.
    /// </summary>
    private static TypeSyntax GetSequenceValueType(IDbType sequenceType)
    {
        var keyword = Type.GetTypeCode(sequenceType.ClrType) switch
        {
            TypeCode.Byte => SyntaxKind.ByteKeyword,
            TypeCode.Int16 => SyntaxKind.ShortKeyword,
            TypeCode.Int32 => SyntaxKind.IntKeyword,
            TypeCode.Decimal => SyntaxKind.DecimalKeyword,
            _ => SyntaxKind.LongKeyword,
        };

        return PredefinedType(Token(keyword));
    }

    // the starting value and the bounds are decimal-valued in the model, but EF Core's sequence
    // builder takes them as long, so a value outside that range is pinned to its nearest end
    private static long ToBoundaryValue(decimal value) =>
        decimal.ToInt64(Math.Clamp(decimal.Truncate(value), long.MinValue, long.MaxValue));

    private static int ToIncrementsByValue(decimal increment) =>
        decimal.ToInt32(Math.Clamp(decimal.Truncate(increment), int.MinValue, int.MaxValue));
}