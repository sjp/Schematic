using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Oracle.Queries;

namespace SJP.Schematic.Oracle;

/// <summary>
/// A query view provider for Oracle.
/// </summary>
/// <seealso cref="IDatabaseViewProvider" />
public class OracleDatabaseQueryViewProvider : IDatabaseViewProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OracleDatabaseQueryViewProvider"/> class.
    /// </summary>
    /// <param name="connection">A schematic connection.</param>
    /// <param name="identifierDefaults">Database identifier defaults.</param>
    /// <param name="identifierResolver">An identifier resolver.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection"/> or <paramref name="identifierDefaults"/> or <paramref name="identifierResolver"/> are <c>null</c>.</exception>
    public OracleDatabaseQueryViewProvider(ISchematicConnection connection, IIdentifierDefaults identifierDefaults, IIdentifierResolutionStrategy identifierResolver)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        IdentifierDefaults = identifierDefaults ?? throw new ArgumentNullException(nameof(identifierDefaults));
        IdentifierResolver = identifierResolver ?? throw new ArgumentNullException(nameof(identifierResolver));
    }

    /// <summary>
    /// A database connection that is specific to a given Oracle database.
    /// </summary>
    /// <value>A database connection.</value>
    protected ISchematicConnection Connection { get; }

    /// <summary>
    /// Identifier defaults for the associated database.
    /// </summary>
    /// <value>Identifier defaults.</value>
    protected IIdentifierDefaults IdentifierDefaults { get; }

    /// <summary>
    /// Gets an identifier resolver that enables more relaxed matching against database object names.
    /// </summary>
    /// <value>An identifier resolver.</value>
    protected IIdentifierResolutionStrategy IdentifierResolver { get; }

    /// <summary>
    /// A database connection factory used to query the database.
    /// </summary>
    /// <value>A database connection factory.</value>
    protected IDbConnectionFactory DbConnection => Connection.DbConnection;

    /// <summary>
    /// The dialect for the associated database.
    /// </summary>
    /// <value>A database dialect.</value>
    protected IDatabaseDialect Dialect => Connection.Dialect;

    /// <summary>
    /// Gets all database views.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of database views.</returns>
    public IAsyncEnumerable<IDatabaseView> GetAllViews(CancellationToken cancellationToken = default)
    {
        return DbConnection.QueryEnumerableAsync<GetAllViewNames.Result>(GetAllViewNames.Sql, cancellationToken)
            .Select(dto => Identifier.CreateQualifiedIdentifier(dto.SchemaName, dto.ViewName))
            .Select(QualifyViewName)
            .SelectAwait(viewName => LoadViewAsyncCore(viewName, cancellationToken).ToValue());
    }

    /// <summary>
    /// Gets a database view.
    /// </summary>
    /// <param name="viewName">A database view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A database view in the 'some' state if found; otherwise 'none'.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    public OptionAsync<IDatabaseView> GetView(Identifier viewName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var candidateViewName = QualifyViewName(viewName);
        return LoadView(candidateViewName, cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the view. This enables non-strict name matching to be applied.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A view name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    protected OptionAsync<Identifier> GetResolvedViewName(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var resolvedNames = IdentifierResolver
            .GetResolutionOrder(viewName)
            .Select(QualifyViewName);

        return resolvedNames
            .Select(name => GetResolvedViewNameStrict(name, cancellationToken))
            .FirstSome(cancellationToken);
    }

    /// <summary>
    /// Gets the resolved name of the view without name resolution. i.e. the name must match strictly to return a result.
    /// </summary>
    /// <param name="viewName">A view name that will be resolved.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A view name that, if available, can be assumed to exist and applied strictly.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    protected OptionAsync<Identifier> GetResolvedViewNameStrict(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var candidateViewName = QualifyViewName(viewName);
        var qualifiedViewName = DbConnection.QueryFirstOrNone(
            GetViewName.Sql,
            new GetViewName.Query { SchemaName = candidateViewName.Schema!, ViewName = candidateViewName.LocalName },
            cancellationToken
        );

        return qualifiedViewName.Map(name => Identifier.CreateQualifiedIdentifier(candidateViewName.Server, candidateViewName.Database, name.SchemaName, name.ViewName));
    }

    /// <summary>
    /// Retrieves a database view, if available.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A view definition, if available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    protected OptionAsync<IDatabaseView> LoadView(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var candidateViewName = QualifyViewName(viewName);
        return GetResolvedViewName(candidateViewName, cancellationToken)
            .MapAsync(name => LoadViewAsyncCore(name, cancellationToken));
    }

    private async Task<IDatabaseView> LoadViewAsyncCore(Identifier viewName, CancellationToken cancellationToken)
    {
        var (columns, definition) = await TaskUtilities.WhenAll(
            LoadColumnsAsync(viewName, cancellationToken),
            LoadDefinitionAsync(viewName, cancellationToken)
        ).ConfigureAwait(false);

        return new DatabaseView(viewName, definition, columns);
    }

    /// <summary>
    /// Retrieves the definition of a view.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A string representing the definition of a view.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    protected Task<string?> LoadDefinitionAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return DbConnection.ExecuteScalarAsync(
            GetViewDefinition.Sql,
            new GetViewDefinition.Query { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        );
    }

    /// <summary>
    /// Retrieves the columns for a given view.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An ordered collection of columns.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    protected Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsync(Identifier viewName, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return LoadColumnsAsyncCore(viewName, cancellationToken);
    }

    private async Task<IReadOnlyList<IDatabaseColumn>> LoadColumnsAsyncCore(Identifier viewName, CancellationToken cancellationToken)
    {
        var query = await DbConnection.QueryAsync(
            GetViewColumns.Sql,
            new GetViewColumns.Query { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        ).ConfigureAwait(false);

        var columnNames = query
            .Where(static row => row.ColumnName != null)
            .Select(static row => row.ColumnName!)
            .ToList();
        var notNullableColumnNames = await GetNotNullConstrainedColumnsAsync(viewName, columnNames, cancellationToken).ConfigureAwait(false);
        var result = new List<IDatabaseColumn>();

        foreach (var row in query)
        {
            var typeMetadata = new ColumnTypeMetadata
            {
                TypeName = Identifier.CreateQualifiedIdentifier(row.ColumnTypeSchema, row.ColumnTypeName),
                Collation = !row.Collation.IsNullOrWhiteSpace()
                    ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.Collation))
                    : Option<Identifier>.None,
                MaxLength = row.DataLength,
                NumericPrecision = row.Precision > 0 || row.Scale > 0
                    ? Option<INumericPrecision>.Some(new NumericPrecision(row.Precision, row.Scale))
                    : Option<INumericPrecision>.None
            };
            var columnType = Dialect.TypeProvider.CreateColumnType(typeMetadata);

            var isNullable = !notNullableColumnNames.Contains(row.ColumnName);
            var columnName = Identifier.CreateQualifiedIdentifier(row.ColumnName);
            var defaultValue = !row.DefaultValue.IsNullOrWhiteSpace()
                 ? Option<string>.Some(row.DefaultValue)
                 : Option<string>.None;

            var column = new OracleDatabaseColumn(columnName, columnType, isNullable, defaultValue);

            result.Add(column);
        }

        return result;
    }

    /// <summary>
    /// Retrieves the names all of the not-null constrained columns in a given view.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <param name="columnNames">The column names for the given view.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of not-null constrained column names.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> or <paramref name="columnNames"/> are <c>null</c>.</exception>
    protected Task<IEnumerable<string>> GetNotNullConstrainedColumnsAsync(Identifier viewName, IEnumerable<string> columnNames, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(viewName);
        ArgumentNullException.ThrowIfNull(columnNames);

        return GetNotNullConstrainedColumnsAsyncCore(viewName, columnNames, cancellationToken);
    }

    private async Task<IEnumerable<string>> GetNotNullConstrainedColumnsAsyncCore(Identifier viewName, IEnumerable<string> columnNames, CancellationToken cancellationToken)
    {
        var checks = await DbConnection.QueryAsync(
            GetViewChecks.Sql,
            new GetViewChecks.Query { SchemaName = viewName.Schema!, ViewName = viewName.LocalName },
            cancellationToken
        ).ConfigureAwait(false);

        if (checks.Empty())
            return [];

        var columnNotNullConstraints = columnNames
            .Select(name => new KeyValuePair<string, string>(GenerateNotNullDefinition(name), name))
            .ToReadOnlyDictionary();

        return checks
            .Where(c => c.Definition != null
                && string.Equals(c.EnabledStatus, EnabledValue, StringComparison.Ordinal)
                && columnNotNullConstraints.ContainsKey(c.Definition))
            .Select(c => columnNotNullConstraints[c.Definition!])
            .ToList();
    }

    /// <summary>
    /// Creates a not null constraint definition, used to determine whether a constraint is a <c>NOT NULL</c> constraint.
    /// </summary>
    /// <param name="columnName">A column name.</param>
    /// <returns>A <c>NOT NULL</c> constraint definition for the given column.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="columnName"/> is <c>null</c>, empty or whitespace.</exception>
    protected static string GenerateNotNullDefinition(string columnName)
    {
        if (columnName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(columnName));

        return "\"" + columnName + "\" IS NOT NULL";
    }

    /// <summary>
    /// Qualifies the name of the view.
    /// </summary>
    /// <param name="viewName">A view name.</param>
    /// <returns>A view name is at least as qualified as the given view name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="viewName"/> is <c>null</c>.</exception>
    protected Identifier QualifyViewName(Identifier viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        var schema = viewName.Schema ?? IdentifierDefaults.Schema;
        return Identifier.CreateQualifiedIdentifier(IdentifierDefaults.Server, IdentifierDefaults.Database, schema, viewName.LocalName);
    }

    private const string EnabledValue = "ENABLED";
}