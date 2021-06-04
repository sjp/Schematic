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

namespace SJP.Schematic.DataAccess.OrmLite
{
    /// <summary>
    /// Generate data access classes for tables for use with OrmLite.
    /// </summary>
    /// <seealso cref="DatabaseTableGenerator" />
    public class OrmLiteTableGenerator : DatabaseTableGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrmLiteTableGenerator"/> class.
        /// </summary>
        /// <param name="fileSystem">A file system.</param>
        /// <param name="nameTranslator">The name translator.</param>
        /// <param name="baseNamespace">The base namespace.</param>
        /// <exception cref="ArgumentNullException"><paramref name="baseNamespace"/> is <c>null</c>, empty, or whitespace.</exception>
        public OrmLiteTableGenerator(IFileSystem fileSystem, INameTranslator nameTranslator, string baseNamespace)
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
        /// Generates source code that enables interoperability with a given database table for OrmLite.
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
                .Union(new[] { "ServiceStack.DataAnnotations" }, StringComparer.Ordinal)
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
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var className = NameTranslator.TableToClassName(table.Name);
            var properties = table.Columns
                .Select(c => BuildColumn(table, c, comment, className))
                .ToList();

            return RecordDeclaration(Token(SyntaxKind.RecordKeyword), className)
                .AddAttributeLists(BuildClassAttributes(table, className).ToArray())
                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.SealedKeyword))
                .WithLeadingTrivia(BuildTableComment(table.Name, comment))
                .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
                .WithMembers(List<MemberDeclarationSyntax>(properties))
                .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));
        }

        private PropertyDeclarationSyntax BuildColumn(IRelationalDatabaseTable table, IDatabaseColumn column, Option<IRelationalDatabaseTableComments> comment, string className)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
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
                .AddAttributeLists(BuildColumnAttributes(table, column, propertyName).ToArray())
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

        private IEnumerable<AttributeListSyntax> BuildClassAttributes(IRelationalDatabaseTable table, string className)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (className.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(className));

            var attributes = new List<AttributeListSyntax>();

            var schemaName = table.Name.Schema;
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

            if (!string.Equals(className, table.Name.LocalName, StringComparison.Ordinal))
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
                                            Literal(table.Name.LocalName))))))));
                attributes.Add(aliasAttribute);
            }

            var multiColumnUniqueKeys = table.UniqueKeys.Where(static uk => uk.Columns.Skip(1).Any()).ToList();
            foreach (var uniqueKey in multiColumnUniqueKeys)
            {
                var columnNames = uniqueKey.Columns
                    .Select(c => NameTranslator.ColumnToPropertyName(className, c.Name.LocalName))
                    .Select(static p => AttributeArgument(
                        InvocationExpression(
                            IdentifierName(
                                Identifier(
                                    TriviaList(),
                                    SyntaxKind.NameOfKeyword,
                                    "nameof",
                                    "nameof",
                                    TriviaList())),
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(
                                        IdentifierName(p)))))));

                var uniqueConstraintAttribute = AttributeList(
                    SingletonSeparatedList(
                        Attribute(
                            SyntaxUtilities.AttributeName(nameof(UniqueConstraintAttribute)),
                            AttributeArgumentList(
                                SeparatedList(columnNames)))));
                attributes.Add(uniqueConstraintAttribute);
            }

            var multiColumnIndexes = table.Indexes.Where(static ix => ix.Columns.Skip(1).Any()).ToList();
            foreach (var index in multiColumnIndexes)
            {
                var indexColumns = index.Columns;
                var dependentColumns = indexColumns.SelectMany(static ic => ic.DependentColumns).ToList();
                if (dependentColumns.Count > indexColumns.Count)
                    continue;

                var attrParams = dependentColumns
                    .Select(c => NameTranslator.ColumnToPropertyName(className, c.Name.LocalName))
                    .Select(static p => AttributeArgument(
                        InvocationExpression(
                            IdentifierName(
                                Identifier(
                                    TriviaList(),
                                    SyntaxKind.NameOfKeyword,
                                    "nameof",
                                    "nameof",
                                    TriviaList())),
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(
                                        IdentifierName(p))))
                        )
                    ))
                    .ToList();

                if (index.IsUnique)
                {
                    var uniqueTrueArgument = AttributeArgument(
                        LiteralExpression(SyntaxKind.TrueLiteralExpression)
                    );
                    attrParams = new[] { uniqueTrueArgument }.Concat(attrParams).ToList();
                }

                var compositeIndexAttribute = AttributeList(
                    SingletonSeparatedList(
                        Attribute(
                            SyntaxUtilities.AttributeName(nameof(CompositeIndexAttribute)),
                            AttributeArgumentList(
                                SeparatedList(attrParams)))));
                attributes.Add(compositeIndexAttribute);
            }

            return attributes;
        }

        private IEnumerable<AttributeListSyntax> BuildColumnAttributes(IRelationalDatabaseTable table, IDatabaseColumn column, string propertyName)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (propertyName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(propertyName));

            var attributes = new List<AttributeListSyntax>();
            var clrType = column.Type.ClrType;

            if (clrType == typeof(string) && column.Type.MaxLength > 0)
            {
                var maxLengthAttribute = AttributeList(
                    SingletonSeparatedList(
                        Attribute(
                            SyntaxUtilities.AttributeName(nameof(StringLengthAttribute)),
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

            var isPrimaryKey = ColumnIsPrimaryKey(table, column);
            if (isPrimaryKey)
            {
                var primaryKeyAttribute = AttributeList(
                    SingletonSeparatedList(
                        Attribute(
                            SyntaxUtilities.AttributeName(nameof(PrimaryKeyAttribute)))));
                attributes.Add(primaryKeyAttribute);
            }

            if (column.AutoIncrement.IsSome)
            {
                var autoIncrementAttribute = AttributeList(
                    SingletonSeparatedList(
                        Attribute(
                            SyntaxUtilities.AttributeName(nameof(AutoIncrementAttribute)))));
                attributes.Add(autoIncrementAttribute);
            }

            var isNonUniqueIndex = ColumnIsNonUniqueIndex(table, column);
            var isUniqueIndex = ColumnIsUniqueIndex(table, column);
            var isIndex = isNonUniqueIndex || isUniqueIndex;
            if (isIndex)
            {
                var indexAttribute = Attribute(
                    SyntaxUtilities.AttributeName(nameof(IndexAttribute))
                );
                var indexAttributeList = AttributeList(SingletonSeparatedList(indexAttribute));

                if (isNonUniqueIndex)
                {
                    attributes.Add(indexAttributeList);
                }
                else
                {
                    var uniqueIndexAttribute = indexAttribute
                        .WithArgumentList(
                            AttributeArgumentList(
                                SingletonSeparatedList(
                                    AttributeArgument(
                                        LiteralExpression(SyntaxKind.TrueLiteralExpression)))));

                    var uniqueIndexAttributeList = AttributeList(SingletonSeparatedList(uniqueIndexAttribute));
                    attributes.Add(uniqueIndexAttributeList);
                }
            }

            var isUniqueKey = ColumnIsUniqueKey(table, column);
            if (isUniqueKey)
            {
                var uniqueKeyAttribute = AttributeList(
                    SingletonSeparatedList(
                        Attribute(
                            SyntaxUtilities.AttributeName(nameof(UniqueAttribute)))));
                attributes.Add(uniqueKeyAttribute);
            }

            column.DefaultValue.IfSome(def =>
            {
                var defaultAttribute = AttributeList(
                    SingletonSeparatedList(
                        Attribute(
                            SyntaxUtilities.AttributeName(nameof(DefaultAttribute)),
                            AttributeArgumentList(
                                SingletonSeparatedList(
                                    AttributeArgument(
                                        LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            Literal(def))))))));
                attributes.Add(defaultAttribute);
            });

            var isForeignKey = ColumnIsForeignKey(table, column);
            if (isForeignKey)
            {
                var relationalKey = ColumnRelationalKey(table, column);
                if (relationalKey == null)
                    throw new InvalidOperationException("Could not find parent key for foreign key relationship. Expected to find one for " + column.Name.LocalName + "." + column.Name.LocalName);

                var parentTable = relationalKey.ParentTable;
                var parentClassName = NameTranslator.TableToClassName(parentTable);
                // TODO check that this is not implicit -- i.e. there is a naming convention applied
                //      so explicitly declaring via [References(...)] may not be necessary

                var fkAttributeArgs = new List<AttributeArgumentSyntax>
                {
                    AttributeArgument(TypeOfExpression(ParseTypeName(parentClassName)))
                };

                relationalKey.ChildKey.Name.IfSome(fkName =>
                {
                    var foreignKeyNameArg = AttributeArgument(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName(nameof(ForeignKeyAttribute.ForeignKeyName)),
                            Token(SyntaxKind.EqualsToken),
                            LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal(fkName.LocalName))));
                    fkAttributeArgs.Add(foreignKeyNameArg);
                });

                if (relationalKey.DeleteAction != ReferentialAction.NoAction)
                {
                    var deleteAction = ForeignKeyAction[relationalKey.DeleteAction];
                    var foreignKeyOnDeleteArg = AttributeArgument(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName(nameof(ForeignKeyAttribute.OnDelete)),
                            Token(SyntaxKind.EqualsToken),
                            LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal(deleteAction))));
                    fkAttributeArgs.Add(foreignKeyOnDeleteArg);
                }

                if (relationalKey.UpdateAction != ReferentialAction.NoAction)
                {
                    var updateAction = ForeignKeyAction[relationalKey.UpdateAction];
                    var foreignKeyOnUpdateArg = AttributeArgument(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName(nameof(ForeignKeyAttribute.OnUpdate)),
                            Token(SyntaxKind.EqualsToken),
                            LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal(updateAction))));
                    fkAttributeArgs.Add(foreignKeyOnUpdateArg);
                }

                var foreignKeyAttribute = AttributeList(
                    SingletonSeparatedList(
                        Attribute(
                            SyntaxUtilities.AttributeName(nameof(ForeignKeyAttribute)),
                            AttributeArgumentList(SeparatedList(fkAttributeArgs)))));
                attributes.Add(foreignKeyAttribute);
            }

            if (!string.Equals(propertyName, column.Name.LocalName, StringComparison.Ordinal))
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
                                            Literal(column.Name.LocalName))))))));
                attributes.Add(aliasAttribute);
            }

            return attributes;
        }

        /// <summary>
        /// Determines whether a given column is a primary key column for the table.
        /// </summary>
        /// <param name="table">A database table.</param>
        /// <param name="column">A column in the given table.</param>
        /// <returns><c>true</c> if the column is a primary key column; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c> or <paramref name="column"/> is <c>null</c>.</exception>
        protected static bool ColumnIsPrimaryKey(IRelationalDatabaseTable table, IDatabaseColumn column)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return table.PrimaryKey
                .Where(pk => pk.Columns.Count == 1
                    && string.Equals(column.Name.LocalName, pk.Columns.First().Name.LocalName, StringComparison.Ordinal))
                .IsSome;
        }

        /// <summary>
        /// Determines whether a given column is a foreign key column for the table.
        /// </summary>
        /// <param name="table">A database table.</param>
        /// <param name="column">A column in the given table.</param>
        /// <returns><c>true</c> if the column is a foreign key column; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c> or <paramref name="column"/> is <c>null</c>.</exception>
        protected static bool ColumnIsForeignKey(IRelationalDatabaseTable table, IDatabaseColumn column)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var foreignKeys = table.ParentKeys;
            if (foreignKeys.Empty())
                return false;

            foreach (var foreignKey in foreignKeys)
            {
                if (foreignKey.ParentKey.KeyType != DatabaseKeyType.Primary)
                    continue; // ormlite only supports FK to primary key

                var childColumns = foreignKey.ChildKey.Columns;
                if (childColumns.Count > 1)
                    continue;

                var childColumn = childColumns.First();
                if (string.Equals(childColumn.Name.LocalName, column.Name.LocalName, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves the relational key associated with a given column, if available.
        /// </summary>
        /// <param name="table">A database table.</param>
        /// <param name="column">A column within the given table.</param>
        /// <returns>A relational key, if available, otherwise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="table"/> or <paramref name="column"/> are <c>null</c>.</exception>
        protected static IDatabaseRelationalKey? ColumnRelationalKey(IRelationalDatabaseTable table, IDatabaseColumn column)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var foreignKeys = table.ParentKeys;
            if (foreignKeys.Empty())
                return null;

            foreach (var foreignKey in foreignKeys)
            {
                if (foreignKey.ParentKey.KeyType != DatabaseKeyType.Primary)
                    continue; // ormlite only supports FK to primary key

                var childColumns = foreignKey.ChildKey.Columns;
                if (childColumns.Count > 1)
                    continue;

                var childColumn = childColumns.First();
                if (string.Equals(childColumn.Name.LocalName, column.Name.LocalName, StringComparison.Ordinal))
                    return foreignKey;
            }

            return null;
        }

        /// <summary>
        /// Determines whether a given column is a non-unique index column for the table.
        /// </summary>
        /// <param name="table">A database table.</param>
        /// <param name="column">A column in the given table.</param>
        /// <returns><c>true</c> if the column is a non-unique index column; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c> or <paramref name="column"/> is <c>null</c>.</exception>
        protected static bool ColumnIsNonUniqueIndex(IRelationalDatabaseTable table, IDatabaseColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var indexes = table.Indexes.Where(static i => !i.IsUnique).ToList();
            if (indexes.Empty())
                return false;

            foreach (var index in indexes)
            {
                var columns = index.Columns;
                if (columns.Count > 1)
                    continue;

                var indexColumn = columns.First();
                var dependentColumns = indexColumn.DependentColumns;
                if (dependentColumns.Count > 1)
                    continue;

                var dependentColumn = dependentColumns[0];
                if (string.Equals(dependentColumn.Name.LocalName, column.Name.LocalName, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether a given column is a unique index column for the table.
        /// </summary>
        /// <param name="table">A database table.</param>
        /// <param name="column">A column in the given table.</param>
        /// <returns><c>true</c> if the column is a unique index column; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c> or <paramref name="column"/> is <c>null</c>.</exception>
        protected static bool ColumnIsUniqueIndex(IRelationalDatabaseTable table, IDatabaseColumn column)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var indexes = table.Indexes.Where(static i => i.IsUnique).ToList();
            if (indexes.Empty())
                return false;

            foreach (var index in indexes)
            {
                var columns = index.Columns;
                if (columns.Count > 1)
                    continue;

                var indexColumn = columns.First();
                var dependentColumns = indexColumn.DependentColumns;
                if (dependentColumns.Count > 1)
                    continue;

                var dependentColumn = dependentColumns[0];
                if (string.Equals(dependentColumn.Name.LocalName, column.Name.LocalName, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether a given column is unique key column for the table.
        /// </summary>
        /// <param name="table">A database table.</param>
        /// <param name="column">A column in the given table.</param>
        /// <returns><c>true</c> if the column is a unique key column; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="table"/> is <c>null</c> or <paramref name="column"/> is <c>null</c>.</exception>
        protected static bool ColumnIsUniqueKey(IRelationalDatabaseTable table, IDatabaseColumn column)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var uniqueKeys = table.UniqueKeys;
            if (uniqueKeys.Empty())
                return false;

            foreach (var uniqueKey in uniqueKeys)
            {
                var ukColumns = uniqueKey.Columns;
                if (ukColumns.Count != 1)
                    continue;

                var ukColumn = ukColumns.First();
                if (string.Equals(column.Name.LocalName, ukColumn.Name.LocalName, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static readonly IReadOnlyDictionary<ReferentialAction, string> ForeignKeyAction = new Dictionary<ReferentialAction, string>
        {
            [ReferentialAction.NoAction] = "NO ACTION",
            [ReferentialAction.Restrict] = "RESTRICT",
            [ReferentialAction.Cascade] = "CASCADE",
            [ReferentialAction.SetDefault] = "SET DEFAULT",
            [ReferentialAction.SetNull] = "SET NULL"
        };
    }
}
